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

        private void OnDrawGizmos()
        {
            // Ensure tilemap is set
            if (!audioTilemap) return;

            // Compute audibility in 2D space
            AudibilityLevel.UpdateAudibilityLevel(audioTilemap, ref _audioSourceComputeData, ref _tileComputeData);

            // Draw gizmos
            for (int n = 0; n < _tileComputeData.Length; n++)
            {
                float averageAudioLevel = _tileComputeData[n].currentAudioLevel.GetAverage();
                Gizmos.color = Color.Lerp(Color.red, Color.green, averageAudioLevel / AudibilityLevel.LOUDNESS_MAX);
                Gizmos.DrawSphere(_tileComputeData[n].worldPosition, 0.2f);
            }
        }
    }
}