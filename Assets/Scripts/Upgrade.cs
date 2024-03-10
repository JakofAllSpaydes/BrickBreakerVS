using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Upgrade
{
    public string name;
    public string description;
    public string rarity;
    public Sprite icon;
    public EffectType effectType;
    public float additiveValue;
    public float multiplierValue;

    public enum EffectType
    {
        Speed,
        Size,
        BounceAngle,
        ExtraBall, // Requires special handling
        // Add more effect types here as needed
    }
}