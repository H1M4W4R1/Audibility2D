using System.Runtime.CompilerServices;
using Systems.Audibility.Common.Data;
using Systems.Audibility.Common.Utility;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Tiles
{
    [CreateAssetMenu(menuName = "Systems/Audibility/AudioTile", fileName = "AudioTile")]
    public sealed class AudioTile : TileBase
    {
        [Tooltip("Tile material used to define muffling levels")]
        [SerializeField] private AudioMufflingMaterialData audioMaterialData;
     
        [field: SerializeField] public Sprite PreviewSprite { get; private set; }
        [field: SerializeField] public Color PreviewColor { get; private set; } = Color.gray;
        
        /// <summary>
        ///     Get tile muffle levels
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public DecibelLevel GetMufflingData()
        {
            if (ReferenceEquals(audioMaterialData, null)) return Muffling.NONE;
            return audioMaterialData.MuffleLevel;
        }

        /// <summary>
        ///     Set tile audio material to reduce or increase sound level
        /// </summary>
        public void SetAudioMaterial(AudioMufflingMaterialData audioMaterial) => audioMaterialData = audioMaterial;
        
        /// <summary>
        /// Retrieves any tile rendering data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileData">Data to render the tile.</param>
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = PreviewSprite;
            tileData.color = PreviewColor;
            //tileData.flags = TileFlags.LockTransform;
            tileData.transform = Matrix4x4.identity;
            tileData.colliderType = Tile.ColliderType.None;
        }
    }
}