using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Upgrade
{
    public string name;
    public string description;
    public Rarity rarity; 
    public Sprite icon;
    public List<Effect> effects; // List of effects for this upgrade

    [System.Serializable]
    public class Effect
    {
        public EffectType effectType;
        public float genericValue;
        public float additiveValue;
        public float multiplierValue;
    }

    public enum EffectType
    {
        Speed, // Move speed
        Size, // Size
        Pierce, // Penetrate blocks
        Split, // Duplicate ball
        Boost, // Temp speed boost on collision
        Charge, // constant speed increase, reset on collision
        BounceAngle, // Preferred bounce angle on collision
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}