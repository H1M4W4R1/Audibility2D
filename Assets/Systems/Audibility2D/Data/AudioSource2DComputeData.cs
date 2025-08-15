using Systems.Audibility.Common.Data;
using Unity.Mathematics;

namespace Systems.Audibility2D.Data
{
    public readonly struct AudioSource2DComputeData
    {
        public readonly int tileIndex;
        public readonly float3 worldPosition;
        public readonly DecibelLevel audioLevel;
        public readonly float range;
        public readonly float rangeSq;

        public AudioSource2DComputeData(int tileIndex, float3 worldPosition, DecibelLevel audioLevel, float range)
        {
            this.tileIndex = tileIndex;
            this.worldPosition = worldPosition;
            this.audioLevel = audioLevel;
            this.range = range;
            rangeSq = range * range;
        }
    }
}