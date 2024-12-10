float3 TriplanarNormalMapping(float3 worldPos, float3 worldNormal, float tiling, float offset)
{
    // Calculate blend weights based on the normal's absolute components
    float3 blend = abs(worldNormal);
    blend = (blend - 0.2) * 7.0; // Sharpness adjustment
    blend = max(blend, 0.00001);
    blend /= (blend.x + blend.y + blend.z);

    // Triplanar uvs
    float2 uvX = worldPos.zy + float2(0, offset); // x facing plane
    float2 uvY = worldPos.xz; // y facing plane
    float2 uvZ = worldPos.xy + float2(0, offset); // z facing plane

    // Tangent space normal maps
    half3 tnormalX = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvX * tiling));
    half3 tnormalY = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvY * tiling));
    half3 tnormalZ = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uvZ * tiling));

    // Swizzle world normals into tangent space and apply Whiteout blend
    tnormalX = half3(
        tnormalX.xy + worldNormal.zy,
        abs(tnormalX.z) * worldNormal.x
        );
    tnormalY = half3(
        tnormalY.xy + worldNormal.xz,
        abs(tnormalY.z) * worldNormal.y
        );
    tnormalZ = half3(
        tnormalZ.xy + worldNormal.xy,
        abs(tnormalZ.z) * worldNormal.z
        );
    
    // Swizzle tangent normals to match world orientation and triblend
    return normalize(
        tnormalX.zyx * blend.x +
        tnormalY.xzy * blend.y +
        tnormalZ.xyz * blend.z
    );
}