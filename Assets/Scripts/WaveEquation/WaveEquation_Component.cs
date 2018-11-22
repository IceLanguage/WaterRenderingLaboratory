
using UnityEngine;

namespace LinHowe
{
    /// <summary>
    /// 波动方程模拟组件
    /// </summary>
    class WaveEquation_Component : IWaveComponent
    {
        private Vector4 waveParams; //波形参数
        private RenderTexture PreTexture;//上一刻的渲染纹理

        public WaveEquation_Component(WaterRender.WaterSurface water) : base(water)
        {

        }

        public override bool InitAndCheckWaveParams(float speed, float viscosity, float d)
        {
            if (speed <= 0)
            {
                Debug.LogError("波速不允许小于等于0！");
                return false;
            }
            if (viscosity <= 0)
            {
                Debug.LogError("粘度系数不允许小于等于0！");
                return false;
            }
            float maxvelocity = d / (2 * Time.fixedDeltaTime) * Mathf.Sqrt(viscosity * Time.fixedDeltaTime + 2);
            float velocity = maxvelocity * speed;
            float viscositySq = viscosity * viscosity;
            float velocitySq = velocity * velocity;
            float deltaSizeSq = d * d;
            float dt = Mathf.Sqrt(viscositySq + 32 * velocitySq / (deltaSizeSq));
            float dtden = 8 * velocitySq / (deltaSizeSq);
            float maxT = (viscosity + dt) / dtden;
            float maxT2 = (viscosity - dt) / dtden;
            if (maxT2 > 0 && maxT2 < maxT)
                maxT = maxT2;
            if (maxT < Time.fixedDeltaTime)
            {
                Debug.LogError("粘度系数不符合要求");
                return false;
            }

            float fac = velocitySq * Time.fixedDeltaTime * Time.fixedDeltaTime / deltaSizeSq;
            float i = viscosity * Time.fixedDeltaTime - 2;
            float j = viscosity * Time.fixedDeltaTime + 2;

            float k1 = (4 - 8 * fac) / (j);
            float k2 = i / j;
            float k3 = 2 * fac / j;

            waveParams = new Vector4(k1, k2, k3, d);

            return true;
        }

        public override void InitWaterCamera(int texSize, Material waveEquationMat)
        {
            PreTexture = RenderTexture.GetTemporary(texSize, texSize, 16);
            PreTexture.name = "[PreTex]";
            RenderTexture.active = PreTexture;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            SetWaveParams(waveEquationMat);
        }

        public override void OnDestroy()
        {
            if (PreTexture)
                RenderTexture.ReleaseTemporary(PreTexture);
        }

        public override void OnRenderImage(RenderTexture src, RenderTexture dst, Material waveEquationMat)
        {
            //传入前一次的高度渲染结果，以在shader中根据二维波方程计算当前高度
            waveEquationMat.SetTexture("_PreTex", PreTexture);

            Graphics.Blit(src, dst, waveEquationMat);

            Graphics.Blit(src, PreTexture);

        }

        protected void SetWaveParams(Material waveEquationMat)
        {
            waveEquationMat.SetVector("_WaveParams", waveParams);
        }


    }
}
