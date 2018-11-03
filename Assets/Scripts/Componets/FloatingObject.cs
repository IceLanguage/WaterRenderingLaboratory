using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LinHowe.WaterRender;

namespace LinHowe
{
    /// <summary>
    /// 浮体
    /// </summary>

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshFilter))]
    public class FloatingObject : Object
    {
        public int VoxelSize = 1;
        public float dragInWater = 1f,angularDragInWater = 1f;//阻力

        private Rigidbody m_Rigidbody;
        private float m_Volume;//体积
        private float m_Density;//密度
        private float m_Mass;//质量
        private Vector3[] m_Voxels;//体素
        private Vector3 voxelSize;
        private float initialDrag, initialAngularDrag;

        protected override void Init()
        {
            
            if(VoxelSize<1)
                VoxelSize = 1;
            m_Rigidbody = gameObject.GetComponent<Rigidbody>();
            initialDrag = m_Rigidbody.drag;
            initialAngularDrag = m_Rigidbody.angularDrag;

          
            m_Mass = m_Rigidbody.mass;

            CalualateVolume();
            m_Density = m_Mass / m_Volume;
            CalualateVoxels();
        }

        /// <summary>
        /// 计算体积
        /// </summary>
        private void CalualateVolume()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            Mesh mesh = mf.mesh;
            float volume = 0f;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                Vector3 p1 = vertices[triangles[i + 0]];
                Vector3 p2 = vertices[triangles[i + 1]];
                Vector3 p3 = vertices[triangles[i + 2]];

                Vector3 a = p1 - p2;
                Vector3 b = p1 - p3;
                Vector3 c = p1 - Vector3.zero;

                volume += (Vector3.Dot(a, Vector3.Cross(b, c))) / 6f;

            }

            m_Volume = Mathf.Abs(volume) * transform.localScale.x * transform.localScale.y * transform.localScale.z;
        }

        private void CalualateVoxels()
        {
            Quaternion initialRotation = this.transform.rotation;
            this.transform.rotation = Quaternion.identity;

            Bounds bounds = m_Bounds;
            this.voxelSize.x = bounds.size.x / VoxelSize;
            this.voxelSize.y = bounds.size.y / VoxelSize;
            this.voxelSize.z = bounds.size.z / VoxelSize;
            List<Vector3> voxels = new List<Vector3>( VoxelSize * VoxelSize * VoxelSize);

            for (int j = 0; j < VoxelSize; j++)
            {
                for (int i = 0; i < VoxelSize; i++)
                {
                    for (int k = 0; k < VoxelSize; k++)
                    {
                        float pX = bounds.min.x + this.voxelSize.x * (0.5f + i);
                        float pY = bounds.min.y + this.voxelSize.y * (0.5f + j);
                        float pZ = bounds.min.z + this.voxelSize.z * (0.5f + k);

                        Vector3 point = new Vector3(pX, pY, pZ);
                        if (IsPointInsideCollider(point))
                        {
                            voxels.Add(this.transform.InverseTransformPoint(point));
                        }
                    }
                }
            }

            transform.rotation = initialRotation;

            m_Voxels = voxels.ToArray();
        }

        private bool IsPointInsideCollider(Vector3 point)
        {
            float rayLength = m_Bounds.size.magnitude;
            Ray ray = new Ray(point, m_Collider.transform.position - point);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayLength))
            {
                if (hit.collider == m_Collider)
                {
                    return false;
                }
            }

            return true;
        }


        private void FixedUpdate()
        {
            float voxelHeight = voxelSize.y;
            int len = m_Voxels.Length;
            int planeVoxels = VoxelSize * VoxelSize;
            Vector3 worldBoundsMin = transform.TransformPoint(m_Bounds.min);
            foreach (WaterSurface water in waterSurfacesList)
            {
                Vector3 force = water.Density * m_Volume * -Physics.gravity / m_Voxels.Length;
               
                float submergedVolume = 0f;
                
                for (int i = 0; i < len; i++)
                {
                    Vector3 worldPoint = transform.TransformPoint(m_Voxels[i]);
                    float submergedFactor = 0;
                    float level = i / planeVoxels + voxelSize.y*0.5f;
                    if (worldPoint.y < water.transform.position.y)
                        submergedFactor = level;
                    submergedVolume += submergedFactor;

                    Vector3 surfaceNormal = water.GetSurfaceNormal(worldPoint);
                    Quaternion surfaceRotation = Quaternion.FromToRotation(water.transform.up, surfaceNormal);
                    surfaceRotation = Quaternion.Slerp(surfaceRotation, Quaternion.identity, submergedFactor);

                    Vector3 finalVoxelForce = surfaceRotation * force * submergedFactor;
                    m_Rigidbody.AddForceAtPosition(finalVoxelForce, worldPoint);

                    Debug.DrawLine(worldPoint, worldPoint + finalVoxelForce.normalized, Color.blue);
                }

                submergedVolume /= len;

                m_Rigidbody.drag = Mathf.Lerp(this.initialDrag, this.dragInWater, submergedVolume);
                m_Rigidbody.angularDrag = Mathf.Lerp(this.initialAngularDrag, this.angularDragInWater, submergedVolume);
            }
        }

        
    }
}