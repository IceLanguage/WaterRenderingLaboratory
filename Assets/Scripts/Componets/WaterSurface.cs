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
        public Material material;//水面材质
        public float depth;//水面深度
        public int MapSize;//纹理单元格大小
        public float Velocity = 1f;//波速
        public float Viscosity = 0.894f;//粘度系数
        public Shader waveEquationShader, normalGenerateShader, forceShader;
        public float forceFactor;
        public float Density = 1f;
        private MeshRenderer mr;
        private MeshFilter mf;
        private Mesh mesh;

        private Vector4 waveParams; //波形参数

        private float d;//单元间隔

        private int xsize, ysize;
        private float xcellsize, uvxcellsize, ycellsize, uvycellsize;
        private List<Vector3> vertexList;
        private Vector3[] curVertexs;
        public WaterCamera M_Camera { get; set; }
        //public void CalculateUV(Vector3 worldPos,out float u,out float v)
        //{
        //    float xzero = -width * 0.5f;
        //    float zzero = -length * 0.5f;
        //    u = (worldPos.x - xzero) / xcellsize * uvxcellsize;
        //    v = (worldPos.z - zzero) / ycellsize * uvycellsize;
        //}

        public Vector3 GetSurfaceNormal(Vector3 worldPoint)
        {
            Vector3[] meshPolygon = this.GetSurroundingTrianglePolygon(worldPoint);
            if (meshPolygon != null)
            {
                Vector3 planeV1 = meshPolygon[1] - meshPolygon[0];
                Vector3 planeV2 = meshPolygon[2] - meshPolygon[0];
                Vector3 planeNormal = Vector3.Cross(planeV1, planeV2).normalized;
                if (planeNormal.y < 0f)
                {
                    planeNormal *= -1f;
                }

                return planeNormal;
            }

            return transform.up;
        }

        private int GetIndex(int x,int z)
        {
            return z * (xsize + 1) + x;
        }
        public Vector3[] GetSurroundingTrianglePolygon(Vector3 worldPoint)
        {
            Vector3 localPoint = this.transform.InverseTransformPoint(worldPoint);
            int x = Mathf.CeilToInt((localPoint.x + width/2f)/ xcellsize);
            int z = Mathf.CeilToInt((localPoint.z + length / 2f) / ycellsize);
            if (x <= 0 || z <= 0 || x >= (xsize + 1) || z >= (ysize + 1))
            {
                return null;
            }

            Vector3[] trianglePolygon = new Vector3[3];
            
            if ((worldPoint - vertexList[GetIndex(x, z)]).sqrMagnitude <
                ((worldPoint - vertexList[GetIndex(x - 1, z - 1)]).sqrMagnitude))
            {
                trianglePolygon[0] = curVertexs[GetIndex(x, z)];
            }
            else
            {
                trianglePolygon[0] = curVertexs[GetIndex(x - 1, z - 1)];
            }

            trianglePolygon[1] = curVertexs[GetIndex(x - 1, z)];
            trianglePolygon[2] = curVertexs[GetIndex(x, z - 1)];

            return trianglePolygon;
        }
        private void Update()
        {
            curVertexs = mesh.vertices;
            int len = curVertexs.Length;
            for (int i = 0;i<len;++i)
            {
                curVertexs[i] = transform.TransformPoint(curVertexs[i]);
            }
        }
        void Start()
        {
            gameObject.AddComponent<ReflectCamera>();
            d = 1.0f / MapSize;

            if (!CheckSupport())
                return;
            InitWaterCamera();
            InitComponent();
            InitMesh();

            Shader.SetGlobalFloat("internal_Force", forceFactor);
        }

        public void DrawMesh(Mesh mesh, Matrix4x4 matrix)
        {
            if (M_Camera)
                M_Camera.ForceDrawMesh(mesh, matrix);
        }
        public void DrawObject(Renderer renderer)
        {
            if (M_Camera)
                M_Camera.ForceDrawRenderer(renderer);
        }
        private void InitWaterCamera()
        {
            M_Camera = new GameObject("[WaterCamera]").AddComponent<WaterCamera>();
            M_Camera.transform.SetParent(transform);
            M_Camera.transform.localPosition = Vector3.zero;
            M_Camera.transform.localEulerAngles = new Vector3(90, 0, 0);
            if(null == waveEquationShader||null == normalGenerateShader || null == forceShader)
            {
                Debug.LogError("请配置波动方程所需要的shader");
                return;
            }
            M_Camera.InitMat(waveEquationShader, normalGenerateShader, forceShader);
            M_Camera.Init(width, length, depth, MapSize, waveParams);
        }

        private void InitComponent()
        {
            mr = gameObject.GetComponent<MeshRenderer>();
            if (mr == null)
                mr = gameObject.AddComponent<MeshRenderer>();
            mf = gameObject.GetComponent<MeshFilter>();
            if (mf == null)
                mf = gameObject.AddComponent<MeshFilter>();
        }
        private bool CheckSupport()
        {
            if (cellSize <= 0)
            {
                Debug.LogError("网格单元格大小不允许小于等于0！");
                return false;
            }
            if (width <= 0 || length <= 0)
            {
                Debug.LogError("液体长宽不允许小于等于0！");
                return false;
            }
            if (depth <= 0)
            {
                Debug.LogError("液体深度不允许小于等于0！");
                return false;
            }


            if (!RefreshWaveParams(Velocity, Viscosity))
                return false;

            return true;
        }

        private bool RefreshWaveParams(float speed, float viscosity)
        {
            if (speed <= 0)
            {
                Debug.LogError("波速不允许小于等于0！");
                return false;
            }
            if (viscosity <= 0)
            {
                Debug.LogError("粘度系数不允许小于等于0！");
                return false;
            }
            float maxvelocity = d / (2 * Time.fixedDeltaTime) * Mathf.Sqrt(viscosity * Time.fixedDeltaTime + 2);
            float velocity = maxvelocity * speed;
            float viscositySq = viscosity * viscosity;
            float velocitySq = velocity * velocity;
            float deltaSizeSq = d * d;
            float dt = Mathf.Sqrt(viscositySq + 32 * velocitySq / (deltaSizeSq));
            float dtden = 8 * velocitySq / (deltaSizeSq);
            float maxT = (viscosity + dt) / dtden;
            float maxT2 = (viscosity - dt) / dtden;
            if (maxT2 > 0 && maxT2 < maxT)
                maxT = maxT2;
            if (maxT < Time.fixedDeltaTime)
            {
                Debug.LogError("粘度系数不符合要求");
                return false;
            }

            float fac = velocitySq * Time.fixedDeltaTime * Time.fixedDeltaTime / deltaSizeSq;
            float i = viscosity * Time.fixedDeltaTime - 2;
            float j = viscosity * Time.fixedDeltaTime + 2;

            float k1 = (4 - 8 * fac) / (j);
            float k2 = i / j;
            float k3 = 2 * fac / j;

            waveParams = new Vector4(k1, k2, k3, d);

            Velocity = speed;
            Viscosity = viscosity;

            return true;
        }
        private void InitMesh()
        {
            xsize = Mathf.RoundToInt(width / cellSize);
            ysize = Mathf.RoundToInt(length / cellSize);

            mesh = new Mesh();

            xcellsize = width / xsize;
            uvxcellsize = 1.0f / xsize;
            ycellsize = length / ysize;
            uvycellsize = 1.0f / ysize;

            int ListCapacity = (ysize + 1) * (xsize + 1);
            vertexList = new List<Vector3>(ListCapacity);
            List<Vector2> uvList = new List<Vector2>(ListCapacity);
            List<Vector3> normalList = new List<Vector3>(ListCapacity);
            List<int> indexList = new List<int>(ListCapacity * 6);

            for (int i = 0; i <= ysize; i++)
            {
                for (int j = 0; j <= xsize; j++)
                {
                    vertexList.Add(new Vector3(-width * 0.5f + j * xcellsize, 0, -length * 0.5f + i * ycellsize));
                    uvList.Add(new Vector2(j * uvxcellsize, i * uvycellsize));
                    normalList.Add(Vector3.up);

                    if (i < ysize && j < xsize)
                    {
                        int k = i * (xsize + 1) + j;
                        indexList.Add(k);
                        indexList.Add(k + xsize + 1);
                        indexList.Add(k + xsize + 2);

                        indexList.Add(k);
                        indexList.Add(k + xsize + 2);
                        indexList.Add(k + 1);
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
            curVertexs = vertexList.ToArray();


        }
    }
}