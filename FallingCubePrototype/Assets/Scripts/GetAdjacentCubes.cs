using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("This behavior has moved to CubeManager. Safe to delete this until all referernces are remoedd from gameobjects or sces.")]
public class GetAdjacentCubes : MonoBehaviour
{
    private GameObject parent;
    public bool canDetect;

    private CubeBehavior cubeBehavior;
    private List<GameObject> tmpTargets;
    public GameObject tmp;

    
}
