//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

#include "Includes/SimpleNoise.hlsl"

void WindDisplacement_float(float3 _WorldPos, float _Frequency, float _WindForce, float _Ratio, float _OriginY, float _DistanceMultiplier, float _Direction, float _OriginDirection, float _TimeScale, float2 _UV, bool _CanSway, out float3 Out)
{
    float3 worldPos = _WorldPos;

    if (_CanSway)
    {
        bool isUpDir = (_OriginDirection > 0.5);
        bool condition = isUpDir ? (worldPos.y < _OriginY) : (worldPos.y > _OriginY);
    
        if (condition)
        {
            float newFreq = _Frequency / 2 + SimpleNoise(float2(_Time.y,0), 1) / 2;
            float cosDir = cos(_Direction);
            float sinDir = sin(_Direction);
            float timeFactor = _TimeScale * _Time.y * newFreq;
            
            float distanceFromOrigin = worldPos.y - _OriginY;
            float sinAngle = sin(timeFactor + worldPos.y * PI);
    
            float directionalStaticX = cosDir * _WindForce;
            float directionalStaticZ = sinDir * _WindForce;
    
            float intensityX = cosDir * _WindForce * _Ratio * sinAngle;
            float intensityZ = sinDir * _WindForce * _Ratio * sinAngle;
    
            float displacementX = _DistanceMultiplier * pow(distanceFromOrigin, 2) * (directionalStaticX + intensityX);
            float displacementZ = _DistanceMultiplier * pow(distanceFromOrigin, 2) * (directionalStaticZ + intensityZ);
            worldPos.x += displacementX;
            worldPos.z += displacementZ;
        }
    }

    Out = worldPos;
}

#endif //MYHLSLINCLUDE_INCLUDED
