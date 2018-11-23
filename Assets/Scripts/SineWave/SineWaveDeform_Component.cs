using System.Collections.Generic;
using UnityEngine;

namespace LinHowe
{
    /// <summary>
    /// 正弦函数变形模拟水面
    /// </summary>
    class SineWaveDeform_Component : SineWave_Component
    {

        public SineWaveDeform_Component(WaterRender.WaterSurface water) : base(water)
        {

        }
        public override void InitWaterCamera(int texSize, Material waveEquationMat)
        {
            waveEquationMat.SetFloat("k", 4.4f);
        }
    }
}
