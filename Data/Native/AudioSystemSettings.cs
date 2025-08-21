using JetBrains.Annotations;
using Systems.Audibility2D.Data.Settings;

namespace Systems.Audibility2D.Data.Native
{
    public readonly struct AudioSystemSettings
    {
        /// <summary>
        ///     Sound decay rate per in-game unit
        /// </summary>
        public readonly short soundDecayPerUnit;

        public AudioSystemSettings([NotNull] AudibilitySettings settings)
        {
            soundDecayPerUnit = settings.soundDecayPerUnit;
        }
    }
}