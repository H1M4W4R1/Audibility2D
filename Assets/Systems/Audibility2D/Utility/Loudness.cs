namespace Systems.Audibility2D.Utility
{
    /// <summary>
    ///     Universal collection of loudness values for reference
    /// </summary>
    public static class Loudness
    {
        public const byte NONE = 0;
        public const byte SILENCE = NONE;
        public const byte WHISPER = 15;
        public const byte LIBRARY = 45;
        public const byte CONVERSATION = 60;
        public const byte LIGHT_TRAFFIC = 75;
        public const byte HEAVY_TRAFFIC = 85;
        public const byte NOISY_PLACE = 90;
        public const byte JET_ENGINE = 120;
        public const byte GUNSHOT = 157;
        public const byte MAX = 160;
    }
}