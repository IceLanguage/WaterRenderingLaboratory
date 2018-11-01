using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LinHowe.WaterRender;

namespace LinHowe
{
    public class Object : MonoBehaviour
    {
        public List<WaterSurface> waterSurfacesList;

        private Renderer m_Renderer;
        private Matrix4x4 m_LocalMatrix;
        

        private void Start()
        {
            m_Renderer = gameObject.GetComponent<Renderer>();
            m_LocalMatrix = transform.localToWorldMatrix;
        }

        private void OnRenderObject()
        {
            if (m_Renderer && m_LocalMatrix != transform.localToWorldMatrix)
            {
                m_LocalMatrix = transform.localToWorldMatrix;
                foreach(WaterSurface water in waterSurfacesList)
                {
                    water.DrawObject(m_Renderer);
                }
               
            }
        }

        
    }
}

