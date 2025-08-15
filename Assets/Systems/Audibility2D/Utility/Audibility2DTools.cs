using System.Collections.Generic;
using JetBrains.Annotations;
using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Systems.Audibility2D.Data;
using Systems.Audibility2D.Tiles;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility
{
    [BurstCompile] public static class Audibility2DTools
    {
        private static readonly Dictionary<Tilemap, NativeArray<DecibelLevel>> _tileMufflingLevels = new();

        private static void RefreshTileData([NotNull] Tilemap audioTilemap)
        {
            Vector3Int tilemapOrigin = audioTilemap.origin;
            Vector3Int tilemapSize = audioTilemap.size;
            int tilesCount = tilemapSize.x * tilemapSize.y;

            // Try to get value and update array if necessary
            if (!_tileMufflingLevels.TryGetValue(audioTilemap, out NativeArray<DecibelLevel> mufflingLevelsArray))
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
                    int nIndex = x * tilemapSize.y + y;

                    // Pre-compute Unity-based data
                    Vector3Int cellPosition = tilemapOrigin + new Vector3Int(x, y, 0);
                    AudioTile audioTile = audioTilemap.GetTile(cellPosition) as AudioTile;

                    // ReSharper disable once Unity.NoNullPropagation
                    DecibelLevel mufflingStrength = audioTile?.GetMufflingData() ?? Muffling.NONE;
                    mufflingLevelsArray[nIndex] = mufflingStrength;
                }
            }

            // System is no longer dirty
            AudibilitySystem2D.SetDirty(audioTilemap, false);
        }

        [BurstCompile]
        private static void _TilemapToArray(
            in Vector3Int tilemapOrigin,
            in Vector3Int tilemapSize,
            in float3 cellSize,
            in float3 worldOrigin,
            in NativeArray<DecibelLevel> mufflingLevels,
            ref NativeArray<AudioTile2DComputeData> audioTileData
        )
        {
            // Prepare analysis data
            for (int x = 0; x < tilemapSize.x; x++)
            {
                for (int y = 0; y < tilemapSize.y; y++)
                {
                    // Compute index
                    int nIndex = x * tilemapSize.y + y;

                    // Pre-compute Unity-based data
                    Vector3Int cellPosition = tilemapOrigin + new Vector3Int(x, y, 0);
                    DecibelLevel mufflingStrength = mufflingLevels[nIndex];

                    int northIndex = Hint.Likely(y + 1 < tilemapSize.y) ? x * tilemapSize.y + y + 1 : -1;
                    int southIndex = Hint.Likely(y - 1 >= 0) ? x * tilemapSize.y + y - 1 : -1;
                    int westIndex = Hint.Likely(x - 1 >= 0) ? (x - 1) * tilemapSize.y + y : -1;
                    int eastIndex = Hint.Likely(x + 1 < tilemapSize.x) ? (x + 1) * tilemapSize.y + y : -1;

                    // Get data from tile, we're pre-caching it early and using multiplication
                    // to improve performance, as it's faster than casting external calls to Unity API
                    float3 worldPosition = worldOrigin + new float3(x * cellSize.x, y * cellSize.y, 0);


                    AudioTile2DComputeData tileData = new(
                        nIndex, worldPosition, cellPosition, mufflingStrength,
                        northIndex, eastIndex, southIndex, westIndex);
                    audioTileData[nIndex] = tileData;
                }
            }
        }

        [BurstDiscard] public static void TilemapToArray(
            [NotNull] Tilemap audioTilemap,
            ref NativeArray<AudioTile2DComputeData> audioTileData
        )
        {
            // Refresh tilemap if dirty
            if (AudibilitySystem2D.IsDirty(audioTilemap)) RefreshTileData(audioTilemap);
            NativeArray<DecibelLevel> mufflingLevels = _tileMufflingLevels[audioTilemap];

            // Prepare tilemap data
            Vector3Int tilemapOrigin = audioTilemap.origin;
            Vector3Int tilemapSize = audioTilemap.size;
            int tilesCount = tilemapSize.x * tilemapSize.y;

            // Ensure arrays are initialized 
            QuickArray.PerformEfficientAllocation(ref audioTileData, tilesCount, Allocator.Persistent);

            float3 cellSize = audioTilemap.cellSize;
            float3 worldOrigin = (float3) audioTilemap.CellToWorld(audioTilemap.origin) + 0.5f * cellSize;

            // Perform conversion
            _TilemapToArray(tilemapOrigin, tilemapSize, cellSize, worldOrigin, mufflingLevels, ref audioTileData);
        }
    }
}