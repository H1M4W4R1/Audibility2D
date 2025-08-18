using System.Runtime.CompilerServices;
using Systems.Audibility2D.Data;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Audibility2D.Data.Settings;
using Systems.Audibility2D.Utility;
using Systems.Audibility2D.Utility.Internal;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Components
{
    /// <summary>
    ///     Script that automatically updates audibility data. Created to make implementation as quick as
    ///     possible. Must be implemented on tilemap consisting of audio tile(s).
    /// </summary>
    [RequireComponent(typeof(Tilemap))] [ExecuteInEditMode] public sealed class AudibilityUpdater : MonoBehaviour
    {
        /// <summary>
        ///     Enables debug drawing
        /// </summary>
        [SerializeField] private bool enableDebug = true;
        
        private Tilemap _tilemap;
        private NativeArray<AudioTileInfo> _audioTileData;
        private NativeArray<AudioSourceInfo> _audioSourceData;
        private bool _areEventsConfigured;

        /// <summary>
        ///     Access local tilemap
        /// </summary>
        private Tilemap Tilemap
        {
            get
            {
                if (!_tilemap) _tilemap = GetComponent<Tilemap>();
                return _tilemap;
            }
        }

        /// <summary>
        ///     Get tile data array
        /// </summary>
        /// <remarks>
        ///     Do not modify!
        /// </remarks>
        public NativeArray<AudioTileInfo> GetTileDataArray() => _audioTileData;

        /// <summary>
        ///     Get info about tile at specified absolute location
        /// </summary>
        public AudioTileInfo GetTileInfo(int3 tileLocationAbsolute) => _audioTileData[
            TileIndex.ToIndexAbsolute(tileLocationAbsolute, new TilemapInfo(Tilemap))
        ];

        /// <summary>
        ///     Get info about tile at index
        /// </summary>
        public AudioTileInfo GetTileInfo(int tileIndex) => _audioTileData[tileIndex];

        private void Update()
        {
            // Perform update of audio level
            AudibilityTools.UpdateAudibilityLevel(Tilemap, ref _audioSourceData,
                ref _audioTileData);
        }

        /// <summary>
        ///     Recompute cache of audio muffling data
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RefreshAudioMufflingCache()
        {
            AudibilityTools.RefreshTileDataArray(Tilemap, ref _audioTileData);
        }

        private void OnDrawGizmos()
        {
            // If debug is not enabled skip
            if (!enableDebug) return;
            
            // Ensure tilemap is set
            if (!Tilemap) return;

            // Use Scene camera in Editor (falls back to main camera if missing)
            // In case no camera was found we don't want anything
            Camera gizmosCamera = Camera.current ? Camera.current : Camera.main;
            if (!gizmosCamera) return;

            // Ensure that audio tile data exists in current context
            // to prevent throwing assertion errors when recompiled in background
            if (!_audioTileData.IsCreated) return;

            TilemapInfo tilemapInfo = new(Tilemap);

            // Compute camera planes
            NativeArray<float4> frustrumPlanes = new(6, Allocator.TempJob);
            gizmosCamera.ExtractFrustumPlanes(ref frustrumPlanes);

            // Draw gizmos
            foreach (AudioTileInfo audioTileInfo in _audioTileData)
            {
                // TODO: Improve perf of this line using some Black Magic F*$#ery
                float3 worldTilePosition = audioTileInfo.index.GetWorldPosition(tilemapInfo);

                // Quickly check camera point in view frustrum
                if (!MakeGizmosFasterUtility.PointInFrustum(worldTilePosition, frustrumPlanes)) continue;

                Color audibilityNoneColor = AudibilitySettings.Instance.gizmosColorMinAudibility;
                Color audibilityFullColor = AudibilitySettings.Instance.gizmosColorMaxAudibility;
                
                Gizmos.color = Color.Lerp(audibilityNoneColor, audibilityFullColor,
                    audioTileInfo.currentAudioLevel / (float) AudibilityTools.LOUDNESS_MAX);
                Gizmos.DrawSphere(worldTilePosition, 0.2f);
            }

            frustrumPlanes.Dispose();
        }
        
#region EVENTS_HANDLING

        internal void OnTileUpdatedHandler(int3 tilePosition)
        {
            AudibilityTools.RefreshTileDataArrayAt(Tilemap, tilePosition,
                ref _audioTileData);
            //RefreshAudioMufflingCache();
        }
        
        internal void OnMufflingMaterialDataChangedHandler(AudioMufflingMaterialData materialScriptableObject) =>
            RefreshAudioMufflingCache();

        internal void OnMufflingMaterialChangedHandler(
            TileBase tileScriptableObject,
            AudioMufflingMaterialData newMaterial) =>
            RefreshAudioMufflingCache();

        private void OnDestroy()
        {
            // Clear allocations
            _audioTileData.Dispose();
            _audioSourceData.Dispose();
        }

#endregion
    }
}