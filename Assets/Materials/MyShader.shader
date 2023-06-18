// シェーダーの情報
Shader "Unlit/MyShader"
{
    // Unity上でやり取りをするプロパティ情報
    // マテリアルのInspectorウィンドウ上に表示され、スクリプト上からも設定できる
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1) // Color プロパティー (デフォルトは白)   a____
    }
    // サブシェーダー
    // シェーダーの主な処理はこの中に記述する
    // サブシェーダーは複数書くことも可能が、基本は一つ
    SubShader
    {
        // パス
        // 1つのオブジェクトの1度の描画で行う処理をここに書く
        // これも基本一つだが、複雑な描画をするときは複数書くことも可能
        Pass
        {
            CGPROGRAM   // プログラムを書き始めるという宣言
            
            // 関数宣言
            #pragma vertex vert    // "vert" 関数を頂点シェーダー使用する宣言
            #pragma fragment frag  // "frag" 関数をフラグメントシェーダーと使用する宣言
            #include "UnityCG.cginc"
            
            // 変数宣言
            fixed4 _Color; // マテリアルからのカラー   a____
 
            // 頂点シェーダー
            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }
 
            // フラグメントシェーダー
            fixed4 frag () : SV_Target
            {
                return _Color;  // a____
            }
 
            ENDCG   // プログラムを書き終わるという宣言
        }
    }
}