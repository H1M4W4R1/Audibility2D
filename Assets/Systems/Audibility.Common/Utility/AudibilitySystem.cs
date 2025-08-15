namespace Systems.Audibility.Common.Utility
{
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