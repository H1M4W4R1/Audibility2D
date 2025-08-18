using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Systems.Audibility2D.Components;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Audibility2D.Data.Tiles;
using Systems.Audibility2D.Jobs;
using Systems.Audibility2D.Utility.Internal;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility
{
    /// <summary>
    ///     Additional class to reduce mess in <see cref="AudibilityLevel"/>
    /// </summary>
    [BurstCompile] public static class AudibilityTools
    {
        /// <summary>
        ///     Data about tile muffling levels, cached to improve performance
        /// </summary>
        private static readonly Dictionary<Tilemap, NativeArray<AudioLoudnessLevel>> _tileMufflingLevels = new();

        /// <summary>
        ///     Deactivate audio tilemap to dispose of unused data
        /// </summary>
        /// <param name="audioTilemap">Tilemap to activate</param>
        public static void DeactivateAudioTilemap([NotNull] Tilemap audioTilemap)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");
            
            // Disable tilemap gameObject if not disabled
            if (!CheckIfTilemapIsEnabled(audioTilemap)) audioTilemap.enabled = false;
            
            // Get rid of array
            if (_tileMufflingLevels.TryGetValue(audioTilemap, out NativeArray<AudioLoudnessLevel> array))
                array.Dispose();
        }
        
        /// <summary>
        ///     Activate tilemap if necessary
        /// </summary>
        /// <param name="audioTilemap">Tilemap to activate</param>
        public static void ActivateAudioTilemap([NotNull] Tilemap audioTilemap)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");
            
            // Activate tilemap object
            if (!CheckIfTilemapIsEnabled(audioTilemap))
            {
                audioTilemap.gameObject.SetActive(true);
                audioTilemap.enabled = true;
            }

            EnsureTilemapIsReady(audioTilemap);
        } 
            
        /// <summary>
        ///     Ensures that tilemap is ready to be used in computation analysis
        /// </summary>
        /// <param name="audioTilemap">Tilemap to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EnsureTilemapIsReady([NotNull] Tilemap audioTilemap)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");
            if (!CheckIfTilemapIsReady(audioTilemap)) RefreshTileData(audioTilemap);
        }

        /// <summary>
        ///     Check if tilemap is ready to be used in computation
        /// </summary>
        /// <param name="audioTilemap">Tilemap to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckIfTilemapIsReady([NotNull] Tilemap audioTilemap)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");
            return AudibilitySystem.IsDirty(audioTilemap) && _tileMufflingLevels.ContainsKey(audioTilemap);
        }

        /// <summary>
        ///     Check if tilemap is enabled (required to compute data)
        /// </summary>
        /// <param name="audioTilemap">Tilemap to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckIfTilemapIsEnabled([NotNull] Tilemap audioTilemap)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");
            return audioTilemap.isActiveAndEnabled;
        }

        /// <summary>
        ///     Get average loudness data from results array
        /// </summary>
        /// <param name="audioTilemap">Tilemap to get debug data for</param>
        /// <param name="tileDataAfterComputing">Array with computed tile data</param>
        /// <param name="debugDataArray">
        ///     Output array to store loudness, same length as <see cref="tileDataAfterComputing"/>
        /// </param>
        public static void GetTileDebugData(
            [NotNull] in Tilemap audioTilemap,
            in NativeArray<AudioTileInfo> tileDataAfterComputing,
            ref NativeArray<AudioTileDebugInfo> debugDataArray)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");
            Assert.IsTrue(tileDataAfterComputing.IsCreated, "Tile data results array is not created");
            Assert.IsTrue(debugDataArray.IsCreated, "Debug data array is not created");
            Assert.AreEqual(debugDataArray.Length, tileDataAfterComputing.Length, "Arrays length mismatch");

            TilemapInfo tilemapInfo = new(audioTilemap);

            GetDebugAudioTileDataJob job = new()
            {
                tilemapInfo = tilemapInfo,
                tileData = tileDataAfterComputing,
                audioTileDebugData = debugDataArray
            };

            JobHandle waitHandle = job.Schedule(tileDataAfterComputing.Length,
                math.min(tileDataAfterComputing.Length, 64));
            waitHandle.Complete();
        }

        /// <summary>
        ///     Get average loudness data from results array
        /// </summary>
        /// <param name="tileDataAfterComputing">Array with computed tile data</param>
        /// <param name="averageLoudnessArray">
        ///     Output array to store loudness, same length as <see cref="tileDataAfterComputing"/>
        /// </param>
        public static void GetAverageLoudnessData(
            in NativeArray<AudioTileInfo> tileDataAfterComputing,
            ref NativeArray<AudioLoudnessLevel> averageLoudnessArray)
        {
            Assert.IsTrue(tileDataAfterComputing.IsCreated, "Tile data results array is not created");
            Assert.IsTrue(averageLoudnessArray.IsCreated, "Average loudness array is not created");
            Assert.AreEqual(averageLoudnessArray.Length, tileDataAfterComputing.Length, "Arrays length mismatch");

            GetAverageAudioLoudnessDataJob averageAudioLoudnessDataJob = new()
            {
                tileData = tileDataAfterComputing,
                averageTileLoudnessData = averageLoudnessArray
            };

            JobHandle waitHandle = averageAudioLoudnessDataJob.Schedule(tileDataAfterComputing.Length,
                math.min(tileDataAfterComputing.Length, 64));
            waitHandle.Complete();
        }

        /// <summary>
        ///     Converts tilemap to array of tile data for computation 
        /// </summary>
        /// <param name="audioTilemap">
        ///     Instance of tilemap to calculate audio data from, should contain <see cref="AudioTile"/> objects
        /// </param>
        /// <param name="tileComputeData">
        ///     Reference to handle for Tile Data array, automatically allocated as PERSISTENT
        ///     Also your output array.
        ///     Output value.
        /// </param>
        /// <param name="allocator">Allocation mode for <see cref="tileComputeData"/></param>
        [BurstDiscard] public static void TilemapToArray(
            [NotNull] Tilemap audioTilemap,
            ref NativeArray<AudioTileInfo> tileComputeData,
            Allocator allocator = Allocator.Persistent
        )
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");
            Assert.IsTrue(CheckIfTilemapIsEnabled(audioTilemap), "Tilemap is not enabled nor active");

            TilemapInfo tilemapInfo = new(audioTilemap);

            // Refresh tilemap if dirty
            EnsureTilemapIsReady(audioTilemap);
            NativeArray<AudioLoudnessLevel> mufflingLevels = _tileMufflingLevels[audioTilemap];

            // Prepare tilemap data
            Vector3Int tilemapSize = audioTilemap.size;
            int tilesCount = tilemapSize.x * tilemapSize.y * tilemapSize.z;

            // Ensure arrays are initialized 
            QuickArray.PerformEfficientAllocation(ref tileComputeData, tilesCount, allocator);

            // Perform conversion
            _TilemapToArray(tilemapInfo, mufflingLevels,
                ref tileComputeData);

            Assert.AreEqual(tileComputeData.Length, tilesCount, "Something went wrong during computation");
        }

        /// <summary>
        ///     Converts tilemap and audio sources array of audio source data for computation
        /// </summary>
        /// <param name="audioTilemap">
        ///     Instance of tilemap to calculate audio data from, should contain <see cref="AudioTile"/> objects
        /// </param>
        /// <param name="sources">All audio sources to include in data array</param>
        /// <param name="audioSourceComputeData">
        ///     Reference to handle for Audio Source Data array, automatically allocated as PERSISTENT
        ///     Output value.
        /// </param>
        [BurstDiscard] public static void AudioSourcesToArray(
            [NotNull] Tilemap audioTilemap,
            [NotNull] AudibleSound[] sources,
            ref NativeArray<AudioSourceInfo> audioSourceComputeData)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");
            Assert.IsNotNull(sources, "Sources array is null");

            TilemapInfo tilemapInfo = new(audioTilemap);

            // This should be pretty performant
            for (int nIndex = 0; nIndex < sources.Length; nIndex++)
            {
                // Get basic information
                AudibleSound source = sources[nIndex];
                float3 worldPosition = source.transform.position;

                // Compute tilemap index
                Vector3Int tilePosition = audioTilemap.WorldToCell(worldPosition); // Do not subtract origin
                TileIndex tileIndex = new(new int3(tilePosition.x, tilePosition.y, tilePosition.z), tilemapInfo);

                // Assign value
                audioSourceComputeData[nIndex] =
                    new AudioSourceInfo(tileIndex, source.GetDecibelLevel(), source.GetRange());
            }

            Assert.AreEqual(sources.Length, audioSourceComputeData.Length, "Something went wrong during computation");
        }


        /// <summary>
        ///     Refreshes tile data, called when tilemap is dirty
        /// </summary>
        private static void RefreshTileData([NotNull] Tilemap audioTilemap)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");
            
            Vector3Int tilemapOrigin = audioTilemap.origin;
            Vector3Int tilemapSize = audioTilemap.size;
            int tilesCount = tilemapSize.x * tilemapSize.y * tilemapSize.z;

            // Try to get value and update array if necessary
            if (!_tileMufflingLevels.TryGetValue(audioTilemap,
                    out NativeArray<AudioLoudnessLevel> mufflingLevelsArray))
            {
                QuickArray.PerformEfficientAllocation(ref mufflingLevelsArray, tilesCount,
                    Allocator.Persistent);
                _tileMufflingLevels.Add(audioTilemap, mufflingLevelsArray);
            }
            else
            {
                QuickArray.PerformEfficientAllocation(ref mufflingLevelsArray, tilesCount,
                    Allocator.Persistent);
            }

            for (int x = 0; x < tilemapSize.x; x++)
            {
                for (int y = 0; y < tilemapSize.y; y++)
                {
                    for (int z = 0; z < tilemapSize.z; z++)
                    {
                        int nIndex = x * tilemapSize.y + y;

                        // Pre-compute Unity-based data
                        Vector3Int cellPosition = tilemapOrigin + new Vector3Int(x, y, 0);
                        AudioTile audioTile = audioTilemap.GetTile(cellPosition) as AudioTile;

                        // ReSharper disable once Unity.NoNullPropagation
                        AudioLoudnessLevel mufflingStrength =
                            audioTile?.GetMufflingData() ?? AudibilityLevel.LOUDNESS_NONE;
                        mufflingLevelsArray[nIndex] = mufflingStrength;
                    }
                }
            }

            // Ensure proper array is assigned
            _tileMufflingLevels[audioTilemap] = mufflingLevelsArray;
            
            // System is no longer dirty
            AudibilitySystem.SetDirty(audioTilemap, false);
        }

        /// <summary>
        ///     Internal, burst-compatible method to compute all necessary data
        /// </summary>
        [BurstCompile] private static void _TilemapToArray(
            in TilemapInfo tilemapInfo,
            in NativeArray<AudioLoudnessLevel> mufflingLevels,
            ref NativeArray<AudioTileInfo> audioTileData
        )
        {
            int3 tilemapSize = tilemapInfo.size;

            // Prepare analysis data
            for (int x = 0; x < tilemapSize.x; x++)
            {
                for (int y = 0; y < tilemapSize.y; y++)
                {
                    for (int z = 0; z < tilemapSize.z; z++)
                    {
                        int3 cellPosition = new(x, y, z);

                        // Compute tile index
                        TileIndex nIndex = new(cellPosition + tilemapInfo.originPoint, tilemapInfo);

                        // Pre-compute Unity-based data
                        AudioLoudnessLevel mufflingStrength = mufflingLevels[nIndex];

                        // Copy new tile data into array
                        AudioTileInfo tileInfo = new(nIndex, mufflingStrength);
                        audioTileData[nIndex] = tileInfo;
                    }
                }
            }
        }
    }
}