using UnityEngine;

namespace LinHowe.WaterRender
{
    /// <summary>
    /// 反射相机
    /// </summary>
    
    public class ReflectCamera : MonoBehaviour
    {
        private RenderTexture MirrorTexture;//镜像纹理
        private Camera m_camera;
        private Material WaterMat;
        private bool isInit = false,isRender =false;

        private void Init()
        {
            MirrorTexture = RenderTexture.GetTemporary(512, 512, 16);

            m_camera = new GameObject("ReflectCamera").AddComponent<Camera>();
            m_camera.enabled = false;
            m_camera.hideFlags = HideFlags.HideInHierarchy;
            Transform t = m_camera.transform;
            t.position = transform.position;
            t.rotation = transform.rotation;

            Camera curCamera = Camera.current;
            CopyCamera(curCamera, m_camera);

            WaterMat = GetComponent<Renderer>().sharedMaterial;

            MirrorTexture.isPowerOfTwo = true;

            m_camera.targetTexture = MirrorTexture;
            m_camera.cullingMask = ~(1 << 4) & -1;  //设置可以反射的物体

            WaterMat.SetTexture("_WaterReflectTexture", MirrorTexture);
        }

        private void OnDestroy()
        {
            if(MirrorTexture) RenderTexture.ReleaseTemporary(MirrorTexture);
            
        }
        private static void CopyCamera(Camera src, Camera dest)
        {

            dest.clearFlags = src.clearFlags;
            dest.backgroundColor = src.backgroundColor;
            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic = src.orthographic;
            dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
            dest.depthTextureMode = DepthTextureMode.None;
            dest.renderingPath = RenderingPath.Forward;
        }

        /// <summary>
        /// 计算反射矩阵
        /// 原理https://www.cnblogs.com/wantnon/p/5630915.html
        /// </summary>
        /// <param name="reflectionMatrix"></param>
        /// <param name="plane"></param>
        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMatrix, Vector4 plane)
        {
            reflectionMatrix.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMatrix.m01 = (-2F * plane[0] * plane[1]);
            reflectionMatrix.m02 = (-2F * plane[0] * plane[2]);
            reflectionMatrix.m03 = (-2F * plane[3] * plane[0]);

            reflectionMatrix.m10 = (-2F * plane[1] * plane[0]);
            reflectionMatrix.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMatrix.m12 = (-2F * plane[1] * plane[2]);
            reflectionMatrix.m13 = (-2F * plane[3] * plane[1]);

            reflectionMatrix.m20 = (-2F * plane[2] * plane[0]);
            reflectionMatrix.m21 = (-2F * plane[2] * plane[1]);
            reflectionMatrix.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMatrix.m23 = (-2F * plane[3] * plane[2]);

            reflectionMatrix.m30 = 0F;
            reflectionMatrix.m31 = 0F;
            reflectionMatrix.m32 = 0F;
            reflectionMatrix.m33 = 1F;
        }

        private static float SignExt(float a)
        {
            if (a > 0.0f) return 1.0f;
            if (a < 0.0f) return -1.0f;
            return 0.0f;
        }

        
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="projection"></param>
        ///// <param name="clipPlane"></param>
        //private static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
        //{
        //    Vector4 q = projection.inverse * new Vector4(SignExt(clipPlane.x), SignExt(clipPlane.y), 1.0f, 1.0f);
        //    Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));

        //    projection[2] = c.x - projection[3];
        //    projection[6] = c.y - projection[7];
        //    projection[10] = c.z - projection[11];
        //    projection[14] = c.w - projection[15];
        //}

        private void OnWillRenderObject()
        {
            if(!isInit)
            {
                Init();
                isInit = true;
            }
            if (isRender) return;
            isRender = true;

            Vector3 pos = transform.position;
            Vector3 normal = transform.up;
            Camera curCamera = Camera.current;

            //计算反射矩阵
            float d = -Vector3.Dot(normal, pos);
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);
            Matrix4x4 reflectionMatrix = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflectionMatrix, reflectionPlane);

            Vector3 oldpos = curCamera.transform.position;
            Vector3 newpos = reflectionMatrix.MultiplyPoint(oldpos);

            Matrix4x4 m = m_camera.worldToCameraMatrix = curCamera.worldToCameraMatrix * reflectionMatrix;

            //计算剪切面
            Vector3 cpos = m.MultiplyPoint(pos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized;
            Vector4 clipPlane = new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));

            //计算出剪切面相关的投影矩阵，剪切面以下内容不显示
            Matrix4x4 projection = curCamera.projectionMatrix;
            projection = curCamera.CalculateObliqueMatrix(clipPlane);
            m_camera.projectionMatrix = projection;

            //反射相机渲染
            GL.invertCulling = true;//渲染的顶点做了翻转操作，但法线没有，所以背面会消隐，该操作翻转剔除操作
            {
                m_camera.transform.position = newpos;
                Vector3 euler = curCamera.transform.eulerAngles;
                m_camera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);
                m_camera.Render();
                m_camera.transform.position = oldpos;
            }
            GL.invertCulling = false;
            isRender = false;
        }

        
    }
}
