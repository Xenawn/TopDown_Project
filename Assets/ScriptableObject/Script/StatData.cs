using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Health,
    Speed,
    ProjectileCount // 미사일 증가수
}
[CreateAssetMenu(fileName = "New StatData", menuName = "Stats/Chracter Stats")]
public class StatData : ScriptableObject
{
    public string characterName;
    public List<StatEntry> stats;
}

[System.Serializable]
public class StatEntry
{
    public StatType statType; // 어떤 스탯인지
    public float baseValue;
}

