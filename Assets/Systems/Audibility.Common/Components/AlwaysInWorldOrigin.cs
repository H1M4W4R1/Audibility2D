using System;
using UnityEngine;

namespace Systems.Audibility.Common.Components
{
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