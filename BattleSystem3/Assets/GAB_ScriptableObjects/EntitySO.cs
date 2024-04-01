using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "New Monster", menuName = "Monsters")]
public class EntitySO : ScriptableObject
{
    [Header("Informations")]
    public string entityName;
    public Sprite sprite;
    public float spawnPosY;
    public EntityType type;

    [Header("Statistics")]
    public int hp;
    public int mp;
    public int atk;
    public int defense;
    public int agility;
    public int level;
    public float mana;
    [Range (0, 100)]
    public int criticalHit;
    [Range (0, 100)]
    public int dodge;

    [Header("Abilities")]
    public List<SpellSO> spells;
    
    [Header("Equipments")]
    public WeaponSO weapon;
    public EquipmentSO helmet;
    public EquipmentSO shield;
    public EquipmentSO armor;

    [Header("Level up")]
    public int[] hpLevelUp = new int[2];
    public int[] mpLevelUp = new int[2];
    public int[] atkLevelUp = new int[2];
    public int[] defLevelUp = new int[2];
    public int[] agiLevelUp = new int[2];
    public float[] manaLevelUp = new float[2];
    public LearnSpell[] learnSpells;
    
    [Header("Monsters Only")]
    public EntityStrategy strategy;
    public ElementType elementType;
    public ItemSO item;
    public int probability;
    public int gold;
    public int xp;
}

public enum EntityType
{
    Ally, Monster
}

public enum EntityStrategy // Les catégories d'ennemis pour les IA
{
    NA,
    Attaquant, // Va attaquer régulièrement
    Mage, // Va utiliser ses aptitudes régulièrement
    Soigneur // Va soigner dès que nécessaire
}
