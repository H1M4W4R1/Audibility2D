using Systems.Audibility.Common.Data;
using Unity.Mathematics;

namespace Systems.Audibility2D.Data
{
    /// <summary>
    ///     Object to store computation data of audio source for 2D audibility system
    /// </summary>
    public readonly struct AudioSource2DComputeData
    {
        /// <summary>
        ///     Index of tile in array
        /// </summary>
        public readonly int tileIndex;
        
        /// <summary>
        ///     World location of tile (center)
        /// </summary>
        public readonly float3 worldPosition;
        
        /// <summary>
        ///     Loudness of this audio source
        /// </summary>
        public readonly DecibelLevel audioLevel;
        
        /// <summary>
        ///     Maximum range of this audio source
        /// </summary>
        public readonly float range;
        
        /// <summary>
        ///     Same as <see cref="range"/>, but powered-up by two
        /// </summary>
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