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
        public int MapSize;

        private MeshRenderer mr;
        private MeshFilter mf;
        private Mesh mesh;

        private List<Vector3> vertexList;
        private List<Vector2> uvList;
        private List<Vector3> normalList;
        private List<int> indexList;
        private void Start()
        {

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
            mr.material = material;
        }

        /// <summary>
        /// 初始化水面摄像头
        /// </summary>
        private void InitWaterCamera()
        {
            WaterCamera waterCamera = new GameObject("[WaterCamera]")
                .AddComponent<WaterCamera>();
            waterCamera.transform.SetParent(transform);
            waterCamera.transform.localPosition = Vector3.zero;
            waterCamera.transform.localEulerAngles = new Vector3(90, 0, 0);
            waterCamera.Init(width, length, depth, MapSize);
        }
    }
}