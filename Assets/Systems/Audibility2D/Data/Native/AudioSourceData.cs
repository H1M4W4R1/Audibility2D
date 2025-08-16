using Unity.Mathematics;

namespace Systems.Audibility2D.Data.Native
{
    /// <summary>
    ///     Object to store computation data of audio source for 2D audibility system
    /// </summary>
    public readonly struct AudioSourceData
    {
        /// <summary>
        ///     Index of tile in array
        /// </summary>
        public readonly int tileIndex; // 4B
        
        /// <summary>
        ///     World location of tile (center)
        /// </summary>
        public readonly float3 worldPosition; // 12B
        
        /// <summary>
        ///     Loudness of this audio source
        /// </summary>
        public readonly AudioLoudnessLevel audioLevel; // 16B
        
        /// <summary>
        ///     Maximum range of this audio source
        /// </summary>
        public readonly float range; // 4B

        public AudioSourceData(int tileIndex, float3 worldPosition, AudioLoudnessLevel audioLevel, float range)
        {
            this.tileIndex = tileIndex;
            this.worldPosition = worldPosition;
            this.audioLevel = audioLevel;
            this.range = range;
        }
    }
}