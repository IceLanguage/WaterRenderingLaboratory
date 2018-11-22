using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LinHowe.WaterRender
{
    /// <summary>
    /// 平行光-用于渲染和投射阴影
    /// </summary>
    
    [RequireComponent(typeof(Camera))]
    public class SunLight : MonoBehaviour
    {
        public int shadowMapSize;

        public float bias;

        public float near;
        public float far;

        public float size;
        public float aspect;
        public Color color;
        public float intensity;

        public Shader shadowMapRenderShader;

        private Camera m_Camera;
        private RenderTexture m_ShadowMap;

        private Material m_BGMaterial;

        void Start()
        {

            InitRenderTarget();

            m_BGMaterial = new Material(shadowMapRenderShader);

        }

        void OnPreRender()
        {
            InitRenderTarget();
            Matrix4x4 wtl = m_Camera.worldToCameraMatrix;

            Shader.SetGlobalColor("internalWorldLightColor",
                new Color(color.r * intensity, color.g * intensity, color.b * intensity, color.a));
            Vector3 dirction = transform.rotation * - Vector3.forward;
            Shader.SetGlobalVector("internalWorldLightDir",
                dirction);
            Shader.SetGlobalVector("internalWorldLightPos",
                transform.position);
            Shader.SetGlobalMatrix("internalWorldLightMV", wtl);
            Shader.SetGlobalMatrix("internalWorldLightVP", m_Camera.projectionMatrix);
            Shader.SetGlobalVector("internalProjectionParams",
                new Vector4(0.01f, m_Camera.nearClipPlane, m_Camera.farClipPlane, 1 / m_Camera.farClipPlane));
            Shader.SetGlobalFloat("internalBias", bias);
        }

        void InitRenderTarget()
        {
            if (m_Camera == null)
            {
                m_Camera = GetComponent<Camera>();
                if (m_Camera == null)
                    m_Camera = gameObject.AddComponent<Camera>();

                m_Camera.aspect = aspect;
                m_Camera.backgroundColor = new Color(1, 1, 1, 0);
                m_Camera.clearFlags = CameraClearFlags.SolidColor;
                m_Camera.depth = 0;
                m_Camera.farClipPlane = far;
                m_Camera.nearClipPlane = near;
                m_Camera.orthographic = true;
                m_Camera.orthographicSize = size;
                m_Camera.SetReplacementShader(shadowMapRenderShader, "RenderType");
            }
            if (m_ShadowMap == null)
            {
                m_ShadowMap = RenderTexture.GetTemporary(shadowMapSize, shadowMapSize, 16);
                m_Camera.targetTexture = m_ShadowMap;
                Shader.SetGlobalTexture("internalShadowMap", m_ShadowMap);
            }

        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 0.3f, 1f, 0.6f);

            Vector3 vp1 = transform.position + transform.rotation * new Vector3(-size * aspect, -size, near);
            Vector3 vp2 = transform.position + transform.rotation * new Vector3(-size * aspect, size, near);
            Vector3 vp3 = transform.position + transform.rotation * new Vector3(size * aspect, size, near);
            Vector3 vp4 = transform.position + transform.rotation * new Vector3(size * aspect, -size, near);

            Vector3 vp5 = transform.position + transform.rotation * new Vector3(-size * aspect, -size, far);
            Vector3 vp6 = transform.position + transform.rotation * new Vector3(-size * aspect, size, far);
            Vector3 vp7 = transform.position + transform.rotation * new Vector3(size * aspect, size, far);
            Vector3 vp8 = transform.position + transform.rotation * new Vector3(size * aspect, -size, far);

            Gizmos.DrawLine(vp1, vp2);
            Gizmos.DrawLine(vp2, vp3);
            Gizmos.DrawLine(vp3, vp4);
            Gizmos.DrawLine(vp4, vp1);

            Gizmos.DrawLine(vp5, vp6);
            Gizmos.DrawLine(vp6, vp7);
            Gizmos.DrawLine(vp7, vp8);
            Gizmos.DrawLine(vp8, vp5);

            Gizmos.DrawLine(vp1, vp5);
            Gizmos.DrawLine(vp2, vp6);
            Gizmos.DrawLine(vp3, vp7);
            Gizmos.DrawLine(vp4, vp8);
        }

        private void OnDestroy()
        {
            if (m_BGMaterial)
                Destroy(m_BGMaterial);
            if (m_ShadowMap)
                RenderTexture.ReleaseTemporary(m_ShadowMap);
        }
    }
}
