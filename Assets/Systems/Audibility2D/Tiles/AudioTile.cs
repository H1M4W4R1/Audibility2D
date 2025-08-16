using System.Runtime.CompilerServices;
using Systems.Audibility2D.Data;
using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Utility;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Audibility2D.Tiles
{
    /// <summary>
    ///     Audio tile used to compute muffling / dampening properties at specific position
    /// </summary>
    [CreateAssetMenu(menuName = "Systems/Audibility/AudioTile", fileName = "AudioTile")]
    public sealed class AudioTile : TileBase
    {
        /// <summary>
        ///     Sound loudness dampening material, see class for more information
        /// </summary>
        [Tooltip("Tile material used to define muffling levels")]
        [SerializeField] private AudioMufflingMaterialData audioMaterialData;
     
        /// <summary>
        ///     Sprite used to render tile in editor
        /// </summary>
        [field: SerializeField] public Sprite PreviewSprite { get; private set; }
        
        /// <summary>
        ///     Color used to render sprite in editor
        /// </summary>
        [field: SerializeField] public Color PreviewColor { get; private set; } = Color.gray;

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            AudibilitySystem.SetDirtyAll(true);
            return base.StartUp(position, tilemap, go);
        }

        /// <summary>
        ///     Get tile muffle levels
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public AudioLoudnessLevel GetMufflingData()
        {
            if (ReferenceEquals(audioMaterialData, null)) return AudibilityLevel.LOUDNESS_NONE;
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

        private void OnValidate()
        {
           AudibilitySystem.SetDirtyAll(true);
        }
    }
}