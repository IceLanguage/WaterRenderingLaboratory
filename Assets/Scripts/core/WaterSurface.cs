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

        public WaveComponentEnum ChooseWaveComponent;
        private IWaveComponent waveComponent;

        //存储枚举和组件的映射关系
        private readonly Dictionary<WaveComponentEnum, IWaveComponent> WaveComponentsDictionary;


        private float d;//单元间隔

        private int xsize, ysize;
        [HideInInspector]
        public float xcellsize, uvxcellsize, ycellsize, uvycellsize;
        private List<Vector3> vertexList;
        private Vector3[] curVertexs;
        public WaterCamera M_Camera { get; set; }

        //避免在Update中执行过多的运算，只更新与浮体接触的顶点数据
        private HashSet<int> NeedUpdateVertexs = new HashSet<int>();

        WaterSurface()
        {
            WaveComponentsDictionary
            = new Dictionary<WaveComponentEnum, IWaveComponent>()
            {
                {
                    WaveComponentEnum.WaveEquation,
                    new WaveEquation_Component(this)
                },
                {
                    WaveComponentEnum.SineWave,
                    new SineWave_Component(this)
                }
            };
        }

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

        private int GetIndex(int x, int z)
        {
            int index = z * (xsize + 1) + x;
            NeedUpdateVertexs.Add(index);
            return index;
        }
        public Vector3[] GetSurroundingTrianglePolygon(Vector3 worldPoint)
        {
            Vector3 localPoint = this.transform.InverseTransformPoint(worldPoint);
            int x = Mathf.CeilToInt((localPoint.x + width / 2f) / xcellsize);
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
            
            foreach (int i in NeedUpdateVertexs)
            {
                curVertexs[i] = transform.TransformPoint(curVertexs[i]);
            }
            NeedUpdateVertexs.Clear();


        }

        
        void Start()
        {
            InitComponent();
           
            waveComponent = WaveComponentsDictionary[ChooseWaveComponent];
            d = 1.0f / MapSize;
            InitMesh();
            if (!CheckSupport())
                return;
            InitWaterCamera();
            
            

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
            M_Camera.Init(width, length, depth, MapSize,waveComponent);
            
        }

        private void InitComponent()
        {
            gameObject.AddComponent<ReflectCamera>();
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


            if (!waveComponent.InitAndCheckWaveParams(Velocity, Viscosity,d))
                return false;

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