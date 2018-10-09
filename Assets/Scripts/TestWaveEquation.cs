using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace LinHowe.WaveEquation
{
    public class TestWaveEquation : MonoBehaviour
    {
        //需要配置的参数
        public int Xsize = 90;//x轴绘制的精度
        public float height = 0f;//xOy平面上的单位正方形波形的y轴高度，取-1，1之间的数字
        public Material mat = null;//水材质

        private MeshRenderer mr;
        private MeshFilter mf;
       
        private Mesh mesh;

        private List<Vector3> m_VertexList = new List<Vector3>();
        private List<Vector3> m_LastVertexList = new List<Vector3>();
        private List<Vector3> m_NextVertexList = new List<Vector3>();
        private List<int> m_Indexes = new List<int>();

        private void Start()
        {
            InitMesh();
        }
        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitComponents()
        {
            mr = GetComponent<MeshRenderer>();
            if(null == mr)
                mr = gameObject.AddComponent<MeshRenderer>();
            mf = GetComponent<MeshFilter>();
            if (null == mf)
                mf = gameObject.AddComponent<MeshFilter>();
        }

        /// <summary>
        /// 网格初始化
        /// </summary>
        private void InitMesh()
        {
            InitComponents();

            if (mat == null)
                mat = new Material(Shader.Find("Diffuse"));
            mesh = new Mesh();
            mr.sharedMaterial = mat;
            mf.sharedMesh = mesh;

            //为List分配好容量
            int PointsNumber = Xsize * 2;
            if (PointsNumber < 0)
                PointsNumber = 0;
            m_VertexList = new List<Vector3>(PointsNumber);
            m_LastVertexList = new List<Vector3>(PointsNumber);
            m_NextVertexList = new List<Vector3>(PointsNumber);
            m_Indexes = new List<int>(PointsNumber);

            DrawMesh();

            mesh.SetVertices(m_VertexList);
            mesh.SetTriangles(m_Indexes, 0);
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        private void DrawMesh()
        {
            height = Mathf.Clamp(height, -1f, 1f);
            Vector3 p1 = new Vector3(-1, -1, 0);
            Vector3 p2 = new Vector3(-1, height, 0);
            Vector3 deleteVector = new Vector3(2f / Xsize, 0,0);
            //将xOy平面上的单位正方形转换到物体坐标系
            Func<Vector3, Vector3> CubeToLocal = (PosInCube) =>
            {
                Vector3 PosInCamera = Camera.main.projectionMatrix.inverse.MultiplyPoint(PosInCube);
                Vector3 PosInWorld = Camera.main.cameraToWorldMatrix.MultiplyPoint(PosInCamera);
                Vector3 PosInLocal = transform.worldToLocalMatrix.MultiplyPoint(PosInWorld);
                return PosInLocal;
            };

            for (int i = 0;i <= Xsize; ++i,p1 += deleteVector,p2 += deleteVector)
            {
                Vector3 v1 = CubeToLocal(p1);
                Vector3 v2 = CubeToLocal(p2);
                m_VertexList.Add(v1);
                m_VertexList.Add(v2);
                m_LastVertexList.Add(v1);
                m_LastVertexList.Add(v2);
                m_NextVertexList.Add(v1);
                m_NextVertexList.Add(v2);

                //绘制四边形
                if (i < Xsize)
                {
                    int j = i * 2;
                    m_Indexes.Add(j);
                    m_Indexes.Add(j + 1);
                    m_Indexes.Add(j + 3);
                    m_Indexes.Add(j);
                    m_Indexes.Add(j + 3);
                    m_Indexes.Add(j + 2);
                }

            
                
            }


        }
    }
}

