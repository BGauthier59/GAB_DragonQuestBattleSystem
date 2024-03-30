using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu (fileName = "New Equipment", menuName = "Equipment")]
public class EquipmentSO : ItemSO
{
    public string equipmentName;

    public Sprite equipmentSprite;
    
    public EquipmentType equipmentType;

    public int hp;
    public int mp;
    public int atk;
    public int def;
    public int agi;
    public float mana;
    public int dodge;
    public int critical;

}

public enum EquipmentType
{
    Armor, Shield, Helmet, Accessory
}