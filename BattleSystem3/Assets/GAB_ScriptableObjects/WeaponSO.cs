using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "New Weapon", menuName = "Weapons")]
public class WeaponSO : ScriptableObject
{
    public string weaponName;

    public Sprite weaponSprite;
    
    public WeaponType weaponType;

    public int hp;
    public int mp;
    public int atk;
    public int def;
    public int agi;
    public float mana;
    public int dodge;
    public int critical;

}

public enum WeaponType
{
    Sword, Spear, Staff, Boomerang, Axe, Bow
}
