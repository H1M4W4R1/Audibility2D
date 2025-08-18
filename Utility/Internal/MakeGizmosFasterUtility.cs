using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.Audibility2D.Utility.Internal
{
    [BurstCompile]
    public static class MakeGizmosFasterUtility
    {
        /// <summary>
        ///     Quickly check if point is inside view frustrum
        /// </summary>
        [BurstCompile]
        public static bool PointInFrustum(in float3 point, in NativeArray<float4> planes)
        {
            for (int i = 0; i < 6; i++)
            {
                float4 plane = planes[i];
                if (math.dot(new float3(plane.x, plane.y, plane.z), point) + plane.w < 0f)
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Extract frustrum planes from camera
        /// </summary>
        /// <remarks>
        ///     NativeArray must be preallocated with size of 6 to make this work properly
        /// </remarks>
        [BurstDiscard] public static void ExtractFrustumPlanes(
            [NotNull] this Camera camera,
            ref NativeArray<float4> planes)
        {
            Matrix4x4 projectionMatrix = camera.projectionMatrix;
            Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
            ExtractFrustumPlanes(projectionMatrix, viewMatrix, ref planes);
        }
        
        /// <summary>
        ///     Extract frustrum planes from camera planes
        /// </summary>
        /// <remarks>
        ///     NativeArray must be preallocated with size of 6 to make this work properly
        /// </remarks>
        [BurstCompile] public static void ExtractFrustumPlanes(
            in Matrix4x4 projectionMatrix, in Matrix4x4 worldToCameraMatrix, ref NativeArray<float4> planes)
        {
            // Assume that planes length must be six
            Assert.IsTrue(planes.IsCreated, "Planes array must be created");
            Assert.IsTrue(planes.Length == 6, "Planes array length must be 6");

            Matrix4x4 mat = projectionMatrix * worldToCameraMatrix;

            // Left
            planes[0] = new float4(mat.m30 + mat.m00, mat.m31 + mat.m01, mat.m32 + mat.m02, mat.m33 + mat.m03);
            // Right
            planes[1] = new float4(mat.m30 - mat.m00, mat.m31 - mat.m01, mat.m32 - mat.m02, mat.m33 - mat.m03);
            // Bottom
            planes[2] = new float4(mat.m30 + mat.m10, mat.m31 + mat.m11, mat.m32 + mat.m12, mat.m33 + mat.m13);
            // Top
            planes[3] = new float4(mat.m30 - mat.m10, mat.m31 - mat.m11, mat.m32 - mat.m12, mat.m33 - mat.m13);
            // Near
            planes[4] = new float4(mat.m30 + mat.m20, mat.m31 + mat.m21, mat.m32 + mat.m22, mat.m33 + mat.m23);
            // Far
            planes[5] = new float4(mat.m30 - mat.m20, mat.m31 - mat.m21, mat.m32 - mat.m22, mat.m33 - mat.m23);

            // Normalize planes (optional, for consistent distances)
            for (int i = 0; i < 6; i++)
            {
                float3 normal = new(planes[i].x, planes[i].y, planes[i].z);
                float length = math.length(normal);
                planes[i] /= length;
            }
        }
        
    }
}