using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility2D.Data
{
    public struct AudioTile2DComputeData
    {
        public DecibelLevel currentAudioLevel;
        
        public readonly int index;
        public readonly float3 worldPosition;
        public readonly Vector3Int tilemapPosition;
        public readonly DecibelLevel mufflingStrength;
        public readonly int northIndex;
        public readonly int eastIndex;
        public readonly int southIndex;
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