using UnityEngine;

[CreateAssetMenu (fileName = "New Zone", menuName = "Zones")]
public class BattleAreaSO : ScriptableObject
{
    [Header("Monsters")]
    public EntitySO[] monstersInZoneDay;
    public EntitySO[] monstersInZoneNight;
    public int[] minMaxMonstersInBattle = new int[2];
    
    [Header("Environment")]
    public Sprite fontZoneDay;
    public Sprite fontZoneNight;
    
    [Header("Audio")]
    public AudioClip musicInZone;
    
    [Header("Data")]
    public CombatZone combatZone;
}