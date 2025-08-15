using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility
{
    public static class AudibilitySystem2D
    {
        private static Dictionary<Tilemap, bool> IsDirtyCache { get; } = new();

        public static bool IsDirty([NotNull] Tilemap tilemap)
        {
            if (IsDirtyCache.TryGetValue(tilemap, out bool value)) return value;
            SetDirty(tilemap, true);
            return true;
        }

        public static void SetDirtyAll(bool value)
        {
            foreach (KeyValuePair<Tilemap, bool> kvp in IsDirtyCache)
            {
                IsDirtyCache[kvp.Key] = value;
            }
        }
        
        public static void SetDirty([NotNull] Tilemap tilemap, bool value)
        {
            IsDirtyCache[tilemap] = value;
        }
    }
}