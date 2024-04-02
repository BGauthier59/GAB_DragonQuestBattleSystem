using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityManager : MonoBehaviour
{
    #region Variables
    
    public EntitySO entitySO;

    public int entityIndex;
    public EntityType entityType;
    public string entityPronoun;
    public EntityStrategy entityStrategy;
    
    public string entityName;
    public int entityHpMax;
    public int entityHp;

    public int entityMpMax;
    public int entityMp;

    public int entityAtkInit;
    public int entityAtk;

    public int entityDefInit;
    public int entityDef;
    public bool isDefending;

    public int entityAgiInit;
    public int entityAgi;

    public int entitytLv;

    public float entityManaInit;
    public float entityMana;

    public int entityCriticalInit;
    public int entityCritical;

    public int entityDodgeInit;
    public int entityDodge;
    public int entityResilienceToMPTheft;

    public List<SpellSO> entitySpells;

    public Statut entityStatut;

    public List<bool> hasEffect = new List<bool>();
    public List<int> effectList = new List<int>();
    public List<int> statList = new List<int>();
    public List<float> boostList = new List<float>();

    // Heros

    public WeaponSO entityWeapon;

    public EquipmentSO entityHelmet;
    public EquipmentSO entityShield;
    public EquipmentSO entityArmor;

    // Monsters
    public RectTransform rectTransform;
    public Vector3 localPos;
    public Animation entityAnim;
    public Image entityImage;
    public Button entityButton;
    public TextMeshProUGUI entityNameDisplay;
    public bool isDodgingAnim;

    public bool isDefeated;

    #endregion
    
    public void LinkingStat()
    {
        entityName = entitySO.entityName;
        name = entityName;

        entityType = entitySO.type;

        entityPronoun = entitySO.pronoun;

        entityHpMax = entitySO.hp;
        entityHp = entityHpMax;
        
        entityMpMax = entitySO.mp;
        entityMp = entityMpMax;
        
        entityAtkInit = entitySO.atk;
        entityAtk = entityAtkInit;
        
        entityDefInit = entitySO.defense;
        entityDef = entityDefInit;
        
        entityAgiInit = entitySO.agility;
        entityAgi = entityAgiInit;
        
        entityManaInit = entitySO.mana;
        entityMana = entityManaInit;

        entityCriticalInit = entitySO.criticalHit;
        entityCritical = entityCriticalInit;

        entityDodgeInit = entitySO.dodge;
        entityDodge = entityDodgeInit;

        entityResilienceToMPTheft = entitySO.entityResilienceToMPTheft;

        entitytLv = entitySO.level;
        
        entitySpells = entitySO.spells;
        
        entityImage = GetComponent<Image>();
        entityImage.sprite = FightManager.instance.emptySprite;
        entityButton = GetComponent<Button>();

        entityStrategy = entitySO.strategy;
    }

    public void Heal()
    {
        entityHp = entityHpMax;
        entityMp = entityMpMax;
        entityAtk = entityAtkInit;
        entityDef = entityDefInit;
        entityAgi = entityAgiInit;
        entityMana = entityManaInit;
        entityCritical = entityCriticalInit;
        entityDodge = entityDodgeInit;
    }
    
    public void LinkingEquipments()
    {
        if (entitySO.weapon != null)
        {
            entityWeapon = entitySO.weapon;
            
            entityHpMax += entitySO.weapon.hp;
            entityHp = entityHpMax;
        
            entityMpMax += entitySO.weapon.mp;
            entityMp = entityMpMax;
        
            entityAtkInit += entitySO.weapon.atk;
            entityAtk = entityAtkInit;
        
            entityDefInit += entitySO.weapon.def;
            entityDef = entityDefInit;
        
            entityAgiInit += entitySO.weapon.agi;
            entityAgi = entityAgiInit;
        
            entityManaInit += entitySO.weapon.mana;
            entityMana = entityManaInit;

            entityCriticalInit += entitySO.weapon.critical;
            entityCritical = entityCriticalInit;

            entityDodgeInit += entitySO.weapon.dodge;
            entityDodge = entityDodgeInit;
        }

        if (entitySO.armor != null)
        {
            entityArmor = entitySO.armor;
            
            entityHpMax += entitySO.armor.hp;
            entityHp = entityHpMax;
        
            entityMpMax += entitySO.armor.mp;
            entityMp = entityMpMax;
        
            entityAtkInit += entitySO.armor.atk;
            entityAtk = entityAtkInit;
        
            entityDefInit += entitySO.armor.def;
            entityDef = entityDefInit;
        
            entityAgiInit += entitySO.armor.agi;
            entityAgi = entityAgiInit;
        
            entityManaInit += entitySO.armor.mana;
            entityMana = entityManaInit;

            entityCriticalInit += entitySO.armor.critical;
            entityCritical = entityCriticalInit;

            entityDodgeInit += entitySO.armor.dodge;
            entityDodge = entityDodgeInit;
        }

        if (entitySO.shield != null)
        {
            entityShield = entitySO.shield;
            
            entityHpMax += entitySO.shield.hp;
            entityHp = entityHpMax;
        
            entityMpMax += entitySO.shield.mp;
            entityMp = entityMpMax;
        
            entityAtkInit += entitySO.shield.atk;
            entityAtk = entityAtkInit;
        
            entityDefInit += entitySO.shield.def;
            entityDef = entityDefInit;
        
            entityAgiInit += entitySO.shield.agi;
            entityAgi = entityAgiInit;
        
            entityManaInit += entitySO.shield.mana;
            entityMana = entityManaInit;

            entityCriticalInit += entitySO.shield.critical;
            entityCritical = entityCriticalInit;

            entityDodgeInit += entitySO.shield.dodge;
            entityDodge = entityDodgeInit;
        }

        if (entitySO.helmet != null)
        {
            entityHelmet = entitySO.helmet;
            
            entityHpMax += entitySO.helmet.hp;
            entityHp = entityHpMax;
        
            entityMpMax += entitySO.helmet.mp;
            entityMp = entityMpMax;
        
            entityAtkInit += entitySO.helmet.atk;
            entityAtk = entityAtkInit;
        
            entityDefInit += entitySO.helmet.def;
            entityDef = entityDefInit;
        
            entityAgiInit += entitySO.helmet.agi;
            entityAgi = entityAgiInit;
        
            entityManaInit += entitySO.helmet.mana;
            entityMana = entityManaInit;

            entityCriticalInit += entitySO.helmet.critical;
            entityCritical = entityCriticalInit;

            entityDodgeInit += entitySO.helmet.dodge;
            entityDodge = entityDodgeInit;
        }

    }

    public void ResetStat()
    {
        Heal();
    }

    public void StatAfterBattle()
    {
        entityAtk = entityAtkInit;
        
        entityDef = entityDefInit;
        
        entityAgi = entityAgiInit;
        
        entityMana = entityManaInit;

        entityCritical = entityCriticalInit;

        entityDodge = entityDodgeInit;

        isDefending = false;

        entityStatut = Statut.None;
    }
    
    #region Monsters

    public void LinkingImage()
    {
        entityImage.sprite = entitySO.sprite;
        rectTransform.position = rectTransform.position + new Vector3(0, entitySO.spawnPosY, 0);
        entityImage.SetNativeSize();
        entityNameDisplay.text = entityName;
        entityAnim = GetComponent<Animation>();
        IsMonsterSelectable(false);
    }

    public void IsMonsterSelectable(bool interactable)
    {
        entityButton.interactable = interactable;
        if (interactable)
        {
            entityNameDisplay.gameObject.SetActive(true);
        }
        else
        {
            entityNameDisplay.gameObject.SetActive(false);
        }
    }

    public void OnMonsterSelected()
    {
        for (int i = 0; i < SpawningManager.instance.monstersInBattle.Count; i++)
        {
            EntityManager entityManager = SpawningManager.instance.monstersInBattle[i].GetComponent<EntityManager>();
            if (!entityManager.isDefeated)
            {
                entityManager.IsMonsterSelectable(false);
            }
        }
        BattleManager.instance.entityTargeted = this;
        BattleManager.instance.hasMonsterBeenTargeted = true;
    }

    private void Start()
    {
        if (entityType == EntityType.Monster)
        {
            localPos = rectTransform.localPosition;
        }
    }

    public void LateUpdate()
    {
        if (isDodgingAnim)
        {
            rectTransform.localPosition += localPos;
        }
    }

    public void SetPos()
    {
        Debug.Log(rectTransform.localPosition);
        Debug.Log(localPos);
        rectTransform.localPosition = localPos;
    }

    #endregion

    #region Heroes

    public void LinkingToDisplay()
    {
        StatDisplayManager.instance.DisplayStat(this);
    }
    
    public void OnNewLevel(int hp, int mp, int atk, int def, int agi, float mana)
    {
        entitytLv++;
        
        entityHpMax += hp;
        entityHp = entityHpMax;

        entityMpMax += mp;
        entityMp = entityMpMax;
        
        entityAtkInit += atk;
        entityAtk = entityAtkInit;
        
        entityDefInit += def;
        entityDef = entityDefInit;
        
        entityAgiInit += agi;
        entityAgi = entityAgiInit;

        entityManaInit += mana;
        entityMana = entityManaInit;
    }

    public void SetSpells()
    {
        entitySpells.Clear();

        for (int i = 0; i < entitytLv; i++)
        {
            foreach (LearnSpell learning in entitySO.learnSpells)
            {
                if (i == learning.learningLevel)
                {
                    entitySpells.Add(learning.learningSpell);
                }
            }
        }
        
        Debug.Log("Set spells for "+ entityName);
    }

    #endregion

    public void StatutEffect()
    {

    }
}
