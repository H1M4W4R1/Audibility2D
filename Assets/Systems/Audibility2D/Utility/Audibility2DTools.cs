using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Systems.Audibility2D.Data;
using Systems.Audibility2D.Tiles;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility
{
    public static class Audibility2DTools
    {
        // TODO: Performance improvements of this crap
        public static void TilemapToArray(
                Tilemap audioTilemap,
            ref NativeArray<AudioTile2DComputeData> audioTileData
        )
        {
            // Find tilemap
            if (!audioTilemap)
            {
                Debug.LogError($"[{nameof(Audibility2DTools)}] No audioTilemap found!");
                return;
            }

            // Prepare tilemap data
            Vector3Int tilemapOrigin = audioTilemap.origin;
            Vector3Int tilemapSize = audioTilemap.size;
            int tilesCount = tilemapSize.x * tilemapSize.y;

            // Ensure arrays are initialized 
            QuickArray.PerformEfficientAllocation(ref audioTileData, tilesCount, Allocator.Persistent);

            // Prepare analysis data
            for (int x = 0; x < tilemapSize.x; x++)
            {
                for (int y = 0; y < tilemapSize.y; y++)
                {
                    // Compute index and tile location
                    int nIndex = x * tilemapSize.y + y;
                    int northIndex = Hint.Likely(y + 1 < tilemapSize.y) ? x * tilemapSize.y + y + 1 : -1;
                    int southIndex = Hint.Likely(y - 1 >= 0) ? x * tilemapSize.y + y - 1 : -1;
                    int westIndex = Hint.Likely(x - 1 >= 0) ? (x - 1) * tilemapSize.y + y : -1;
                    int eastIndex = Hint.Likely(x + 1 < tilemapSize.x) ? (x + 1) * tilemapSize.y + y : -1;
 
                    Vector3Int cellPosition = tilemapOrigin + new Vector3Int(x, y, 0);

                    // Get data from tile
                    // TODO: World position can be calculated from tilemap itself using origin cell position
                    //       moved by cell vector index
                    float3 worldPosition = audioTilemap.CellToWorld(cellPosition) + 0.5f * audioTilemap.cellSize;
                    AudioTile audioTile = audioTilemap.GetTile(cellPosition) as AudioTile;

                    // ReSharper disable once Unity.NoNullPropagation
                    // TODO: Maybe this can be cached in editor to prevent acquiring data
                    //       when done to native collection and above suggestion is executed
                    //       we most likely will get rid of any Unity API calls and it will be possible
                    //       to move this method to Burst for perfect efficiency.
                    DecibelLevel mufflingStrength = audioTile?.GetMufflingData() ?? Muffling.NONE;

                    AudioTile2DComputeData tileData = new(
                        nIndex, worldPosition, cellPosition, mufflingStrength,
                        northIndex, eastIndex, southIndex, westIndex);
                    audioTileData[nIndex] = tileData;
                }
            }
        }
    }
}