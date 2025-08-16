using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility2D.Data
{
    /// <summary>
    ///     Object to store computation data of audio-receiving tile for 2D audibility system
    /// </summary>
    public struct AudioTile2DComputeData
    {
        /// <summary>
        ///     Current audio level on this tile, output value
        /// </summary>
        public DecibelLevel currentAudioLevel;
        
        /// <summary>
        ///     Index of this tile in array
        /// </summary>
        public readonly int index;
        
        /// <summary>
        ///     Position of center of this tile in world space
        /// </summary>
        public readonly float3 worldPosition;
        
        /// <summary>
        ///     Position of this tile in tilemap
        /// </summary>
        public readonly Vector3Int tilemapPosition;
        
        /// <summary>
        ///     Muffling level of this tile (used to reduce sound loudness when entering this tile)
        /// </summary>
        public readonly DecibelLevel mufflingStrength;
        
        /// <summary>
        ///     North neighbour index in tile array
        /// </summary>
        public readonly int northIndex;
        
        /// <summary>
        ///     East neighbour index in tile array
        /// </summary>
        public readonly int eastIndex;
        
        /// <summary>
        ///     South neighbour index in tile array
        /// </summary>
        public readonly int southIndex;
        
        /// <summary>
        ///     West neighbour index in tile array
        /// </summary>
        public readonly int westIndex;

        public AudioTile2DComputeData(int index, float3 worldPosition, Vector3Int tilemapPosition, DecibelLevel mufflingStrength, int northIndex, int eastIndex, int southIndex, int westIndex)
        {
            this.index = index;
            this.worldPosition = worldPosition;
            this.tilemapPosition = tilemapPosition;
            this.mufflingStrength = mufflingStrength;
            this.northIndex = northIndex;
            this.eastIndex = eastIndex;
            this.southIndex = southIndex;
            this.westIndex = westIndex;

            currentAudioLevel = Loudness.SILENCE;
        }
    }
}