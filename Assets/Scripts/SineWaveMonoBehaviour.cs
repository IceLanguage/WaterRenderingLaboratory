using System;
using System.Collections.Generic;
using UnityEngine;
namespace LinHowe
{
    [Serializable]
    public struct SineWave
    {
        public float L, A, S;//波长，波幅,速度
        public Vector2 pos;//波源
        public Vector2 D;//方向
    }
    /// <summary>
    /// 用以显示波形参数
    /// </summary>
    public class SineWaveMonoBehaviour : MonoBehaviour
    {
        [SerializeField]
        public List<SineWave> waves = new List<SineWave>();
        private WaterRender.WaterSurface water;
        private void Awake()
        {
            water = GetComponent<WaterRender.WaterSurface>();
            if (water == null)
                Debug.LogError("Need WaterSurface Component");
        }

        /// <summary>
        /// 需要将SineWave.pos 转换成uv 
        /// </summary>
        public void CaluateUV()
        {
            Vector2 WaterMinBoundary = new Vector2(-water.width / 2, -water.length / 2);
            for(int i = 0;i <waves.Count;++i)
            {
                SineWave wave = waves[i];
                wave.pos = new Vector2
                (
                    (waves[i].pos.x - WaterMinBoundary.x) / water.xcellsize * water.uvxcellsize,
                    (waves[i].pos.y - WaterMinBoundary.y) / water.ycellsize * water.uvycellsize
                );
                wave.pos.x = Mathf.Clamp(wave.pos.x, 0, 1);
                wave.pos.y = Mathf.Clamp(wave.pos.y, 0, 1);
                waves[i] = wave;
            }
        }
        private void OnDrawGizmos()
        {
            foreach(SineWave sinewave in waves)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3
                        (
                            sinewave.pos.x,
                            transform.position.y,
                            sinewave.pos.y
                         );
                Gizmos.DrawSphere(center, 0.05f);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(
                    new Ray
                    (
                        center,
                        new Vector3
                        (
                            sinewave.D.x,
                            transform.position.y,
                            sinewave.D.y
                        )
                    ));
            }
        }
    }
}