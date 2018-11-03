using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LinHowe.WaterRender;

namespace LinHowe
{
    /// <summary>
    /// 与水交互的物体
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Object : MonoBehaviour
    {
        public List<WaterSurface> waterSurfacesList;

        protected Renderer m_Renderer;
        protected Matrix4x4 m_LocalMatrix;
        protected float heightOffset;//最高点
        protected Collider m_Collider;
        protected Bounds m_Bounds;
        protected void Start()
        {
            m_Renderer = gameObject.GetComponent<Renderer>();
            m_LocalMatrix = transform.localToWorldMatrix;
            m_Collider = GetComponent<Collider>();
            m_Bounds = m_Collider.bounds;
            heightOffset = m_Bounds.size.y / 2 + m_Bounds.center.y - transform.position.y + 0.05f;
            Init();
        }
        protected virtual void Init()
        {

        }
        protected void OnRenderObject()
        {
            if (m_Renderer && m_LocalMatrix != transform.localToWorldMatrix)
            {
                m_LocalMatrix = transform.localToWorldMatrix;
                foreach(WaterSurface water in waterSurfacesList)
                {
                    if(transform.position.y + heightOffset > water.transform.position.y)
                    {
                        water.DrawObject(m_Renderer);
                    }
                        
                }
               
            }
        }

        
    }
}

