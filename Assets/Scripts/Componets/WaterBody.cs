using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LinHowe.WaterRender
{
    public class WaterBody : MonoBehaviour
    {
        //需要配置的参数
        public float width, length, cellSize;//水宽度，长度，单元格大小
        public float depth;//水深度
        public Material material;//水体材质

        private MeshRenderer mr;
        private MeshFilter mf;
        private Mesh mesh;

        private void Start()
        {
            InitComponent();
            InitMesh();
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

        private void InitMesh()
        {
            int xsize = Mathf.RoundToInt(width / cellSize);
            int ysize = Mathf.RoundToInt(length / cellSize);

            mesh = new Mesh();

            int ListCapacity = 4 * (ysize + 1) + 4 * (xsize + 1);
            List<Vector3> vertexList = new List<Vector3>(ListCapacity);
            List<Vector3> normalList = new List<Vector3>(ListCapacity);
            List<Vector2> uvList = new List<Vector2>(ListCapacity);
            List<Color> colorList = new List<Color>(ListCapacity);
            List<int> indexList = new List<int>(ListCapacity * 3);

            float xcellsize = width / xsize;
            float uvxcellsize = 1.0f / xsize;
            float ycellsize = length / ysize;
            float uvycellsize = 1.0f / ysize;

            for (int i = 0; i <= ysize; i++)
            {
                vertexList.Add(new Vector3(-width * 0.5f, -depth, -length * 0.5f + i * ycellsize));
                vertexList.Add(new Vector3(-width * 0.5f, 0, -length * 0.5f + i * ycellsize));
                vertexList.Add(new Vector3(width * 0.5f, -depth, -length * 0.5f + i * ycellsize));
                vertexList.Add(new Vector3(width * 0.5f, 0, -length * 0.5f + i * ycellsize));
                normalList.Add(Vector3.left);
                normalList.Add(Vector3.left);
                normalList.Add(Vector3.right);
                normalList.Add(Vector3.right);
                colorList.Add(Color.white);
                colorList.Add(new Color(1, 1, 1, 0));
                colorList.Add(Color.white);
                colorList.Add(new Color(1, 1, 1, 0));
                uvList.Add(new Vector2(0, i * uvycellsize));
                uvList.Add(new Vector2(0, i * uvycellsize));
                uvList.Add(new Vector2(1, i * uvycellsize));
                uvList.Add(new Vector2(1, i * uvycellsize));

                if (i < ysize)
                {
                    indexList.Add(i * 4);
                    indexList.Add((i + 1) * 4 + 1);
                    indexList.Add(i * 4 + 1);
                    indexList.Add(i * 4);
                    indexList.Add((i + 1) * 4);
                    indexList.Add((i + 1) * 4 + 1);

                    indexList.Add((i + 1) * 4 + 2);
                    indexList.Add(i * 4 + 3);
                    indexList.Add((i + 1) * 4 + 3);
                    indexList.Add((i + 1) * 4 + 2);
                    indexList.Add(i * 4 + 2);
                    indexList.Add(i * 4 + 3);
                }
            }

            for (int j = 0; j <= xsize; j++)
            {
                vertexList.Add(new Vector3(-width * 0.5f + j * xcellsize, -depth, -length * 0.5f));
                vertexList.Add(new Vector3(-width * 0.5f + j * xcellsize, 0, -length * 0.5f));
                vertexList.Add(new Vector3(-width * 0.5f + j * xcellsize, -depth, length * 0.5f));
                vertexList.Add(new Vector3(-width * 0.5f + j * xcellsize, 0, length * 0.5f));
                normalList.Add(Vector3.back);
                normalList.Add(Vector3.back);
                normalList.Add(Vector3.forward);
                normalList.Add(Vector3.forward);
                colorList.Add(Color.white);
                colorList.Add(new Color(1, 1, 1, 0));
                colorList.Add(Color.white);
                colorList.Add(new Color(1, 1, 1, 0));
                uvList.Add(new Vector2(j * uvxcellsize, 0));
                uvList.Add(new Vector2(j * uvxcellsize, 0));
                uvList.Add(new Vector2(j * uvxcellsize, 1));
                uvList.Add(new Vector2(j * uvxcellsize, 1));

                if (j < xsize)
                {
                    indexList.Add((ysize + 1) * 4 + j * 4);
                    indexList.Add((ysize + 1) * 4 + j * 4 + 1);
                    indexList.Add((ysize + 1) * 4 + (j + 1) * 4 + 1);
                    indexList.Add((ysize + 1) * 4 + j * 4);
                    indexList.Add((ysize + 1) * 4 + (j + 1) * 4 + 1);
                    indexList.Add((ysize + 1) * 4 + (j + 1) * 4);

                    indexList.Add((ysize + 1) * 4 + (j + 1) * 4 + 2);
                    indexList.Add((ysize + 1) * 4 + (j + 1) * 4 + 3);
                    indexList.Add((ysize + 1) * 4 + j * 4 + 3);
                    indexList.Add((ysize + 1) * 4 + (j + 1) * 4 + 2);
                    indexList.Add((ysize + 1) * 4 + j * 4 + 3);
                    indexList.Add((ysize + 1) * 4 + j * 4 + 2);
                }
            }

            mesh.SetVertices(vertexList);
            mesh.SetNormals(normalList);
            mesh.SetColors(colorList);
            mesh.SetUVs(0, uvList);
            mesh.SetTriangles(indexList, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            mf.sharedMesh = mesh;
            mr.sharedMaterial = material;
        }
    }
}

