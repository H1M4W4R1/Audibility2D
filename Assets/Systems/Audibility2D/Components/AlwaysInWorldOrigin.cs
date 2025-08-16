using UnityEngine;

namespace Systems.Audibility2D.Components
{
    /// <summary>
    ///     Utility script to enforce object to be always in world origin position (0,0,0)
    ///     Works only in Editor.
    /// </summary>
    [ExecuteInEditMode] public sealed class AlwaysInWorldOrigin : MonoBehaviour
    {
#if UNITY_EDITOR
        private void Update()
        {
            transform.position = Vector3.zero;
        }
#endif
    }
}