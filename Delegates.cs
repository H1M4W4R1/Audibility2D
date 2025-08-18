using Systems.Audibility2D.Components;
using Systems.Audibility2D.Data;
using Unity.Mathematics;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D
{
    public sealed class Delegates
    {
        internal delegate void MufflingMaterialChangedHandler(
            TileBase tileScriptableObject,
            AudioMufflingMaterialData newMaterial);

        internal delegate void MufflingMaterialDataChangedHandler(
            AudioMufflingMaterialData materialScriptableObject
        );

        internal delegate void TileUpdatedHandler(AudibilityUpdater updater, int3 tilePosition);
    }
}