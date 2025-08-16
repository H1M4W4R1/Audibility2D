using JetBrains.Annotations;
using Systems.Audibility.Common.Components;
using Systems.Audibility.Common.Utility;
using Systems.Audibility2D.Data;
using Systems.Audibility2D.Tiles;
using Systems.Audibility2D.Utility;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace Systems.Audibility2D.Debugging
{
    /// <summary>
    ///     Debug script used to sample 2D audio for specified tilemap
    /// </summary>
    public sealed class AudioSampler2D : MonoBehaviour
    {
        /// <summary>
        ///     Audio tilemap that will be used to compute data
        ///     Must use <see cref="AudioTile"/> to modify audio behaviour
        /// </summary>
        [SerializeField] [CanBeNull] private Tilemap audioTilemap;

        private NativeArray<AudioTile2DComputeData> _tileComputeData;
        private NativeArray<AudioSource2DComputeData> _audioSourceComputeData;

        private void OnDrawGizmos()
        {
            // Ensure tilemap is set
            if (!audioTilemap) return;

            // Initialize tilemap arrays
            AudibilityTools2D.TilemapToArray(audioTilemap, ref _tileComputeData);

            // Prepare array of audio sources
            AudibleSound[] sources =
                FindObjectsByType<AudibleSound>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            QuickArray.PerformEfficientAllocation(ref _audioSourceComputeData, sources.Length,
                Allocator.Persistent);

            // Get audio sources data
            AudibilityTools2D.AudioSourcesToArray(audioTilemap, sources, ref _audioSourceComputeData);

            // Handle computation
            AudibilityLevel2D.UpdateAudibilityLevel(_audioSourceComputeData, ref _tileComputeData);

            // Draw gizmos
            for (int n = 0; n < _tileComputeData.Length; n++)
            {
                float averageAudioLevel = _tileComputeData[n].currentAudioLevel.GetAverage();
                Gizmos.color = Color.Lerp(Color.red, Color.green, averageAudioLevel / Loudness.MAX);
                Gizmos.DrawSphere(_tileComputeData[n].worldPosition, 0.2f);
            }
        }
    }
}