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

        private IWaveComponent waveComponent;
        public void InitMat(Shader waveEquationShader, Shader normalGenerateShader, Shader forceShader)
        {
            if (waveEquationShader) waveEquationMat = new Material(waveEquationShader);
            else waveEquationMat = new Material(Shader.Find("LinHowe/WaveEquation"));

            if (normalGenerateShader) normalGenerateMat = new Material(normalGenerateShader);
            else normalGenerateMat = new Material(Shader.Find("LinHowe/NormalGenerate"));

            if (forceShader) forceMat = new Material(forceShader);
            else forceMat = new Material(Shader.Find("LinHowe/Force"));
        }

        public void Init(float width, float height, float depth, int texSize, IWaveComponent waveComponent)
        {
            this.waveComponent = waveComponent;
            waveComponent.SetWaveParams(this);
            waveComponent.InitWaterCamera(texSize);

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

            //PreTexture = RenderTexture.GetTemporary(texSize, texSize, 16);
            //PreTexture.name = "[PreTex]";
            //RenderTexture.active = PreTexture;
            //GL.Clear(false, true, new Color(0, 0, 0, 0));

            HeightMap = RenderTexture.GetTemporary(texSize, texSize, 16);
            HeightMap.name = "[HeightMap]";
            RenderTexture.active = HeightMap;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            NormalMap = RenderTexture.GetTemporary(texSize, texSize, 16);
            //NormalMap.format = RenderTextureFormat.ARGB32;
            NormalMap.name = "[NormalMap]";

            RenderTexture.active = tmp;
            m_Camera.targetTexture = CurTexture;
        }
        public void SetWaveParams(Vector4 wave)
        {
            waveEquationMat.SetVector("_WaveParams", wave);
        }
        public void ForceDrawMesh(Mesh mesh, Matrix4x4 matrix)
        {
            if (null == mesh || null == m_CommandBuffer)
                return;
            m_CommandBuffer.DrawMesh(mesh, matrix, forceMat);
        }

        public void ForceDrawRenderer(Renderer renderer)
        {
            if (!renderer)
                return;
            if (IsBoundsInCamera(renderer.bounds, m_Camera))
                m_CommandBuffer.DrawRenderer(renderer, forceMat);
        }

        /// <summary>
        /// 判断包围盒是否被相机裁剪
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        private static bool IsBoundsInCamera(Bounds bounds, Camera camera)
        {

            Matrix4x4 matrix = camera.projectionMatrix * camera.worldToCameraMatrix;

            int code =
                ComputeOutCode(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y + bounds.size.y / 2,
                    bounds.center.z + bounds.size.z / 2, 1), matrix);


            code &=
                ComputeOutCode(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y + bounds.size.y / 2,
                    bounds.center.z + bounds.size.z / 2, 1), matrix);

            code &=
                ComputeOutCode(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y - bounds.size.y / 2,
                    bounds.center.z + bounds.size.z / 2, 1), matrix);

            code &=
                ComputeOutCode(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y - bounds.size.y / 2,
                    bounds.center.z + bounds.size.z / 2, 1), matrix);

            code &=
                ComputeOutCode(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y + bounds.size.y / 2,
                    bounds.center.z - bounds.size.z / 2, 1), matrix);

            code &=
                ComputeOutCode(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y + bounds.size.y / 2,
                    bounds.center.z - bounds.size.z / 2, 1), matrix);

            code &=
                ComputeOutCode(new Vector4(bounds.center.x + bounds.size.x / 2, bounds.center.y - bounds.size.y / 2,
                    bounds.center.z - bounds.size.z / 2, 1), matrix);

            code &=
                ComputeOutCode(new Vector4(bounds.center.x - bounds.size.x / 2, bounds.center.y - bounds.size.y / 2,
                    bounds.center.z - bounds.size.z / 2, 1), matrix);


            if (code != 0) return false;

            return true;
        }

        private static int ComputeOutCode(Vector4 pos, Matrix4x4 projection)
        {
            pos = projection * pos;
            int code = 0;
            if (pos.x < -pos.w) code |= 0x01;
            if (pos.x > pos.w) code |= 0x02;
            if (pos.y < -pos.w) code |= 0x04;
            if (pos.y > pos.w) code |= 0x08;
            if (pos.z < -pos.w) code |= 0x10;
            if (pos.z > pos.w) code |= 0x20;
            return code;
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if(waveComponent!=null)
                waveComponent.OnRenderImage(src, dst, waveEquationMat);
            //waveEquationMat.SetTexture("_PreTex", PreTexture);

            //Graphics.Blit(src, dst, waveEquationMat);

            //Graphics.Blit(src, PreTexture);
            Graphics.Blit(dst, HeightMap);

            Graphics.Blit(HeightMap, NormalMap, normalGenerateMat);

            
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
            if (waveEquationMat)
                Destroy(waveEquationMat);
            if (forceMat)
                Destroy(forceMat);
            if (normalGenerateMat)
                Destroy(normalGenerateMat);
            if (null != waveComponent)
                waveComponent.OnDestroy();
        }
    }
}

