Shader "Custom/Mask" {

        SubShader{
          Tags {
            "Queue" = "Transparent+100"
            "RenderType" = "Transparent"
          }

          Pass {
            Name "Mask"
            Cull Off
            ZTest[_ZTest]
            ZWrite Off
            ColorMask 0

            Stencil {
              Ref 1
              Pass Replace
            }
          }
    }
}