using Systems.Audibility2D.Data.Native.Wrappers;

namespace Systems.Audibility2D.Data.Native
{
    /// <summary>
    ///     Object to store computation data of audio source for 2D audibility system
    /// </summary>
    public readonly struct AudioSourceInfo // 6B
    {
        /// <summary>
        ///     Index of tile in array
        /// </summary>
        public readonly Index2D tileIndex; // 4B
        
        /// <summary>
        ///     Loudness of this audio source
        /// </summary>
        public readonly AudioLoudnessLevel audioLevel; // 2B


        public AudioSourceInfo(Index2D tileIndex, AudioLoudnessLevel audioLevel)
        {
            this.tileIndex = tileIndex;
            this.audioLevel = audioLevel;
        }
    }
}