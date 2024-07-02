using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using JetBrains.Annotations;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class EntityManager : MonoBehaviour
{
    #region Variables

    public EntitySO entitySO;
    public bool isAIally;

    public int entityIndex;
    public EntityType entityType;
    public string entityPronoun;
    public EntityStrategy entityStrategy;
    public ElementType elementType;

    public string entityName;
    public int entityHpMax;
    public int entityHp;
    public int entityHpWS;

    public int entityMpMax;
    public int entityMp;
    public int entityMpWS;

    public int entityAtkInit;
    public int entityAtk;
    public int entityAtkWS;

    public int entityDefInit;
    public int entityDef;
    public int entityDefWS;
    public bool isDefending;

    public int entityAgiInit;
    public int entityAgi;
    public int entityAgiWS;

    public int entitytLv;

    public float entityManaInit;
    public float entityMana;
    public float entityManaWS;

    public int entityCriticalInit;
    public int entityCritical;
    public int entityCriticalWS;

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

        isAIally = entitySO.isAIally;

        entityPronoun = entitySO.pronoun;

        entityWeapon = entitySO.weapon;
        entityArmor = entitySO.armor;
        entityShield = entitySO.shield;
        entityHelmet = entitySO.helmet;

        entityHpMax = entitySO.hp;
        entityHp = entityHpMax;
        entityHpWS = entityHp;

        entityMpMax = entitySO.mp;
        entityMp = entityMpMax;
        entityMpWS = entityMp;

        entityAtkInit = entitySO.atk;
        entityAtk = entityAtkInit;
        entityAtkWS = entityAtk;

        entityDefInit = entitySO.defense;
        entityDef = entityDefInit;
        entityDefWS = entityDef;

        entityAgiInit = entitySO.agility;
        entityAgi = entityAgiInit;
        entityAgiWS = entityAgi;

        entityManaInit = entitySO.mana;
        entityMana = entityManaInit;
        entityManaWS = entityMana;

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

    public void ResetStuff()
    {
        entityHpMax = entityHpWS;
        entityMpMax = entityMpWS;
        entityAtkInit = entityAtkWS;
        entityDefInit = entityDefWS;
        entityAgiInit = entityAgiWS;
        entityManaInit = entityManaWS;
    }

    public void LinkingEquipments()
    {
        if (entityWeapon != null)
        {
            entityHpMax = entityHpWS + entityWeapon.hp;

            entityMpMax = entityMpWS + entityWeapon.mp;

            entityAtkInit = entityAtkWS + entityWeapon.atk;
            entityAtk = entityAtkInit;

            entityDefInit = entityDefWS + entityWeapon.def;
            entityDef = entityDefInit;

            entityAgiInit = entityAgiWS + entityWeapon.agi;
            entityAgi = entityAgiInit;

            entityManaInit = entityManaWS + entityWeapon.mana;
            entityMana = entityManaInit;

            entityCriticalInit = entityCriticalWS + entityWeapon.critical;
            entityCritical = entityCriticalInit;
        }

        if (entityArmor != null)
        {
            entityHpMax += entityArmor.hp;

            entityMpMax += entityArmor.mp;

            entityAtkInit += entityArmor.atk;
            entityAtk = entityAtkInit;

            entityDefInit += entityArmor.def;
            entityDef = entityDefInit;

            entityAgiInit += entityArmor.agi;
            entityAgi = entityAgiInit;

            entityManaInit += entityArmor.mana;
            entityMana = entityManaInit;

            entityCriticalInit += entityArmor.critical;
            entityCritical = entityCriticalInit;
        }

        if (entityShield != null)
        {
            entityHpMax += entityShield.hp;

            entityMpMax += entityShield.mp;

            entityAtkInit += entityShield.atk;
            entityAtk = entityAtkInit;

            entityDefInit += entityShield.def;
            entityDef = entityDefInit;

            entityAgiInit += entityShield.agi;
            entityAgi = entityAgiInit;

            entityManaInit += entityShield.mana;
            entityMana = entityManaInit;

            entityCriticalInit += entityShield.critical;
            entityCritical = entityCriticalInit;
        }

        if (entityHelmet != null)
        {
            entityHpMax += entityHelmet.hp;

            entityMpMax += entityHelmet.mp;

            entityAtkInit += entityHelmet.atk;
            entityAtk = entityAtkInit;

            entityDefInit += entityHelmet.def;
            entityDef = entityDefInit;

            entityAgiInit += entityHelmet.agi;
            entityAgi = entityAgiInit;

            entityManaInit += entityHelmet.mana;
            entityMana = entityManaInit;

            entityCriticalInit += entityHelmet.critical;
            entityCritical = entityCriticalInit;
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

        entityHpWS += hp;
        entityHpMax = entityHpWS;

        entityMpWS += mp;
        entityMpMax = entityMpWS;

        entityAtkWS += atk;
        entityAtk = entityAtkWS;

        entityDefWS += def;
        entityDef = entityDefWS;

        entityAgiWS += agi;
        entityAgi = entityAgiWS;

        entityManaWS += mana;
        entityMana = entityManaWS;
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

    public void EscouadeBonus(Escouade escouade)
    {
        switch (escouade)
        {
            case (Escouade.NA): break;


            case (Escouade.La_Fratrie_Baï):

                entityHpMax += 50;
                entityHp = entityHpMax;
                entityAtk += 30;
                entityDef += 30;
                entityChanceToUseSpell += 1;

                entitySpells.Add(entitySO.bonusSpell);

                break;

            case (Escouade.Les_Bûcherons):

                entityHpMax += 30;
                entityHp = entityHpMax;
                entityMpMax += 30;
                entityMp = entityMpMax;

                break;

            case (Escouade.Les_Bois_Réincarnés):

                entityAtk += 20;
                entityDef += 20;
                entityChanceToUseSpell += 1;

                break;

            case (Escouade.Les_Ensorceleurs):

                entityMpMax += 20;
                entityMp = entityMpMax;
                entityMana += 0.2f;
                entityChanceToUseSpell += 1;
                entityResilienceToSilence = 999;

                entitySpells.Add(entitySO.bonusSpell);

                break;

            case (Escouade.Chien_et_Chats):

                entitySpells.Add(entitySO.bonusSpell);

                break;

            case (Escouade.Les_Monumentaux):

                entityDef += 30;
                entityResilienceToStatDecrease = 999;
                
                break;


            case (Escouade.Le_Ciel_Etoilé):

                entityAgi += 30;

                entitySpells.Add(entitySO.bonusSpell);

                break;

            case (Escouade.Le_Grand_Bleu):

                entityHpMax += 40;
                entityHp = entityHpMax;
                entityMana += 0.2f;

                entitySpells.Add(entitySO.bonusSpell);

                break;

            case (Escouade.Le_Gang_Des_Dragons):

                entityHpMax += 50;
                entityHp = entityHpMax;
                entityDef += 30;

                entitySpells.Add(entitySO.bonusSpell);

                break;

            case (Escouade.Les_Oiseaux_Mythiques):

                entityAgi += 20;
                entityChanceToUseSpell += 1;

                entitySpells.Add(entitySO.bonusSpell);

                break;

            case (Escouade.Les_Minimators):

                entityHpMax += 30;
                entityHp = entityHpMax;
                entityDodge += 5;

                entitySpells.Add(entitySO.bonusSpell);

                break;

            case (Escouade.Les_Croisés):

                entityResilienceToSleep = 999;
                entityResilienceToPoison = 999;

                entitySpells.Add(entitySO.bonusSpell);

                break;

            case (Escouade.Les_Légendaires):

                entityMana += 0.2f;
                entityMpMax += 10;
                entityMp = entityMpMax;

                entitySpells.Add(entitySO.bonusSpell);

                break;

            case (Escouade.Votre_Escouade):

                //Bonus d'escouade

                //entityHpMax += 30;
                //entityHpMax += 50;

                entityHp = entityHpMax; 

                //entityMpMax += 15;
                //entityMpMax += 30;

                entityMp = entityMpMax;

                //entityAtk += 30;
                entityAtk += 50;

                //entityDef += 30;
                entityDef += 50;

                //entityAgi += 15;
                //entityAgi += 30;

                //entityMana += 0.2f;
                //entityMana += 0.3f;

                //entityChanceToUseSpell += 1;
                //entityChanceToUseSpell += 2;

                entitySpells.Add(entitySO.bonusSpell);


                break;
        }
    }

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

                float entityDefUpper = entityDefInit * 1.4f;
                int realEntityDefUpper = (int)entityDefUpper;
                entityDef = realEntityDefUpper;
                break;

            case (2):

                float entityDefMax = entityDefInit * 1.75f;
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

                float entityAtkUpper = entityAtkInit * 1.25f;
                int realEntityAtkUpper = (int)entityAtkUpper;
                entityAtk = realEntityAtkUpper;
                break;

            case (2):

                float entityAtkMax = entityAtkInit * 1.5f;
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
