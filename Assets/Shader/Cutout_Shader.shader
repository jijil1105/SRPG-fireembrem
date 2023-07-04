Shader "Custom/Cutout_Shader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Opaque" = "Geometry-1" }
        
        Pass{
            Zwrite On
            ColorMask 0
        }
    }
    FallBack "Diffuse"
}
