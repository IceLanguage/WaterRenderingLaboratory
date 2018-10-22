using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LinHowe.WaterRender
{
    /// <summary>
    /// 水面
    /// </summary>
    public class WaterSurface : MonoBehaviour
    {

        //需要配置的参数
        public float width, length, cellSize;//水面网格宽度，长度，单元格大小
        public Material material;//水材质
        public float depth;//水面深度
        public int MapSize;//纹理单元格大小
        public float Velocity = 1f;//波速
        public float Viscosity = 0.894f;//粘度系数

        private MeshRenderer mr;
        private MeshFilter mf;
        private Mesh mesh;

        private List<Vector3> vertexList;
        private List<Vector2> uvList;
        private List<Vector3> normalList;
        private List<int> indexList;

        private Vector3 waveParams; //波形参数

        private float d;//单元间隔

        private WaterCamera waterCamera;
        public WaterCamera Camera
        {
            get
            {
                if(null == waterCamera)
                {
                    InitWaterCamera();
                    
                }
                return waterCamera;
            }
        }
        private void Start()
        {
            if(!CheckParams()) return;
       
            InitMesh();
            InitWaterCamera();
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitComponents()
        {
            mr = GetComponent<MeshRenderer>();
            if (null == mr)
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

            if (cellSize <= 1)
                cellSize = 1f;
            if (width < cellSize)
                width = cellSize;
            if (length < cellSize)
                length = cellSize;

            int xsize = Mathf.RoundToInt(width / cellSize);
            int ysize = Mathf.RoundToInt(length / cellSize);

            mesh = new Mesh();

            float xcellsize = width / xsize;
            float uvxcellsize = 1.0f / xsize;
            float ycellsize = length / ysize;
            float uvycellsize = 1.0f / ysize;

            int ListCapacity = (ysize + 1) * (xsize + 1);
            vertexList = new List<Vector3>(ListCapacity);
            uvList = new List<Vector2>(ListCapacity);
            normalList = new List<Vector3>(ListCapacity);
            indexList = new List<int>(ListCapacity * 6);

            for (int i = 0; i <= ysize; i++)
            {
                for (int j = 0; j <= xsize; j++)
                {
                    vertexList.Add(new Vector3(-width * 0.5f + j * xcellsize, 0, -length * 0.5f + i * ycellsize));
                    uvList.Add(new Vector2(j * uvxcellsize, i * uvycellsize));
                    normalList.Add(Vector3.up);

                    if (i < ysize && j < xsize)
                    {
                        indexList.Add(i * (xsize + 1) + j);
                        indexList.Add((i + 1) * (xsize + 1) + j);
                        indexList.Add((i + 1) * (xsize + 1) + j + 1);

                        indexList.Add(i * (xsize + 1) + j);
                        indexList.Add((i + 1) * (xsize + 1) + j + 1);
                        indexList.Add(i * (xsize + 1) + j + 1);
                    }
                }
            }

            mesh.SetVertices(vertexList);
            mesh.SetUVs(0, uvList);
            mesh.SetNormals(normalList);
            mesh.SetTriangles(indexList, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            mf.sharedMesh = mesh;
            mr.sharedMaterial = material;
        }

        /// <summary>
        /// 初始化水面摄像头
        /// </summary>
        private void InitWaterCamera()
        {
            if (null != waterCamera) return;
            waterCamera = new GameObject("[WaterCamera]")
                .AddComponent<WaterCamera>();
            waterCamera.transform.SetParent(transform);
            waterCamera.transform.localPosition = Vector3.zero;
            waterCamera.transform.localEulerAngles = new Vector3(90, 0, 0);
            waterCamera.Init(width, length, depth, MapSize);
            waterCamera.SetWaveParams(waveParams);
        }

        /// <summary>
        /// 检查配置的参数
        /// </summary>
        private bool CheckParams()
        {
            if (cellSize <= 0)
            {
                Debug.LogError("单元格大小需大于0");
                return false;
            }
            d = 1 / cellSize;
            if (width <= 0 || length <=0||depth<=0)
            {
                Debug.LogError("水的高度宽度深度需大于0");
                return false;
            }
            if (Velocity <= 0)
            {
                Debug.LogError("波速需大于0");
            }
            if (Viscosity <= 0)
            {
                Debug.LogError("粘度系数不允许小于等于0！");
                return false;
            }
            float u = Viscosity, c = Velocity;
            //约束速度
            float cmax = d / 2 / Time.fixedDeltaTime * Mathf.Sqrt(u * Time.fixedDeltaTime + 2);
            c = Mathf.Clamp(c, -cmax, cmax);

            //约束时间间隔
            float tmax = (u - Mathf.Sqrt(u * u + 32 * c * c / d / d)) / (8 * c * c / d / d);
            if (tmax <= 0)
            {
                tmax = (u + Mathf.Sqrt(u * u + 32 * c * c / d / d)) / (8 * c * c / d / d);
            }
            if (tmax < Time.fixedDeltaTime)
            {
                Debug.LogError("粘度系数不符合要求");
                return false;
            }

            //计算波形参数
            float K3 = 2 * c * c * Time.fixedDeltaTime * Time.fixedDeltaTime / (d * d);
            float K1 = (4 - 4 * K3) / (u * Time.fixedDeltaTime + 2);
            float K2 = (u * Time.fixedDeltaTime - 2) / (u * Time.fixedDeltaTime + 2);
            K3 = K3 / (u * Time.fixedDeltaTime + 2);

            waveParams = new Vector4(K1, K2, K3, d);
            Viscosity = c;
            return true;
        }
    }
}