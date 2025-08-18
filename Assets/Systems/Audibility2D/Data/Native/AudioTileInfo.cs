using System.Runtime.CompilerServices;
using Systems.Audibility2D.Data.Native.Wrappers;
using Systems.Audibility2D.Utility;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility2D.Data.Native
{
    /// <summary>
    ///     Object to store computation data of audio-receiving tile for 2D audibility system
    /// </summary>
    public struct AudioTileInfo
    {
        /// <summary>
        ///     Current audio level on this tile, output value
        /// </summary>
        public AudioLoudnessLevel currentAudioLevel;

        /// <summary>
        ///     Index of this tile in array
        /// </summary>
        public readonly TileIndex index;

        /// <summary>
        ///     Muffling level of this tile (used to reduce sound loudness when entering this tile)
        /// </summary>
        public readonly AudioLoudnessLevel mufflingStrength;

        /// <summary>
        ///     Node neighbours data
        /// </summary>
        public AudioTileNeighboursInfo neighboursInfo;

        public AudioTileInfo(
            TileIndex index,
            AudioLoudnessLevel mufflingStrength)
        {
            this.index = index;
            this.mufflingStrength = mufflingStrength;

            currentAudioLevel = AudibilityLevel.LOUDNESS_NONE;
            neighboursInfo = AudioTileNeighboursInfo.New();
        }

        /// <summary>
        ///     Adds neighbour to node
        /// </summary>
        /// <returns>1 if neighbour was added, 0 if maximum count has been exceeded</returns>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SetNeighbour(int neighbourIndex, int neighbourPosition)
        {
            if (neighbourPosition is < 0 or > AudioTileNeighboursInfo.MAX_INDEX) return 0;
            neighboursInfo[neighbourPosition] = neighbourIndex;
            return 1;
        }

        /// <summary>
        ///     Check if neighbour at specified index exists
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasNeighbour(int neighbourPosition) => neighboursInfo[neighbourPosition] != -1;

        /// <summary>
        ///     Get neighbour at specified position in array
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNeighbourIndex(int neighbourPosition) => neighboursInfo[neighbourPosition];
    }
}