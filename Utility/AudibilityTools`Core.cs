using JetBrains.Annotations;
using Systems.Audibility2D.Components;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Data.Native.Wrappers;
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

            // Handle computation
            UpdateAudibilityLevel(tilemapInfo, audioSourceComputeData, ref tileComputeData);
        }

        /// <summary>
        ///     Update audibility level for entire map
        /// </summary>
        ///  ///
        ///  <param name="tilemapInfo">Tilemap info to update data for</param>
        ///  <param name="audioSourceComputeData">
        ///     Handle for Audio Source Data array, must be filled with proper data
        /// </param>
        /// <param name="tileComputeData">
        ///     Reference to handle for Tile Data array, must be filled with proper data
        ///     Also your output array.
        /// </param>
        [BurstCompile] public static void UpdateAudibilityLevel(
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
        /// <param name="currentAudioSource">
        ///     Current audio source that is being analyzed
        /// </param>
        [BurstCompile] internal static void UpdateNeighbourAudioLevelsForTile(
            in GridInfo2D tilemapInfo,
            ref NativeList<int> tilesToUpdateNeighbours,
            ref NativeArray<AudioTileInfo> audioTilesData,
            ref AudioTileInfo currentTile,
            in AudioSourceInfo currentAudioSource)
        {
            Index2D currentTileIndex = currentTile.index;

            // North
            CheckNode(currentTileIndex.GetNorthIndex2D(tilemapInfo), ref tilesToUpdateNeighbours, ref audioTilesData,
                ref currentTile, in currentAudioSource, tilemapInfo.tileSize.y);

            // South
            CheckNode(currentTileIndex.GetSouthIndex2D(tilemapInfo), ref tilesToUpdateNeighbours, ref audioTilesData,
                ref currentTile, in currentAudioSource, tilemapInfo.tileSize.y);

            // East
            CheckNode(currentTileIndex.GetEastIndex2D(tilemapInfo), ref tilesToUpdateNeighbours, ref audioTilesData,
                ref currentTile, in currentAudioSource, tilemapInfo.tileSize.x);

            // West
            CheckNode(currentTileIndex.GetWestIndex2D(tilemapInfo), ref tilesToUpdateNeighbours, ref audioTilesData,
                ref currentTile, in currentAudioSource, tilemapInfo.tileSize.x);

            // North-west
            CheckNode(currentTileIndex.GetNorthWestIndex2D(tilemapInfo), ref tilesToUpdateNeighbours, ref audioTilesData,
                ref currentTile, in currentAudioSource, tilemapInfo.diagonalDistance);

            // North-east
            CheckNode(currentTileIndex.GetNorthEastIndex2D(tilemapInfo), ref tilesToUpdateNeighbours, ref audioTilesData,
                ref currentTile, in currentAudioSource, tilemapInfo.diagonalDistance);

            // South-west
            CheckNode(currentTileIndex.GetSouthWestIndex2D(tilemapInfo), ref tilesToUpdateNeighbours, ref audioTilesData,
                ref currentTile, in currentAudioSource, tilemapInfo.diagonalDistance);

            // South-east
            CheckNode(currentTileIndex.GetSouthEastIndex2D(tilemapInfo), ref tilesToUpdateNeighbours, ref audioTilesData,
                ref currentTile, in currentAudioSource, tilemapInfo.diagonalDistance);
        }

        [BurstCompile] private static void CheckNode(
            int neighbourTileIndex,
            ref NativeList<int> tilesToUpdateNeighbours,
            ref NativeArray<AudioTileInfo> audioTilesData,
            ref AudioTileInfo currentTile,
            in AudioSourceInfo currentAudioSource,
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

            UpdateAudioLevelForTile(ref tilesToUpdateNeighbours, ref neighbourTile, currentAudioSource, newLoudness,
                distance);
            audioTilesData[neighbourTileIndex] = neighbourTile;
        }

        /// <summary>
        ///     Method used to update audio level of specific tile
        /// </summary>
        /// <param name="tilesToUpdateNeighbours">
        ///     Reference to list containing indices of tiles which should have their neighbours updated
        /// </param>
        /// <param name="neighbouringTile">
        ///     Reference to tile that should be updated.
        /// </param>
        /// <param name="currentAudioSource">
        ///     Current audio source that is being analyzed
        /// </param>
        /// <param name="currentAudioLevel">
        ///     Audio level in current tile, passed separately to handle source tiles correctly
        /// </param>
        /// <param name="distance">Distance from last tile</param>
        [BurstCompile] internal static void UpdateAudioLevelForTile(
            ref NativeList<int> tilesToUpdateNeighbours,
            ref AudioTileInfo neighbouringTile,
            in AudioSourceInfo currentAudioSource,
            in AudioLoudnessLevel currentAudioLevel,
            float distance)
        {
            // Copy current audio level and compute muffling 
            AudioLoudnessLevel newTileLevel = currentAudioLevel;
            
            // TODO: Replace division with multiplication of sound decay rate per meter
            //       that will be stored in settings and provided to this method
            newTileLevel = newTileLevel.MuffleBy(distance / currentAudioSource.range * LOUDNESS_MAX);
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