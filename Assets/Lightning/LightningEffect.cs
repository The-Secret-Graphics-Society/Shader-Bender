using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightningEffect", menuName = "ScriptableObjects/LightningEffectVariables", order = 1)]
public class LightningEffect : ScriptableObject
{
    // The colours at the start and end of a lightning bolt
    public Color _sourceColor = Color.red;
    public Color _targetColor = Color.blue;

    // Determines how far the first bezier control point is from the source
    public float _bezierCurveDistance = 1f;
    
    // The distance from curve that lighting vertices will be offset
    public float _lightningJitterOffset = 1f;

    public float _animationTime = 0.2f;
    public bool animating = true;
}
