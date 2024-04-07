using UnityEngine;

[CreateAssetMenu (fileName = "New Spell", menuName = "Spells")]
public class SpellSO : ScriptableObject
{
    [Header("Information")]
    public string spellName;
    [TextArea(3, 3)] public string spellDescription;
    [TextArea(3, 3)] public string inBattleDescription;
    [TextArea(3, 3)] public string excuseForInaction;
    public int spellIndex;
    public SpellType spellType;
    public int cost;

    [Header("Effect")]
    public bool hasSpecialEffect;
    public bool hasEffectAndDamages;
    public bool doTargetEveryone;
    public bool helpingSpell;
    public bool selfSpell;
    public bool onlyWorkOnDefeated;
    [Range(1, 100)] public int successRate;

    [Header("Magic")]
    public int strenght;
    public ElementType elementType;
    
    [Header("Aptitude")]
    public int factor;
    public int staticDamages;
}

public enum SpellType
{
    Spell, Aptitude
}

public enum ElementType
{
    NA, Feu, Eau, Vent, Plante, Terre, Divin, Obscur
}

public enum Statut
{
    None, // Aucun effet
    Brûlé, // Subit 1/12 des dégâts max par tour
    Empoisonné, // Subit 1/16 des dégâts max par tour
    EmpMagique, // Comme poison mais pour MP
    Endormi, // Ne peut pas agir. Se réveille à chaque coup physique reçu ou au bout d'un certain nbr de tours
    Silence, // Ne peut pas lancer de sort.
    Paralysé, //Ne peut pas agir pendant un certain nbr de tours
}
