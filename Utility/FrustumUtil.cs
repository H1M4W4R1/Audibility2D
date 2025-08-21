using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Systems.Audibility2D.Utility
{
    [BurstCompile] public static class FrustumUtil
    {
        /// <summary>
        ///     Build a plane from 3 points (counter-clockwise order).
        ///     Returned as float4(normal.xyz, distance).
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CreatePlane(in float3 a, in float3 b, in float3 c, out float4 plane)
        {
            float3 normal = math.normalize(math.cross(b - a, c - a));
            float distance = -math.dot(normal, a);
            plane = new float4(normal, distance);
        }

        /// <summary>
        ///     Check if a point is inside plane.
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsInside(in float3 point, in float4 plane)
        {
            return math.dot(plane.xyz, point) + plane.w >= 0f;
        }

        /// <summary>
        ///     Computes the 6 frustum planes as float4(x,y,z,d).
        /// </summary>
        [BurstCompile] public static void ExtractFrustumPlanes(
            in float3 camPos,
            in quaternion camRot,
            float verticalSize,
            float aspect,
            float near,
            float far,
            ref NativeArray<float4> planes)
        {
            // Assume that planes length must be six
            Assert.IsTrue(planes.IsCreated, "Planes array must be created");
            Assert.IsTrue(planes.Length == 6, "Planes array length must be 6");
            
            // Basis vectors
            float3 forward = math.mul(camRot, new float3(0, 0, 1));
            float3 up = math.mul(camRot, new float3(0, 1, 0));
            float3 rightV = math.mul(camRot, new float3(1, 0, 0));

            // Near/Far centers
            float3 nearCenter = camPos + forward * near;
            float3 farCenter = camPos + forward * far;

            // Half sizes
            float halfFarHeight = verticalSize * 0.5f;
            float halfFarWidth = halfFarHeight * aspect;
            float halfNearHeight = halfFarHeight * (near / far);
            float halfNearWidth = halfNearHeight * aspect;

            // Near plane corners
            float3 ntl = nearCenter + up * halfNearHeight - rightV * halfNearWidth;
            float3 ntr = nearCenter + up * halfNearHeight + rightV * halfNearWidth;
            float3 nbl = nearCenter - up * halfNearHeight - rightV * halfNearWidth;
            float3 nbr = nearCenter - up * halfNearHeight + rightV * halfNearWidth;

            // Far plane corners
            float3 ftl = farCenter + up * halfFarHeight - rightV * halfFarWidth;
            float3 ftr = farCenter + up * halfFarHeight + rightV * halfFarWidth;
            float3 fbl = farCenter - up * halfFarHeight - rightV * halfFarWidth;
            float3 fbr = farCenter - up * halfFarHeight + rightV * halfFarWidth;

            // Planes
            CreatePlane(camPos, nbl, fbl, out float4 left);
            CreatePlane(camPos, fbr, nbr, out float4 right);
            CreatePlane(camPos, ntr, ftr, out float4 top);
            CreatePlane(camPos, fbl, nbl, out float4 bottom);
            CreatePlane(nbl, ntr, ntl, out float4 nearP);
            CreatePlane(ftr, fbr, ftl, out float4 farP);

            planes[0] = left;
            planes[1] = top;
            planes[2] = right;
            planes[3] = bottom;
            planes[4] = nearP;
            planes[5] = farP;
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
            // Assume that planes length must be six
            Assert.IsTrue(planes.IsCreated, "Planes array must be created");
            Assert.IsTrue(planes.Length == 6, "Planes array length must be 6");
            
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
            in Matrix4x4 projectionMatrix,
            in Matrix4x4 worldToCameraMatrix,
            ref NativeArray<float4> planes)
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

        /// <summary>
        ///     Check if a point is inside frustum. 
        /// </summary>
        [BurstCompile] [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsPointInFrustum(
            in float3 point,
            in float4 left,
            in float4 right,
            in float4 top,
            in float4 bottom,
            in float4 nearP,
            in float4 farP)
        {
            return IsInside(point, left) &&
                   IsInside(point, right) &&
                   IsInside(point, top) &&
                   IsInside(point, bottom) &&
                   IsInside(point, nearP) &&
                   IsInside(point, farP);
        }

        /// <summary>
        ///     Quickly check if point is inside view frustrum
        /// </summary>
        [BurstCompile] public static bool IsPointInFrustum(in float3 point, in NativeArray<float4> planes)
        {
            Assert.IsTrue(planes.IsCreated, "Planes array must be created");
            Assert.AreEqual(6, planes.Length, "Planes array length must be 6");
            for (int i = 0; i < 6; i++)
            {
                if (!IsInside(point, planes[i])) return false;
            }

            return true;
        }

        /// <summary>
        ///     Compute intersection point of 3 planes. Returns false if parallel.
        /// </summary>
        [UsedImplicitly] [BurstCompile] private static bool Intersect3Planes(
            in float4 p1,
            in float4 p2,
            in float4 p3,
            out float3 point)
        {
            float3 n1 = p1.xyz;
            float3 n2 = p2.xyz;
            float3 n3 = p3.xyz;

            float det = math.dot(n1, math.cross(n2, n3));
            if (math.abs(det) < 1e-6f)
            {
                point = float3.zero;
                return false; // planes nearly parallel
            }

            float3 c1 = math.cross(n2, n3) * -p1.w;
            float3 c2 = math.cross(n3, n1) * -p2.w;
            float3 c3 = math.cross(n1, n2) * -p3.w;

            point = (c1 + c2 + c3) / det;
            return true;
        }

        /// <summary>
        ///     Draws frustum gizmos from planes (NativeArray length = 6).
        ///     Planes must be in order: left, right, top, bottom, near, far.
        /// </summary>
        /// <remarks>
        ///     Lines array is allocated within method, you shall discard it when not used anymore.
        /// </remarks>
        [BurstCompile] public static void ComputeFrustumGizmosLines(
            in NativeArray<float4> planes,
            out NativeArray<float3x2> lines,
            Allocator linesAllocator = Allocator.TempJob)
        {
            Assert.IsTrue(planes.IsCreated, "Planes array must be created");
            Assert.AreEqual(planes.Length, 6, "Planes array length must be 6");

            // Frustum lines array
            lines = new NativeArray<float3x2>(12, linesAllocator);

            // Extract planes
            float4 left = planes[0];
            float4 right = planes[1];
            float4 top = planes[2];
            float4 bottom = planes[3];
            float4 nearP = planes[4];
            float4 farP = planes[5];

            // Compute corners
            Intersect3Planes(nearP, top, left, out float3 ntl);
            Intersect3Planes(nearP, top, right, out float3 ntr);
            Intersect3Planes(nearP, bottom, left, out float3 nbl);
            Intersect3Planes(nearP, bottom, right, out float3 nbr);

            Intersect3Planes(farP, top, left, out float3 ftl);
            Intersect3Planes(farP, top, right, out float3 ftr);
            Intersect3Planes(farP, bottom, left, out float3 fbl);
            Intersect3Planes(farP, bottom, right, out float3 fbr);

            // Near plane
            lines[0] = new float3x2(ntl, ntr);
            lines[1] = new float3x2(ntr, nbr);
            lines[2] = new float3x2(nbr, nbl);
            lines[3] = new float3x2(nbl, ntl);

            // Far plane
            lines[4] = new float3x2(ftl, ftr);
            lines[5] = new float3x2(ftr, fbr);
            lines[6] = new float3x2(fbr, fbl);
            lines[7] = new float3x2(fbl, ftl);

            // Edges
            lines[8] = new float3x2(ntl, ftl);
            lines[9] = new float3x2(ntr, ftr);
            lines[10] = new float3x2(nbl, fbl);
            lines[11] = new float3x2(nbr, fbr);
        }
    }
}