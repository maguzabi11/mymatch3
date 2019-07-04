using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{

[CreateAssetMenu]
public class MatchPatternObject : ScriptableObject
{
    [SerializeField]
    Point2D[] points;

}


}