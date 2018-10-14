using System;
using UnityEngine;

namespace LinHowe.WaterRender
{
    /// <summary>
    /// 用以获取水面的各种信息
    /// </summary>
    public class WaterCamera:MonoBehaviour
    {
        private Camera m_Camera;
        private RenderTexture CurTexture;//当前渲染纹理
        private RenderTexture HeightMap;//高度纹理贴图
        private RenderTexture NormalMap;//法线纹理贴图
        public void Init(float width, float height, float depth,int texSize)
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

            CurTexture = RenderTexture.GetTemporary(texSize, texSize, 16);
            CurTexture.name = "[CurTex]";
            CurTexture.format = RenderTextureFormat.ARGB32;

            RenderTexture tmp = RenderTexture.active;
            RenderTexture.active = CurTexture;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            HeightMap = RenderTexture.GetTemporary(texSize, texSize, 16);
            HeightMap.name = "[HeightMap]";
            HeightMap.format = RenderTextureFormat.ARGB32;
            RenderTexture.active = HeightMap;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            NormalMap = RenderTexture.GetTemporary(texSize, texSize, 16);
            NormalMap.format = RenderTextureFormat.ARGB32;
            NormalMap.name = "[NormalMap]";

            RenderTexture.active = tmp;
            m_Camera.targetTexture = CurTexture;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(destination, HeightMap);
            Graphics.Blit(source, CurTexture);
        }

        private void OnPostRender()
        {
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
        }
    }
}
