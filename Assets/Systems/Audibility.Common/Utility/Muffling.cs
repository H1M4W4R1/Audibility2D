using Systems.Audibility.Common.Data;

namespace Systems.Audibility.Common.Utility
{
    /// <summary>
    ///     Universal collection of muffling values for reference
    /// </summary>
    public static class Muffling
    {
        public const byte NONE = 0;
        public static readonly DecibelLevel CONCRETE = new(45, 52, 62, 75);

    }
}