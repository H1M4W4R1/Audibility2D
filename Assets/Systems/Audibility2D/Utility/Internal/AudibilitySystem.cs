using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Utility.Internal
{
    /// <summary>
    ///     Utility class used to transfer system-wide notification for 2D API
    /// </summary>
    internal static class AudibilitySystem
    {
        /// <summary>
        ///     Cache used to store data of all Tilemaps used in Audibility system
        /// </summary>
        private static Dictionary<Tilemap, bool> IsDirtyCache { get; } = new();

        /// <summary>
        ///     Check if specified tilemap is dirty and needs to refresh cached data
        /// </summary>
        internal static bool IsDirty([NotNull] Tilemap tilemap)
        {
            if (IsDirtyCache.TryGetValue(tilemap, out bool value)) return value;
            SetDirty(tilemap, true);
            return true;
        }

        /// <summary>
        ///     Set all tilemaps to specified dirtiness value
        /// </summary>
        internal static void SetDirtyAll(bool value)
        {
            for (int i = IsDirtyCache.Count - 1; i >= 0; i--) {
                KeyValuePair<Tilemap, bool> item = IsDirtyCache.ElementAt(i);
                Tilemap key = item.Key;
                IsDirtyCache[key] = value;
            }
        }
        
        /// <summary>
        ///     Set specified tilemap to specified dirtiness value
        /// </summary>
        internal static void SetDirty([NotNull] Tilemap tilemap, bool value)
        {
            IsDirtyCache[tilemap] = value;
        }
    }
}