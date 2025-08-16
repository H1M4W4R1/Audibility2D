namespace Systems.Audibility2D.Utility
{
    /// <summary>
    ///     Internal class used to reference AudibilitySystem2D and 3D using shared events
    ///     which makes it easier to send some notifications that do not depend on any of those
    ///     systems, but are shared among them.
    /// </summary>
    internal static class AudibilitySystem
    {
        internal delegate void SystemIsDirtyHandler();

        /// <summary>
        ///     
        /// </summary>
        internal static event SystemIsDirtyHandler OnSystemDirty;

        internal static void NotifySystemDirty()
        {
            OnSystemDirty?.Invoke();
        }
    }
}