using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Audibility2D.Utility;

namespace Systems.Audibility2D.Data.Native
{
    /// <summary>
    ///     Object to store computation data of audio-receiving tile for 2D audibility system
    /// </summary>
    public struct AudioTileInfo // 8B
    {
        /// <summary>
        ///     Current audio level on this tile, output value
        /// </summary>
        public AudioLoudnessLevel currentAudioLevel; // 2B

        /// <summary>
        ///     Index of this tile in array
        /// </summary>
        public readonly TileIndex index; // 4B

        /// <summary>
        ///     Muffling level of this tile (used to reduce sound loudness when entering this tile)
        /// </summary>
        public readonly AudioLoudnessLevel mufflingStrength; // 2B

        public AudioTileInfo(
            TileIndex index,
            AudioLoudnessLevel mufflingStrength)
        {
            this.index = index;
            this.mufflingStrength = mufflingStrength;

            currentAudioLevel = AudibilityTools.LOUDNESS_NONE;
        }
    }
}