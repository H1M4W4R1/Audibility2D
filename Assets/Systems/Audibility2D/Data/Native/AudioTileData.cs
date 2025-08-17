using System.Runtime.CompilerServices;
using Systems.Audibility2D.Utility;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Audibility2D.Data.Native
{
    /// <summary>
    ///     Object to store computation data of audio-receiving tile for 2D audibility system
    /// </summary>
    public struct AudioTileData
    {
        /// <summary>
        ///     Current audio level on this tile, output value
        /// </summary>
        public AudioLoudnessLevel currentAudioLevel;

        /// <summary>
        ///     Index of this tile in array
        /// </summary>
        public readonly int index;

        /// <summary>
        ///     Position of center of this tile in world space
        /// </summary>
        public readonly float3 worldPosition;

        /// <summary>
        ///     Muffling level of this tile (used to reduce sound loudness when entering this tile)
        /// </summary>
        public readonly AudioLoudnessLevel mufflingStrength;

        /// <summary>
        ///     Node neighbours data
        /// </summary>
        public AudioTileNeighbourData neighbourData;

        public AudioTileData(
            int index,
            float3 worldPosition,
            AudioLoudnessLevel mufflingStrength)
        {
            this.index = index;
            this.worldPosition = worldPosition;
            this.mufflingStrength = mufflingStrength;

            currentAudioLevel = AudibilityLevel.LOUDNESS_NONE;
            neighbourData = AudioTileNeighbourData.New();
        }

        /// <summary>
        ///     Adds neighbour to node
        /// </summary>
        /// <returns>1 if neighbour was added, 0 if maximum count has been exceeded</returns>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SetNeighbour(int neighbourIndex, int neighbourPosition)
        {
            if (neighbourPosition is < 0 or > AudioTileNeighbourData.MAX_INDEX) return 0;
            neighbourData[neighbourPosition] = neighbourIndex;
            return 1;
        }

        /// <summary>
        ///     Check if neighbour at specified index exists
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasNeighbour(int neighbourPosition) => neighbourData[neighbourPosition] != -1;

        /// <summary>
        ///     Get neighbour at specified position in array
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNeighbourIndex(int neighbourPosition) => neighbourData[neighbourPosition];
    }
}