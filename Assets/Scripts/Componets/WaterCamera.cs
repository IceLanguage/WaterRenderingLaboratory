using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace LinHowe.WaterRender
{
    /// <summary>
    /// 用以获取水面的各种信息
    /// </summary>
    public class WaterCamera:MonoBehaviour
    {
        private Camera m_Camera;

        private RenderTexture CurTexture;//当前渲染纹理
        private RenderTexture PreTexture;//上一刻的渲染纹理
        private RenderTexture HeightMap;//高度纹理贴图
        private RenderTexture NormalMap;//法线纹理贴图

        private Material waveEquationMat;//波动方程材质
        private Material normalGenerateMat;//法线生成材质
        private Material forceMat;//力的材质
        private CommandBuffer m_CommandBuffer;
        private void Awake()
        {
            waveEquationMat = new Material(Shader.Find("LinHowe/WaveEquation"));
            normalGenerateMat = new Material(Shader.Find("LinHowe/NormalGenerate"));
            forceMat = new Material(Shader.Find("LinHowe/Force"));
        }

        public void Init(float width, float height, float depth, int texSize, Vector4 wave)
        {
            m_Camera = gameObject.AddComponent<Camera>();
            m_Camera.aspect = width / height;
            m_Camera.backgroundColor = Color.black;
            m_Camera.cullingMask = 0;
            m_Camera.depth = 0;
            m_Camera.farClipPlane = depth;
            m_Camera.nearClipPlane = 0;
            m_Camera.orthographic = true;
            m_Camera.orthographicSize = height * 0.5f;
            m_Camera.clearFlags = CameraClearFlags.Depth;
            m_Camera.allowHDR = false;
            m_CommandBuffer = new CommandBuffer();
            m_Camera.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, m_CommandBuffer);

            RenderTexture tmp = RenderTexture.active;

            CurTexture = RenderTexture.GetTemporary(texSize, texSize, 16);
            CurTexture.name = "[CurTex]";
            RenderTexture.active = CurTexture;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            PreTexture = RenderTexture.GetTemporary(texSize, texSize, 16);
            PreTexture.name = "[PreTex]";
            RenderTexture.active = PreTexture;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            HeightMap = RenderTexture.GetTemporary(texSize, texSize, 16);
            HeightMap.name = "[HeightMap]";
            //HeightMap.format = RenderTextureFormat.ARGB32;
            RenderTexture.active = HeightMap;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            NormalMap = RenderTexture.GetTemporary(texSize, texSize, 16);
            //NormalMap.format = RenderTextureFormat.ARGB32;
            NormalMap.name = "[NormalMap]";

            RenderTexture.active = tmp;
            m_Camera.targetTexture = CurTexture;

            Shader.SetGlobalFloat("internal_Force", 1.5f);
            waveEquationMat.SetVector("_WaveParams", wave);
        }

        public void ForceDrawMesh(Mesh mesh, Matrix4x4 matrix)
        {
            if (null == mesh || null == m_CommandBuffer)
                return;
            m_CommandBuffer.DrawMesh(mesh, matrix, forceMat);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            //传入前一次的高度渲染结果，以在shader中根据二位波方程计算当前高度
            waveEquationMat.SetTexture("_PreTex", PreTexture);

            Graphics.Blit(src, dst, waveEquationMat);

            Graphics.Blit(dst, HeightMap);

            Graphics.Blit(HeightMap, NormalMap, normalGenerateMat);

            Graphics.Blit(src, PreTexture);
        }

        void OnPostRender()
        {
            m_CommandBuffer.Clear();
            m_CommandBuffer.ClearRenderTarget(true, false, Color.black);
            m_CommandBuffer.SetRenderTarget(CurTexture);

            Shader.SetGlobalTexture("_WaterHeightMap", HeightMap);
            Shader.SetGlobalTexture("_WaterNormalMap", NormalMap);
        }

        private void OnDestroy()
        {
            if (HeightMap)
                RenderTexture.ReleaseTemporary(HeightMap);
            if (NormalMap)
                RenderTexture.ReleaseTemporary(NormalMap);
            if (CurTexture)
                RenderTexture.ReleaseTemporary(CurTexture);
            if (PreTexture)
                RenderTexture.ReleaseTemporary(PreTexture);
        }
    }
}

