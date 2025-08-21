using JetBrains.Annotations;
using Systems.Audibility2D.Components;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Audibility2D.Data.Settings;
using Systems.Audibility2D.Data.Tiles;
using Systems.Audibility2D.Jobs;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility
{
    /// <summary>
    ///     Utility class to calculate audibility level from provided data
    /// </summary>
    [BurstCompile] public static partial class AudibilityTools
    {
        public const int LOUDNESS_NONE = 0;
        public const int LOUDNESS_MAX = 160;

        /// <summary>
        ///     Simpler version to handle audibility calculations
        /// </summary>
        /// <param name="audioTilemap">
        ///     Instance of tilemap to calculate audio data from, should contain <see cref="AudioTile"/> objects
        /// </param>
        /// <param name="audioSourceComputeData">
        ///     Reference to handle for Audio Source Data array, automatically allocated as PERSISTENT
        /// </param>
        /// <param name="tileComputeData">
        ///     Reference to handle for Tile Data array, automatically allocated as PERSISTENT
        ///     Also your output array.
        /// </param>
        /// <param name="allocator">Allocation mode for <see cref="tileComputeData"/> and <see cref="audioSourceComputeData"/></param>
        [BurstDiscard] public static void UpdateAudibilityLevel(
            [NotNull] Tilemap audioTilemap,
            ref NativeArray<AudioSourceInfo> audioSourceComputeData,
            ref NativeArray<AudioTileInfo> tileComputeData,
            Allocator allocator = Allocator.Persistent)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");

            GridInfo2D tilemapInfo = audioTilemap.AsGridInfo();

            // Initialize tilemap arrays
            if (!tileComputeData.IsCreated) RefreshTileDataArray(audioTilemap, ref tileComputeData, allocator);

            // Clear audibility levels data
            ClearCurrentAudioLevelsJob clearLevelsJob = new()
            {
                tileComputeData = tileComputeData
            };
            JobHandle waitForClear =
                clearLevelsJob.Schedule(tileComputeData.Length, math.min(tileComputeData.Length, 64));
            waitForClear.Complete();

            // Prepare array of audio sources
            AudibleSound[] sources =
                Object.FindObjectsByType<AudibleSound>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            RefreshAudioSourcesArray(audioTilemap, sources, ref audioSourceComputeData, allocator);

            // Create audibility settings
            AudioSystemSettings audibilitySettings = new AudioSystemSettings(AudibilitySettings.Instance);

            // Handle computation
            UpdateAudibilityLevel(audibilitySettings, tilemapInfo, audioSourceComputeData, ref tileComputeData);
        }

        /// <summary>
        ///     Update audibility level for entire map
        /// </summary>
        ///  ///
        ///  <param name="audibilitySettings">Audibility settings to use</param>
        ///  <param name="tilemapInfo">Tilemap info to update data for</param>
        ///  <param name="audioSourceComputeData">
        ///     Handle for Audio Source Data array, must be filled with proper data
        /// </param>
        /// <param name="tileComputeData">
        ///     Reference to handle for Tile Data array, must be filled with proper data
        ///     Also your output array.
        /// </param>
        [BurstCompile] public static void UpdateAudibilityLevel(
            in AudioSystemSettings audibilitySettings,
            in GridInfo2D tilemapInfo,
            in NativeArray<AudioSourceInfo> audioSourceComputeData,
            ref NativeArray<AudioTileInfo> tileComputeData)
        {
            Assert.AreNotEqual(tilemapInfo, default, "Tilemap info is invalid");
            Assert.IsTrue(audioSourceComputeData.IsCreated, "Audio source data is invalid");
            Assert.IsTrue(tileComputeData.IsCreated, "Audio tile data is invalid");
            Assert.AreEqual(tilemapInfo.size.x * tilemapInfo.size.y, tileComputeData.Length,
                "Invalid tile data length");

            UpdateAudibilityForAudioSourceJob updateAudibilityJob = new()
            {
                audibilitySettings = audibilitySettings,
                tilemapInfo = tilemapInfo,
                audioSourcesData = audioSourceComputeData,
                audioTilesData = tileComputeData
            };

            updateAudibilityJob
                .Schedule(audioSourceComputeData.Length, math.min(audioSourceComputeData.Length, 64))
                .Complete();
        }

        /// <summary>
        ///     Method that computes audio updates for all neighboring tiles of specific tile
        /// </summary>
        /// <param name="audibilitySettings">Audibility settings to use</param>
        /// <param name="tilemapInfo">Tilemap info to compute data for</param>
        /// <param name="tilesToUpdateNeighbours">
        ///     Reference to list containing indices of tiles which should have their neighbours updated
        /// </param>
        /// <param name="audioTilesData">
        ///     Reference to handle for Tile Data array, must be filled with proper data.
        /// </param>
        /// <param name="currentTile">
        ///     Reference to tile which neighbours should be checked.
        /// </param>
        [BurstCompile] internal static void UpdateNeighbourAudioLevelsForTile(
            in AudioSystemSettings audibilitySettings,
            in GridInfo2D tilemapInfo,
            ref NativeList<int> tilesToUpdateNeighbours,
            ref NativeArray<AudioTileInfo> audioTilesData,
            ref AudioTileInfo currentTile)
        {
            Index2D currentTileIndex = currentTile.index;

            // North
            CheckNode(audibilitySettings, currentTileIndex.GetNorthIndex2D(tilemapInfo), ref tilesToUpdateNeighbours,
                ref audioTilesData,
                ref currentTile, tilemapInfo.tileSize.y);

            // South
            CheckNode(audibilitySettings, currentTileIndex.GetSouthIndex2D(tilemapInfo), ref tilesToUpdateNeighbours,
                ref audioTilesData,
                ref currentTile, tilemapInfo.tileSize.y);

            // East
            CheckNode(audibilitySettings, currentTileIndex.GetEastIndex2D(tilemapInfo), ref tilesToUpdateNeighbours,
                ref audioTilesData,
                ref currentTile, tilemapInfo.tileSize.x);

            // West
            CheckNode(audibilitySettings, currentTileIndex.GetWestIndex2D(tilemapInfo), ref tilesToUpdateNeighbours,
                ref audioTilesData,
                ref currentTile, tilemapInfo.tileSize.x);

            // North-west
            CheckNode(audibilitySettings, currentTileIndex.GetNorthWestIndex2D(tilemapInfo),
                ref tilesToUpdateNeighbours,
                ref audioTilesData,
                ref currentTile, tilemapInfo.diagonalDistance);

            // North-east
            CheckNode(audibilitySettings, currentTileIndex.GetNorthEastIndex2D(tilemapInfo),
                ref tilesToUpdateNeighbours,
                ref audioTilesData,
                ref currentTile, tilemapInfo.diagonalDistance);

            // South-west
            CheckNode(audibilitySettings, currentTileIndex.GetSouthWestIndex2D(tilemapInfo),
                ref tilesToUpdateNeighbours,
                ref audioTilesData,
                ref currentTile, tilemapInfo.diagonalDistance);

            // South-east
            CheckNode(audibilitySettings, currentTileIndex.GetSouthEastIndex2D(tilemapInfo),
                ref tilesToUpdateNeighbours,
                ref audioTilesData,
                ref currentTile, tilemapInfo.diagonalDistance);
        }

        [BurstCompile] private static void CheckNode(
            in AudioSystemSettings audibilitySettings,
            int neighbourTileIndex,
            ref NativeList<int> tilesToUpdateNeighbours,
            ref NativeArray<AudioTileInfo> audioTilesData,
            ref AudioTileInfo currentTile,
            in float distance)
        {
            // Early return
            if (Hint.Unlikely(neighbourTileIndex == Index2D.NONE)) return;

            // Process tile
            AudioTileInfo neighbourTile = audioTilesData[neighbourTileIndex];

            // This perfectly handles wall-based sounds because sometimes
            // I am stupid and do weird things
            AudioLoudnessLevel newLoudness = currentTile.currentAudioLevel;
            newLoudness.MuffleBy(currentTile.mufflingStrength);

            UpdateAudioLevelForTile(audibilitySettings,
                ref tilesToUpdateNeighbours, ref neighbourTile, newLoudness,
                distance);
            audioTilesData[neighbourTileIndex] = neighbourTile;
        }

        /// <summary>
        ///     Method used to update audio level of specific tile
        /// </summary>
        /// <param name="audibilitySettings">Audibility settings to use</param>
        /// <param name="tilesToUpdateNeighbours">
        ///     Reference to list containing indices of tiles which should have their neighbours updated
        /// </param>
        /// <param name="neighbouringTile">
        ///     Reference to tile that should be updated.
        /// </param>
        /// <param name="currentAudioLevel">
        ///     Audio level in current tile, passed separately to handle source tiles correctly
        /// </param>
        /// <param name="distance">Distance from last tile</param>
        [BurstCompile] internal static void UpdateAudioLevelForTile(
            in AudioSystemSettings audibilitySettings,
            ref NativeList<int> tilesToUpdateNeighbours,
            ref AudioTileInfo neighbouringTile,
            in AudioLoudnessLevel currentAudioLevel,
            float distance)
        {
            // Copy current audio level and compute muffling 
            AudioLoudnessLevel newTileLevel = currentAudioLevel;

            // TODO: Replace division with multiplication of sound decay rate per meter
            //       that will be stored in settings and provided to this method
            newTileLevel = newTileLevel.MuffleBy(distance * audibilitySettings.soundDecayPerUnit);
            newTileLevel = AudioLoudnessLevel.Max(newTileLevel, neighbouringTile.currentAudioLevel);

            // Detect audio changes to prevent infinite loop
            if (Hint.Likely(neighbouringTile.currentAudioLevel == newTileLevel)) return;

            // Update audio level based on maximum between current level and new one calculated by muffling values
            neighbouringTile.currentAudioLevel = newTileLevel;

            // Notify to update neighbours
            if (Hint.Likely(!tilesToUpdateNeighbours.Contains(neighbouringTile.index.value)))
                tilesToUpdateNeighbours.Add(neighbouringTile.index.value);
        }
    }
}