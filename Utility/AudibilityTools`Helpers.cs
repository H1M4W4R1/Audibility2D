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
    ///     Additional class to reduce mess in <see cref="AudibilityTools"/>
    /// </summary>
    public static partial class AudibilityTools
    {
     
        /// <summary>
        ///     Get average loudness data from results array
        /// </summary>
        /// <param name="audioTilemap">Tilemap to get debug data for</param>
        /// <param name="tileDataAfterComputing">Array with computed tile data</param>
        /// <param name="debugDataArray">
        ///     Output array to store loudness, same length as <see cref="tileDataAfterComputing"/>
        /// </param>
        [BurstDiscard] public static void GetTileDebugData(
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
        [BurstDiscard] public static void GetAverageLoudnessData(
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
        /// <param name="allocator">Allocator to create array</param>
        [BurstDiscard] public static void RefreshAudioSourcesArray(
            [NotNull] Tilemap audioTilemap,
            [NotNull] AudibleSound[] sources,
            ref NativeArray<AudioSourceInfo> audioSourceComputeData,
            Allocator allocator = Allocator.Persistent)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");
            Assert.IsNotNull(sources, "Sources array is null");

            // Create or update array if necessary
            QuickArray.PerformEfficientAllocation(ref audioSourceComputeData, sources.Length,
                allocator);

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

            Assert.AreEqual(sources.Length, audioSourceComputeData.Length,
                "Something went wrong during computation");
        }

        /// <summary>
        ///     Internal solution to update specific tile data
        ///     When modifying also check <see cref="RefreshTileDataArray"/>
        /// </summary>
        [BurstDiscard] internal static void RefreshTileDataArrayAt(
            [NotNull] Tilemap audioTilemap,
            int3 tilePositionAbsolute,
            ref NativeArray<AudioTileInfo> tileComputeData,
            Allocator allocator = Allocator.Persistent)
        {
            // If array is not created build new one
            if (!tileComputeData.IsCreated)
            {
                RefreshTileDataArray(audioTilemap, ref tileComputeData, allocator);
                return;
            }

            // Convert tile data into proper helpers
            TilemapInfo tilemapInfo = new(audioTilemap);
            TileIndex tileIndex = new(TileIndex.ToIndexAbsolute(tilePositionAbsolute, tilemapInfo));

            // Get audio tile at desired location
            AudioTile audioTile = audioTilemap.GetTile<AudioTile>(new Vector3Int(tilePositionAbsolute.x,
                tilePositionAbsolute.y, tilePositionAbsolute.z));

            // ReSharper disable once Unity.NoNullPropagation
            AudioLoudnessLevel mufflingStrength =
                audioTile?.GetMufflingData() ?? LOUDNESS_NONE;

            // Rota-set variable because C# stupid
            AudioTileInfo tileInfo = tileComputeData[tileIndex];
            tileInfo.mufflingStrength = mufflingStrength;
            tileComputeData[tileIndex] = tileInfo;
        }

        /// <summary>
        ///     Refreshes tile data, called when tilemap is dirty
        ///     When modifying also check <see cref="RefreshTileDataArrayAt"/>
        /// </summary>
        [BurstDiscard] public static void RefreshTileDataArray(
            [NotNull] Tilemap audioTilemap,
            ref NativeArray<AudioTileInfo> tileComputeData,
            Allocator allocator = Allocator.Persistent)
        {
            Assert.IsNotNull(audioTilemap, "Audio tilemap is null");

            Vector3Int tilemapOrigin = audioTilemap.origin;
            Vector3Int tilemapSize = audioTilemap.size;
            int tilesCount = tilemapSize.x * tilemapSize.y * tilemapSize.z;

            // Ensure arrays are initialized 
            QuickArray.PerformEfficientAllocation(ref tileComputeData, tilesCount, allocator);

            TilemapInfo tilemapInfo = new(audioTilemap);
            
            for (int x = 0; x < tilemapSize.x; x++)
            {
                for (int y = 0; y < tilemapSize.y; y++)
                {
                    for (int z = 0; z < tilemapSize.z; z++)
                    {
                        // Pre-compute Unity-based data
                        Vector3Int cellPosition = tilemapOrigin + new Vector3Int(x, y, z);
                        AudioTile audioTile = audioTilemap.GetTile<AudioTile>(cellPosition);
                        TileIndex tileIndex =
                            new(TileIndex.ToIndexAbsolute(new int3(cellPosition.x, cellPosition.y, cellPosition.z),
                                tilemapInfo));

                        // ReSharper disable once Unity.NoNullPropagation
                        AudioLoudnessLevel mufflingStrength =
                            audioTile?.GetMufflingData() ?? LOUDNESS_NONE;

                        // Rota-set variable because C# stupid
                        AudioTileInfo tileInfo = tileComputeData[tileIndex];
                        tileInfo.index = tileIndex;
                        tileInfo.mufflingStrength = mufflingStrength;
                        tileComputeData[tileIndex] = tileInfo;
                    }
                }
            }
        }
    }
}