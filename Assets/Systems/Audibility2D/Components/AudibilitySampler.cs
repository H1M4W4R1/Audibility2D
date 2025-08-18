using System;
using NUnit.Framework;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Audibility2D.Utility;
using Systems.Audibility2D.Utility.Internal;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Components
{
    [RequireComponent(typeof(Tilemap))] [ExecuteInEditMode] public sealed class AudibilityUpdater : MonoBehaviour
    {
        private Tilemap _tilemap;
        private NativeArray<AudioTileInfo> _audioTileData;
        private NativeArray<AudioSourceInfo> _audioSourceData;

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

        /// <summary>
        ///     Update every 0.2s (or whatever is defined)
        /// </summary>
        public void Update()
        {
            // Perform update of audio level
            AudibilityTools.UpdateAudibilityLevel(Tilemap, ref _audioSourceData, ref _audioTileData);
        }

        private void OnDrawGizmos()
        {
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
            foreach(AudioTileInfo audioTileInfo in _audioTileData)
            {
                // TODO: Improve perf of this line using some Black Magic F*$#ery
                float3 worldTilePosition = audioTileInfo.index.GetWorldPosition(tilemapInfo);

                // Quickly check camera point in view frustrum
                if (!MakeGizmosFasterUtility.PointInFrustum(worldTilePosition, frustrumPlanes)) continue;

                Gizmos.color = Color.Lerp(Color.red, Color.green,
                    audioTileInfo.currentAudioLevel / (float) AudibilityTools.LOUDNESS_MAX);
                Gizmos.DrawSphere(worldTilePosition, 0.2f);
            }

            frustrumPlanes.Dispose();
        }
    }
}