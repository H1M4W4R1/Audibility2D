using Systems.Audibility2D.Data.Native;

namespace Systems.Audibility2D.Utility
{
    /// <summary>
    ///     Universal collection of muffling values for reference
    /// </summary>
    public static class Muffling
    {
        public const byte NONE = 0;
        public static readonly AudioLoudnessLevel CONCRETE = new(45, 52, 62, 75);

    }
}