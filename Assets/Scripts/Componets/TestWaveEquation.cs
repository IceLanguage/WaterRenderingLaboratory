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
        public Material mat = null;//水材质

        private MeshRenderer mr;
        private MeshFilter mf;
        
        private Mesh mesh;

        private List<Vector3> m_VertexList = new List<Vector3>();
        private List<Vector3> m_LastVertexList = new List<Vector3>();
        private List<Vector3> m_NextVertexList = new List<Vector3>();
        private List<int> m_Indexes = new List<int>();

        private float HalfXLength, HalfYLength;
        private Func<float, float, float> waveFunction;

        private float timeTotal = 0;

        public float A, L, S;
        /// <summary>
        /// 将xOy平面上的单位正方形转换到物体坐标系
        /// </summary>
        /// <param name="PosInCube"></param>
        /// <returns></returns>
        private Vector3 CubeToLocal(Vector3 PosInCube)
        {
            Vector3 PosInCamera = Camera.main.projectionMatrix.inverse.MultiplyPoint(PosInCube);
            Vector3 PosInWorld = Camera.main.cameraToWorldMatrix.MultiplyPoint(PosInCamera);
            Vector3 PosInLocal = transform.worldToLocalMatrix.MultiplyPoint(PosInWorld);
            return PosInLocal;
        }

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

            Vector3 ZeroCenter = CubeToLocal(Vector3.zero);
            HalfXLength = (CubeToLocal(Vector3.right) - ZeroCenter).magnitude;
            HalfYLength = (CubeToLocal(Vector3.up) - ZeroCenter).magnitude;
        }

        /// <summary>
        /// 绘制网格
        /// </summary>
        private void DrawMesh()
        {

            Vector3 p1 = new Vector3(-1, -1, 0);
            Vector3 p2 = Vector3.left;
            if (Xsize < 10)
                Xsize = 10;
            Vector3 deleteVector = new Vector3(2f / Xsize, 0,0);
            

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

        void FixedUpdate()
        {
            if (null == waveFunction)
                RandomWave();
            for (int i = 0; i <= Xsize; i++)
            {
                int j = i * 2 + 1;
                Vector3 v = m_NextVertexList[j];
                v.y = waveFunction(HalfXLength * i, timeTotal);
                m_NextVertexList[j] = v;
                m_LastVertexList[j] = m_VertexList[j];
                m_VertexList[j] = m_NextVertexList[j];
            }
            mesh.Clear();
            mesh.SetVertices(m_VertexList);
            mesh.SetTriangles(m_Indexes, 0);
            timeTotal += Time.fixedDeltaTime;
        }

        [ContextMenu("随机生成波")]
        private void RandomWave()
        {
            //波长
            L = UnityEngine.Random.Range(0f, 1f);
            
            //幅度
            A = UnityEngine.Random.Range(0f, 1f);

            //速度
            S = UnityEngine.Random.Range(-1f, 1f);

            waveFunction = WaveEquation.WaveFunctions.SimplePlaneWave(A, L, S);
        }

       
    }
}

