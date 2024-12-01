using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightningEffect", menuName = "ScriptableObjects/LightningEffectVariables", order = 1)]
public class LightningEffect : ScriptableObject
{
    // The colours at the start and end of a lightning bolt
    [SerializeField] Color _sourceColour = Color.red;
    [SerializeField] Color _targetColour = Color.blue;

    // Determines how far the first bezier control point is from the source
    [SerializeField] float _bezierCurveDistance = 1f;
    
    // The distance from curve that lighting vertices will be offset
    [SerializeField] float _lightningJitterOffset = 1f;
}
