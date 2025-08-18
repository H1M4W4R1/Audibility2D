using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Utilities.Indexing.Grid;

namespace Systems.Audibility2D.Data.Native
{
    /// <summary>
    ///     Object to store computation data of audio source for 2D audibility system
    /// </summary>
    public readonly struct AudioSourceInfo // 10B
    {
        /// <summary>
        ///     Index of tile in array
        /// </summary>
        public readonly Index3D tileIndex; // 4B
        
        /// <summary>
        ///     Loudness of this audio source
        /// </summary>
        public readonly AudioLoudnessLevel audioLevel; // 2B
        
        /// <summary>
        ///     Maximum range of this audio source
        /// </summary>
        public readonly float range; // 4B

        public AudioSourceInfo(Index3D tileIndex, AudioLoudnessLevel audioLevel, float range)
        {
            this.tileIndex = tileIndex;
            this.audioLevel = audioLevel;
            this.range = range;
        }
    }
}