using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LinHowe.WaveEquation
{
    /// <summary>
    /// 波动函数
    /// </summary>
    public static class WaveFunctions
    {
        /// <summary>
        /// 简单的平面波
        /// </summary>
        /// <param name="A">幅度</param>
        /// <param name="L">波长</param>
        /// <param name="S">速度</param>
        /// <returns></returns>
        public static Func<float, float, float> SimplePlaneWave(float A,float L,float S)
        {
            //相位速度
            float InitialPhase = 0;

            //频率
            float w = 2 / L;

            //波数
            float k = 2 * Mathf.PI / L;

            return (x,t)=> A * Mathf.Cos(k * x - w * t + InitialPhase);
        }
        
    }
}
