namespace Systems.Audibility2D
{
    public static class Events
    {
        /// <summary>
        ///     Event raised when any muffling material data is changed
        /// </summary>
        public static Delegates.MufflingMaterialDataChangedHandler OnMufflingMaterialDataChanged;
        
        /// <summary>
        ///     Event raised when any material assigned to AudioTile is changed
        /// </summary>
        public static Delegates.MufflingMaterialChangedHandler OnMufflingMaterialChanged;

        /// <summary>
        ///     Event raised when tile gets updated
        /// </summary>
        public static Delegates.TileUpdatedHandler OnTileUpdated;
    }
}