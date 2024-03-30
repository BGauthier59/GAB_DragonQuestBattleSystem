using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatDisplayManager : MonoBehaviour
{
    public static StatDisplayManager instance;
    
    public TextMeshProUGUI[] names;
    public TextMeshProUGUI[] hps;
    public TextMeshProUGUI[] mps;
    public TextMeshProUGUI[] lvs;

    public TextMeshProUGUI gold;
    public TextMeshProUGUI xp;
    
    public int goldTotal;
    public int xpTotal;

    public GameObject infoZone;
    public TextMeshProUGUI infoName;
    public TextMeshProUGUI infoHp;
    public TextMeshProUGUI infoMp;
    public TextMeshProUGUI infoAtk;
    public TextMeshProUGUI infoDef;
    public TextMeshProUGUI infoAgi;
    public TextMeshProUGUI infoMana;
    public TextMeshProUGUI infoSpells;
    public Image infoWeapon;
    public Image infoArmor;
    public Image infoShield;
    public Image infoHelmet;

    private void Awake()
    {
        if (instance is { })
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;
    }

    public void DisplayStat(EntityManager hero)
    {
        if (hero.entityType == EntityType.Ally)
        {
            names[hero.entityIndex].text = hero.entityName;
            hps[hero.entityIndex].text = $"{hero.entityHp} / {hero.entityHpMax}";
            mps[hero.entityIndex].text = $"{hero.entityMp} / {hero.entityMpMax}";
            lvs[hero.entityIndex].text = hero.entitytLv.ToString();

            if (hero.isDefeated)
            {
                names[hero.entityIndex].color = Color.red;
            }
            else
            {
                names[hero.entityIndex].color = Color.white;
            }
        }
    }

    public void DisplayGoldXp()
    {
        gold.text = goldTotal.ToString();
        xp.text = xpTotal.ToString();
    }
    
    public void ShowingInfos(bool active)
    {
        infoZone.SetActive(active);
        if (BattleManager.instance.entityActing != null &&
            BattleManager.instance.entityActing.entityType == EntityType.Ally)
        {
            EntityManager entity = BattleManager.instance.entityActing;

            infoName.text = entity.entityName;
            infoHp.text = $"HP : {entity.entityHp} / {entity.entityHpMax}";
            infoMp.text = $"MP : {entity.entityMp} / {entity.entityMpMax}";
            infoAtk.text = $"ATK : {entity.entityAtk}";
            infoDef.text = $"DEF : {entity.entityDef}";
            infoAgi.text = $"AGI : {entity.entityAgi}";
            infoMana.text = $"MANA : {entity.entityMana}";

            infoSpells.text = "SPELLS : ";
            foreach (SpellSO spell in entity.entitySpells)
            {
                infoSpells.text += $"{spell.spellName} ({spell.cost} MP) | ";
            }

            if(entity.entityWeapon != null) infoWeapon.sprite = entity.entityWeapon.weaponSprite;
            if(entity.entityArmor != null) infoArmor.sprite = entity.entityArmor.equipmentSprite;
            if(entity.entityShield != null) infoShield.sprite = entity.entityShield.equipmentSprite;
            if(entity.entityHelmet != null) infoHelmet.sprite = entity.entityHelmet.equipmentSprite;

        }
    }
}
