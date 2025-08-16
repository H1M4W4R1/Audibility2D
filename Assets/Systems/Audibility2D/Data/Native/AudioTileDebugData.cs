using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Systems.Audibility2D.Data.Native
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct AudioTileDebugData // 16B
    {
        [FieldOffset(0)] public readonly float normalizedLoudness;
        [FieldOffset(4)] public readonly float3 worldPosition;

        public AudioTileDebugData(float3 worldPosition, float normalizedLoudness)
        {
            this.worldPosition = worldPosition;
            this.normalizedLoudness = normalizedLoudness;
        }
    }
}