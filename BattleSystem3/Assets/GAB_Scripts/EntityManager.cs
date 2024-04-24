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
    public ElementType elementType;

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

    public int entityActionPerTurn;
    public int entityChanceToUseSpell;

    public int entityDodgeInit;
    public int entityDodge;
    public int entityResilienceToMPTheft;
    public int entityResilienceToPoison;
    public int entityResilienceToBurn;
    public int entityResilienceToSilence;
    public int entityResilienceToSleep;
    public int entityResilienceToStatDecrease;

    [Range(-2, 2)] public int atkStatIndex = 0;
    [Range(-2, 2)] public int defStatIndex = 0;
    [Range(-2, 2)] public int manaStatIndex = 0;

    public List<SpellSO> entitySpells;

    public Statut entityStatut;
    public int turnsBeforeRecovering;
    public int turnsBeforeResetDef;
    public int turnsBeforeResetAtk;
    public int turnsBeforeResetMana;
    public int turnsBeforeResetReflexion;
    public bool isBlocked;
    public bool isReflected;

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
    public int gold;
    public int xp;

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

        entityActionPerTurn = entitySO.actionPerTurn;
        entityChanceToUseSpell = entitySO.entityChanceToUseSpell;

        entityResilienceToMPTheft = entitySO.entityResilienceToMPTheft;
        entityResilienceToPoison = entitySO.entityResilienceToPoison;
        entityResilienceToBurn = entitySO.entityResilienceToBurn;
        entityResilienceToSilence = entitySO.entityResilienceToSilence;
        entityResilienceToSleep = entitySO.entityResilienceToSleep;
        entityResilienceToStatDecrease = entitySO.entityResilienceToStatDecrease;

        entitytLv = entitySO.level;

        entitySpells = entitySO.spells;

        entityImage = GetComponent<Image>();
        entityImage.sprite = FightManager.instance.emptySprite;
        entityButton = GetComponent<Button>();

        entityStrategy = entitySO.strategy;

        elementType = entitySO.elementType;

        xp = entitySO.xp;
        gold = entitySO.gold;

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
        isDefeated = false;
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

        atkStatIndex = 0;

        defStatIndex = 0;

        manaStatIndex = 0;

        turnsBeforeRecovering = 0;

        isBlocked = false;

        isReflected = false;

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

        Debug.Log("Set spells for " + entityName);
    }

    #endregion

    public void SetNewDef(int defStatIndex)
    {
        switch (defStatIndex)
        {
            case (-2):

                float entityDefMin = entityDefInit * 0.5f;
                int realEntityDefMin = (int)entityDefMin;
                entityDef = realEntityDefMin;

                break;

            case (-1):

                float entityDefLower = entityDefInit * 0.75f;
                int realEntityDefLower = (int)entityDefLower;
                entityDef = realEntityDefLower;
                break;

            case (0):

                entityDef = entityDefInit;

                break;

            case (1):

                float entityDefUpper = entityDefInit * 1.5f;
                int realEntityDefUpper = (int)entityDefUpper;
                entityDef = realEntityDefUpper;
                break;

            case (2):

                float entityDefMax = entityDefInit * 2;
                entityDef = (int)entityDefMax;
                break;
        }
    }

    public void SetNewAtk(int atkStatIndex)
    {
        switch (atkStatIndex)
        {
            case (-2):

                float entityAtkMin = entityAtkInit * 0.5f;
                int realEntityAtkMin = (int)entityAtkMin;
                entityAtk = realEntityAtkMin;

                break;

            case (-1):

                float entityAtkLower = entityAtkInit * 0.75f;
                int realEntityAtkLower = (int)entityAtkLower;
                entityAtk = realEntityAtkLower;
                break;

            case (0):

                entityAtk = entityAtkInit;

                break;

            case (1):

                float entityAtkUpper = entityAtkInit * 1.5f;
                int realEntityAtkUpper = (int)entityAtkUpper;
                entityAtk = realEntityAtkUpper;
                break;

            case (2):

                float entityAtkMax = entityAtkInit * 2;
                entityAtk = (int)entityAtkMax;
                break;
        }
    }

    public void SetNewMana(int manaStatIndex)
    {
        switch (manaStatIndex)
        {
            case (-2):

                float entityManaMin = entityManaInit * 0.75f;
                entityMana = entityManaMin;

                break;

            case (-1):

                float entityManaLower = entityManaInit * 0.9f;
                entityMana = entityManaLower;
                break;

            case (0):

                entityMana = entityManaInit;

                break;

            case (1):

                float entityManaUpper = entityManaInit * 1.25f;
                entityMana = entityManaUpper;
                break;

            case (2):

                float entityManaMax = entityManaInit * 1.5f;
                entityMana = entityManaMax;
                break;
        }
    }
}
