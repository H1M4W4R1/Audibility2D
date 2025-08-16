using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility
{
    /// <summary>
    ///     Utility class used to transfer system-wide notification for 2D API
    /// </summary>
    public static class AudibilitySystem2D
    {
        private static Dictionary<Tilemap, bool> IsDirtyCache { get; } = new();

        internal static bool IsDirty([NotNull] Tilemap tilemap)
        {
            if (IsDirtyCache.TryGetValue(tilemap, out bool value)) return value;
            SetDirty(tilemap, true);
            return true;
        }

        internal static void SetDirtyAll(bool value)
        {
            for (int i = IsDirtyCache.Count - 1; i >= 0; i--) {
                KeyValuePair<Tilemap, bool> item = IsDirtyCache.ElementAt(i);
                Tilemap key = item.Key;
                IsDirtyCache[key] = value;
            }
        }
        
        internal static void SetDirty([NotNull] Tilemap tilemap, bool value)
        {
            IsDirtyCache[tilemap] = value;
        }

        static AudibilitySystem2D()
        {
            // This will be removed on domain reload, so I don't care it's here
            AudibilitySystem.OnSystemDirty += () => SetDirtyAll(true);
        }

    }
}