
float gaussianBlur(float2 texelSize, float2 uv, sampler2D _SampledTexture)
{
    // Gaussian blur weights
    //  1/16    2/16    1/16
    //  2/16    4/16    2/16
    //  1/16    2/16    1/16
    float w11 = 1.0/16.0; float w12 = 2.0/16.0; float w13 = 1.0/16.0;
    float w21 = 2.0/16.0; float w22 = 4.0/16.0; float w23 = 2.0/16.0;
    float w31 = 1.0/16.0; float w32 = 2.0/16.0; float w33 = 1.0/16.0;

    // Offsets for the neighbors
    float2 uvLeft   = uv - float2(texelSize.x, 0);
    float2 uvRight  = uv + float2(texelSize.x, 0);
    float2 uvUp     = uv - float2(0, texelSize.y);
    float2 uvDown   = uv + float2(0, texelSize.y);
    float2 uvUpLeft = uv + float2(-texelSize.x, -texelSize.y);
    float2 uvUpRight= uv + float2(texelSize.x, -texelSize.y);
    float2 uvDownLeft = uv + float2(-texelSize.x, texelSize.y);
    float2 uvDownRight= uv + float2(texelSize.x, texelSize.y);

    // Sample neighbors
    float c  = tex2D(_SampledTexture, uv).r;
    float ul = tex2D(_SampledTexture, uvUpLeft).r;
    float u  = tex2D(_SampledTexture, uvUp).r;
    float ur = tex2D(_SampledTexture, uvUpRight).r;
    float l  = tex2D(_SampledTexture, uvLeft).r;
    float r  = tex2D(_SampledTexture, uvRight).r;
    float dl = tex2D(_SampledTexture, uvDownLeft).r;
    float d  = tex2D(_SampledTexture, uvDown).r;
    float dr = tex2D(_SampledTexture, uvDownRight).r;

    // Compute Gaussian blur
    float blurredValue =
        ul * w11 + u  * w12 + ur * w13 +
        l  * w21 + c  * w22 + r  * w23 +
        dl * w31 + d  * w32 + dr * w33;
    
    return blurredValue;
}