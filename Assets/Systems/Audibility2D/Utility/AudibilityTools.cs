using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.Audibility2D.Components;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Jobs;
using Systems.Audibility2D.Tiles;
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
    ///     Additional class to reduce mess in <see cref="AudibilityLevel"/>
    /// </summary>
    [BurstCompile] public static class AudibilityTools
    {
        /// <summary>
        ///     Data about tile muffling levels, cached to improve performance
        /// </summary>
        private static readonly Dictionary<Tilemap, NativeArray<AudioLoudnessLevel>> _tileMufflingLevels = new();

        /// <summary>
        ///     Get average loudness data from results array
        /// </summary>
        /// <param name="tileDataAfterComputing">Array with computed tile data</param>
        /// <param name="debugDataArray">
        ///     Output array to store loudness, same length as <see cref="tileDataAfterComputing"/>
        /// </param>
        public static void GetTileDebugData(
            [NotNull] in Tilemap audioTilemap,
            in NativeArray<AudioTileData> tileDataAfterComputing,
            ref NativeArray<AudioTileDebugData> debugDataArray)
        {
            Assert.IsTrue(tileDataAfterComputing.IsCreated);
            Assert.IsTrue(debugDataArray.IsCreated);
            Assert.AreEqual(debugDataArray.Length, tileDataAfterComputing.Length);

            TilemapInfo tilemapInfo = new TilemapInfo(audioTilemap);
            
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
            in NativeArray<AudioTileData> tileDataAfterComputing,
            ref NativeArray<int> averageLoudnessArray)
        {
            Assert.IsTrue(tileDataAfterComputing.IsCreated);
            Assert.IsTrue(averageLoudnessArray.IsCreated);
            Assert.AreEqual(averageLoudnessArray.Length, tileDataAfterComputing.Length);

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
        [BurstDiscard] public static void TilemapToArray(
            [NotNull] Tilemap audioTilemap,
            ref NativeArray<AudioTileData> tileComputeData
        )
        {
            Assert.IsNotNull(audioTilemap);

            TilemapInfo tilemapInfo = new(audioTilemap);
            
            // Refresh tilemap if dirty
            if (AudibilitySystem.IsDirty(audioTilemap)) RefreshTileData(audioTilemap);
            NativeArray<AudioLoudnessLevel> mufflingLevels = _tileMufflingLevels[audioTilemap];

            // Prepare tilemap data
            Vector3Int tilemapSize = audioTilemap.size;
            int tilesCount = tilemapSize.x * tilemapSize.y * tilemapSize.z;

            // Ensure arrays are initialized 
            QuickArray.PerformEfficientAllocation(ref tileComputeData, tilesCount, Allocator.Persistent);

            // Perform conversion
            _TilemapToArray(tilemapInfo, mufflingLevels,
                ref tileComputeData);

            Assert.AreEqual(tileComputeData.Length, tilesCount);
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
            ref NativeArray<AudioSourceData> audioSourceComputeData)
        {
            Assert.IsNotNull(audioTilemap);
            Assert.IsNotNull(sources);

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
                    new AudioSourceData(tileIndex, source.GetDecibelLevel(), source.GetRange());
            }

            Assert.AreEqual(sources.Length, audioSourceComputeData.Length);
        }


        /// <summary>
        ///     Refreshes tile data, called when tilemap is dirty
        /// </summary>
        private static void RefreshTileData([NotNull] Tilemap audioTilemap)
        {
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

            // System is no longer dirty
            AudibilitySystem.SetDirty(audioTilemap, false);
        }

        /// <summary>
        ///     Internal, burst-compatible method to compute all necessary data
        /// </summary>
        [BurstCompile] private static void _TilemapToArray(
            in TilemapInfo tilemapInfo,
            in NativeArray<AudioLoudnessLevel> mufflingLevels,
            ref NativeArray<AudioTileData> audioTileData
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
                        int3 cellPosition = new int3(x, y, z);
                        
                        // Compute tile index
                        TileIndex nIndex = new TileIndex(cellPosition + tilemapInfo.originPoint, tilemapInfo);
                        
                        // Pre-compute Unity-based data
                        AudioLoudnessLevel mufflingStrength = mufflingLevels[nIndex];

                        int northIndex = Hint.Likely(y + 1 < tilemapSize.y) ? x * tilemapSize.y + y + 1 : -1;
                        int southIndex = Hint.Likely(y - 1 >= 0) ? x * tilemapSize.y + y - 1 : -1;
                        int westIndex = Hint.Likely(x - 1 >= 0) ? (x - 1) * tilemapSize.y + y : -1;
                        int eastIndex = Hint.Likely(x + 1 < tilemapSize.x) ? (x + 1) * tilemapSize.y + y : -1;

                        // Register node neighbours taking limit into account
                        int nNeighbours = 0;
                        AudioTileData tileData = new(nIndex, mufflingStrength);
                        nNeighbours += tileData.SetNeighbour(northIndex, nNeighbours);
                        nNeighbours += tileData.SetNeighbour(southIndex, nNeighbours);
                        nNeighbours += tileData.SetNeighbour(westIndex, nNeighbours);
                        // ReSharper disable once RedundantAssignment
                        nNeighbours += tileData.SetNeighbour(eastIndex, nNeighbours);

                        // Copy new tile data into array
                        audioTileData[nIndex] = tileData;
                    }
                }
            }
        }
    }
}