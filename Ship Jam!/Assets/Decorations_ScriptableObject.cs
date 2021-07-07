using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DecorationPool
{
    public List<GameObject> decorations = new List<GameObject>();
    public Vector3 rotationAxis = Vector3.up;
    public Vector3 maxRotationAngle = Vector3.one * 360f;
    [Range(0f, 1f)]
    public float placementChance = 1f;
    public float maxPlacements = 1f;
    public LandType landTypePlacement = LandType.None;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Decorations", order = 1)]
public class Decorations_ScriptableObject : ScriptableObject
{
    public List<DecorationPool> decorationPools = new List<DecorationPool>();
}
