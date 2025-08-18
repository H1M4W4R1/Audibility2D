using JetBrains.Annotations;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Tiles;
using Systems.Audibility2D.Utility;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Components.Debugging
{
    /// <summary>
    ///     Debug script used to sample 2D audio for specified tilemap
    /// </summary>
    public sealed class AudioSampler : MonoBehaviour
    {
        /// <summary>
        ///     Audio tilemap that will be used to compute data
        ///     Must use <see cref="AudioTile"/> to modify audio behaviour
        /// </summary>
        [SerializeField] [CanBeNull] private Tilemap audioTilemap;

        private NativeArray<AudioTileData> _tileComputeData;
        private NativeArray<AudioSourceData> _audioSourceComputeData;
        private NativeArray<AudioTileDebugData> _tileDebugData;

        private void OnDrawGizmos()
        {
            // Ensure tilemap is set
            if (!audioTilemap) return;

            Vector3Int tilemapSize = audioTilemap.size;
            int tilesCount = tilemapSize.x * tilemapSize.y * tilemapSize.z;
            QuickArray.PerformEfficientAllocation(ref _tileDebugData, tilesCount, Allocator.TempJob);

            // Compute audibility in 2D space
            AudibilityLevel.UpdateAudibilityLevel(audioTilemap, ref _audioSourceComputeData, ref _tileComputeData);
            
            // Compute average tile loudness
            AudibilityTools.GetTileDebugData(audioTilemap, in _tileComputeData, ref _tileDebugData);
            
            // Draw gizmos
            for (int n = 0; n < _tileDebugData.Length; n++)
            {
                Gizmos.color = Color.Lerp(Color.red, Color.green, _tileDebugData[n].normalizedLoudness);
                Gizmos.DrawSphere(_tileDebugData[n].worldPosition, 0.2f);
            }

            _tileDebugData.Dispose();
        }
    }
}