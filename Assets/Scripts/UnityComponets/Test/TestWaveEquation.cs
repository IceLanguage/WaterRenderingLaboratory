using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace LinHowe.WaveEquation
{
    /// <summary>
    /// 测试波动方程
    /// </summary>
    public class TestWaveEquation : MonoBehaviour
    {
        //需要配置的参数
        public int Xsize = 90;//x轴绘制的精度
        public Material mat = null;//水材质
        public float u = 0.894f;//液体粘稠度
        

        private MeshRenderer mr;
        private MeshFilter mf;
        
        private Mesh mesh;

        //波形顶点位置
        private List<Vector3> VertexList = new List<Vector3>();

        //记录的前个时间点的波形位置
        private List<Vector3> LastVertexList = new List<Vector3>();

        //记录的下个时间点的波形位置
        private List<Vector3> NextVertexList = new List<Vector3>();

        private List<int> Indexes = new List<int>();

        //波速和dx
        public float c, d;

        private float K1, K2, K3;

        private float minX;
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

        private void Awake()
        {
            InitMesh();
            RandomWave();
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
            int PointsNumber = Xsize * 2 + 2;
            if (PointsNumber < 0)
                PointsNumber = 0;
            VertexList = new List<Vector3>(PointsNumber);
            LastVertexList = new List<Vector3>(PointsNumber);
            NextVertexList = new List<Vector3>(PointsNumber);
            Indexes = new List<int>(PointsNumber);

            DrawMesh();

            mesh.SetVertices(VertexList);
            mesh.SetTriangles(Indexes, 0);

            Vector3 ZeroCenter = CubeToLocal(Vector3.zero);

            float HalfXLength = CubeToLocal(Vector3.right).x - ZeroCenter.x;
            d = HalfXLength / (Xsize / 2);

            minX = CubeToLocal(Vector3.left).x;
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
                VertexList.Add(v1);
                VertexList.Add(v2);
                LastVertexList.Add(v1);
                LastVertexList.Add(v2);
                NextVertexList.Add(v1);
                NextVertexList.Add(v2);

                //绘制四边形
                if (i < Xsize)
                {
                    int j = i * 2;
                    Indexes.Add(j);
                    Indexes.Add(j + 1);
                    Indexes.Add(j + 3);
                    Indexes.Add(j);
                    Indexes.Add(j + 3);
                    Indexes.Add(j + 2);
                }            
            }
        }

        private void FixedUpdate()
        {
            //公式来源 Mathematics for 3D Game Programming and Computer Graphics(third Edition)
            for (int i = 1; i < Xsize; i++)
            {
                float cy = VertexList[i * 2 + 1].y;
                float ly = LastVertexList[i * 2 + 1].y;
                float cy1 = VertexList[(i - 1) * 2 + 1].y;
                float cy2 = VertexList[(i + 1) * 2 + 1].y;

                float ny = cy * K1 + ly * K2 + (cy1 + cy2) * K3;

                Vector3 p = NextVertexList[i * 2 + 1];
                p.y = ny;
                NextVertexList[i * 2 + 1] = p;
            }

            for (int i = 0; i <= Xsize; i++)
            {
                int j = i * 2 + 1;
                LastVertexList[j] = VertexList[j];
                VertexList[j] = NextVertexList[j];
            }
            mesh.Clear();
            mesh.SetVertices(VertexList);
            mesh.SetTriangles(Indexes, 0);
            
        }

        private void RandomWave()
        {

            //速度
            c = UnityEngine.Random.Range(-10f, 10f);

            //液体粘稠度大于0
            u = Mathf.Clamp(u, 0.00000001f, float.MaxValue);

            //约束速度
            float cmax = d / 2 / Time.fixedDeltaTime * Mathf.Sqrt(u * Time.fixedDeltaTime + 2);
            c = Mathf.Clamp(c, -cmax, cmax);

            //约束时间间隔
            float tmax = (u - Mathf.Sqrt(u * u + 32 * c * c / d / d)) / (8 * c * c / d / d);
            if(tmax <= 0)
            {
                tmax = (u + Mathf.Sqrt(u * u + 32 * c * c / d / d)) / (8 * c * c / d / d);
            }
            if (tmax < Time.fixedDeltaTime)
            {
                RandomWave();
                return;
            }

            K3 = 2 * c * c * Time.fixedDeltaTime * Time.fixedDeltaTime / (d * d);

            K1 = (4 - 2 * K3)/(u * Time.fixedDeltaTime + 2);

            K2 = (u * Time.fixedDeltaTime - 2) / (u * Time.fixedDeltaTime + 2);

            K3 = K3/ (u * Time.fixedDeltaTime + 2);
        }
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                AddWave(worldPos);
            }
        }
        private void AddWave(Vector3 pos)
        {
            Vector3 localPos = transform.worldToLocalMatrix.MultiplyPoint(pos);
            
            int hit = Mathf.RoundToInt((localPos.x - minX) / d);
            if (hit >= 0 && hit <= Xsize)
            {
                Vector3 p = VertexList[hit * 2 + 1];
                float force = UnityEngine.Random.Range(-0.5f, 0.5f);
                p.y += force;
                VertexList[hit * 2 + 1] = p;
            }
        }
       
    }
}

