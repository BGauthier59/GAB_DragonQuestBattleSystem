using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BattleManager : MonoSingleton<BattleManager>
{
    public List<GameObject> gameOrder = new List<GameObject>();

    public bool isNextTurn;
    public bool hasEntityActed;
    public bool hasSpellBeenChosen;
    public bool hasMonsterBeenTargeted;
    public bool hasEntityStatutBeenVerified;
    public EntityManager entityActing;
    public EntityManager entityTargeted;
    public SpellSO spellSelected;

    public Coroutine AttackCoroutine;
    public Coroutine SpellCoroutine;
    public Coroutine MonsterSpellCoroutine;

    public GameObject spellButtonPrefab;
    public Transform spellButtonPrefabInitTransform;
    public Transform spellButtonsParent;
    
    public GameObject allyButtonPrefab;
    public Transform allyButtonPrefabInitTransform;
    public Transform allyButtonsParent;

    public List<EntityManager> targets = new List<EntityManager>();

    public bool isVictoryChecked;
    public bool isBattleOver;

    public string monsterItem;
    public bool hasGotItem;
    public ItemSO gotItem;
    public int goldEarned;
    public int xpEarned;

    private string statNotif;

    #region Battle begins

    public IEnumerator BattleInitialization()
    {
        InterfaceManager.instance.FirstPhaseButtonsState(false);

        AllyButtonsCreation();
        Initialization();

        monsterItem = null;
        gotItem = null;
        hasGotItem = false;
        goldEarned = 0;
        xpEarned = 0;
        
        InterfaceManager.instance.Message(true, "Des monstres approchent !");
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        InterfaceManager.instance.Message(false, null);

        InterfaceManager.instance.FirstPhaseButtonsState(true);
    }

    public void Initialization()
    {
        for (int i = 0; i < SpawningManager.instance.heroesInBattle.Count; i++)
        {
            EntityManager entityManager = SpawningManager.instance.heroesInBattle[i].GetComponent<EntityManager>();

            entityManager.hasEffect.Clear();
            entityManager.effectList.Clear();
            entityManager.statList.Clear();
            entityManager.boostList.Clear();
            
            StatDisplayManager.instance.DisplayStat(entityManager);
        }
        
        AudioManager.instance.Play("BattleTheme");
    }

    public void AllyButtonsCreation()
    {
        InterfaceManager.instance.alliesButtonsList.Clear();

        Vector3 buttonPos = allyButtonPrefabInitTransform.position;
        for (int i = 0; i < SpawningManager.instance.heroesInBattle.Count; i++)
        {
            InterfaceManager.instance.alliesButtonsList.Add(Instantiate(allyButtonPrefab, buttonPos, 
                Quaternion.identity, allyButtonPrefabInitTransform));
            InterfaceManager.instance.alliesButtonsList[i].GetComponent<AllyButtonManager>().associatedEntity =
                SpawningManager.instance.heroesInBattle[i].GetComponent<EntityManager>();
            InterfaceManager.instance.alliesButtonsList[i].GetComponent<AllyButtonManager>().SetNameOnDisplay();
            InterfaceManager.instance.alliesButtonsList[i].SetActive(false);
            buttonPos += new Vector3(0, 30, 0);
        }
    }

    #endregion

    #region Battle turn

    public void BattleTurn(bool espaceFail)
    {
        FirstPhase();
        DefiningBattleOrder();
        StartCoroutine(ActionPhase(espaceFail));
        StartCoroutine(NextTurnBeginning());
    } // Lance un tour de jeu

    public void Fleeing()
    {
        StartCoroutine(EscapePhase());
    }

    void FirstPhase()
    {
        isNextTurn = false;
        InterfaceManager.instance.FirstPhaseButtonsState(false);
    }

    IEnumerator EscapePhase()
    {
        InterfaceManager.instance.FirstPhaseButtonsState(false);
        
        InterfaceManager.instance.Message(true, "L'équipe s'enfuit...");
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        
        int aleaEspace = Random.Range(0, 3);

        if (aleaEspace == 0) // Fuite réussie
        {
            ResetBattle();
        }
        else // Fuite ratée
        {
            InterfaceManager.instance.Message(true, $"... mais les ennemis bloquent le chemin !");
            yield return new WaitForSeconds(InterfaceManager.instance.time);
            
            BattleTurn(true);
        }
    }
    
    void DefiningBattleOrder()
    {
        gameOrder.Clear();
        List<GameObject> entities = new List<GameObject>();
        
        // Add heroes to the entity list
        foreach (GameObject entity in SpawningManager.instance.heroesInBattle) entities.Add(entity); 
        // Add monsters to the entity list 
        foreach (GameObject entity in SpawningManager.instance.monstersInBattle) entities.Add(entity); 

        List<int> agilities = new List<int>();
        
        SortedDictionary<int, GameObject> agilityList = new SortedDictionary<int, GameObject>();
        
        foreach (GameObject entity in entities) // Link every entity to its speed
        {
            int agilityValue = entity.GetComponent<EntityManager>().entityAgi;
            
            while (agilities.Contains(agilityValue)) agilityValue++;
            
            agilities.Add(agilityValue);
            agilityList.Add(-agilityValue, entity);
        }
        foreach (KeyValuePair<int, GameObject> kv in agilityList) gameOrder.Add(kv.Value);

    } // Set the game order list

    IEnumerator ActionPhase(bool escapeFail)
    {
        isVictoryChecked = true;
        
        for (int i = 0; i < gameOrder.Count; i++)
        {
            hasEntityActed = false;

            EntityManager entityManager = gameOrder[i].GetComponent<EntityManager>();
            if (!entityManager.isDefeated)
            {
                entityManager.isDefending = false;

                yield return new WaitUntil(() => isVictoryChecked);
                isVictoryChecked = false;

                for (int j = 0; j < entityManager.hasEffect.Count; j++)
                {
                    if (CheckingEffect(entityManager, j))
                    {
                        InterfaceManager.instance.Message(true, statNotif);
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                    }
                }
                
                switch (entityManager.entityType)
                {
                    case(EntityType.Ally):
                        if (!escapeFail) StartCoroutine(AllyIsActing(entityManager));
                        else hasEntityActed = true; // Fuite ratée : on passe directement à la suite pour les héros
                        break;
                    
                    case(EntityType.Monster):
                        StartCoroutine(MonsterIsActing(entityManager));
                        break;
                    
                    default:
                        Debug.LogError("Type invalide");
                        break;
                }
            }
            else
            {
                hasEntityActed = true; // Entité vaincue : on passe directement à la suite
            }

            yield return new WaitUntil(() => hasEntityActed);
            
            CheckVictory();
        }
        isNextTurn = true;
    }

    bool CheckingEffect(EntityManager entity, int i)
    {
        statNotif = null;
        
        if (entity.hasEffect[i]) // Il y a un effet associé à cet index
        {
            entity.effectList[i]--;
            if (entity.effectList[i] == 0) // L'effet prend fin
            {
                switch (entity.statList[i])
                {
                    case 0: // Attaque
                        entity.entityAtk -= (int) entity.boostList[i];
                        statNotif = $"L'attaque de {entity.entityName} revient à la normale."; 
                        break;
                    
                    case 1: // Défense
                        entity.entityDef -= (int) entity.boostList[i];
                        statNotif = $"La défense de {entity.entityName} revient à la normale.";
                        break;
                        
                    case 2: // Agilité
                        entity.entityAgi -= (int) entity.boostList[i];
                        statNotif = $"L'agilité de {entity.entityName} revient à la normale.";
                        break;
                        
                    case 3: // Mana
                        entity.entityMana -= entity.boostList[i];
                        statNotif = $"Le mana de {entity.entityName} revient à la normale.";
                        break;
                        
                    case 4: // Critical
                        entity.entityCritical -= (int) entity.boostList[i];
                        statNotif = $"Le taux de critique de {entity.entityName} revient à la normale.";
                        break;
                        
                    case 5: // Dodge
                        entity.entityDodge -= (int) entity.boostList[i];
                        statNotif = $"L'esquive de {entity.entityName} revient à la normale.";
                        break;
                        
                    case 6: // Status
                        entity.entityStatut = Statut.None;
                        statNotif = $"Le statut de {entity.entityName} revient à la normale.";
                        break;
                        
                    default:
                        Debug.LogError("Index invalide");
                        break;
                }

                entity.hasEffect[i] = false;
                return true;
            }
        }
        return false;
    }

    IEnumerator AllyIsActing(EntityManager ally)
    {
        entityActing = ally;
        hasEntityStatutBeenVerified = false;

        StartCoroutine(OnStatutEffect(ally, ally.entityStatut));

        if (ally.entityStatut != Statut.None)
        {
            Debug.Log("vérification statut de " + ally);
            yield return new WaitUntil(() => hasEntityStatutBeenVerified);
        }

        if (ally.isBlocked == true)
        {
            InterfaceManager.instance.Message(true, $"{ally.entityName} ne peut pas bouger !");
            yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
            ally.isBlocked = false;
            hasEntityActed = true;
        }
        else if (ally.entityStatut == Statut.Endormi)
        {
            InterfaceManager.instance.Message(true, $"{ally.entityName} dort à poings fermés !");
            yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
            hasEntityActed = true;
        }
        else
        {
            AudioManager.instance.Play("PrepareAttack");
            InterfaceManager.instance.Message(true, $"C'est au tour de {ally.entityName} ! Que voulez-vous faire ?");
            yield return new WaitForSeconds(InterfaceManager.instance.time);

            InterfaceManager.instance.ActionButtonsState(true); // Les boutons d'actions apparaissent
        }
    }

    public void AttackButtonSelected()
    {
        InterfaceManager.instance.ActionButtonsState(false); // Les boutons d'actions disparaissent
        InterfaceManager.instance.CancelButtonState(true);

        hasMonsterBeenTargeted = false;
        
        for (int i = 0; i < SpawningManager.instance.monstersInBattle.Count; i++)
        {
            EntityManager entityManager = SpawningManager.instance.monstersInBattle[i].GetComponent<EntityManager>();
            if (!entityManager.isDefeated)
            {
                entityManager.IsMonsterSelectable(true);
            }
        }

        AttackCoroutine = StartCoroutine(Attacking());
    }
    
    IEnumerator Attacking()
    {
        yield return new WaitUntil(() => hasMonsterBeenTargeted);

        InterfaceManager.instance.CancelButtonState(false);

        StartCoroutine(FightManager.instance.Attacking(entityActing, entityTargeted));
    }
    
    public void DefendButtonSelected()
    {
        InterfaceManager.instance.ActionButtonsState(false); // Les boutons d'actions disparaissent
        
        StartCoroutine(Defending());
    }

    IEnumerator Defending()
    {
        entityActing.isDefending = true;
        InterfaceManager.instance.Message(true, $"{entityActing.entityName} se défend !");
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        hasEntityActed = true;

    }
    IEnumerator MagicButtonFailed()
    {
        InterfaceManager.instance.Message(true, $"{entityActing.entityName} est dans l'incapacité de lancer des sorts !");
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        StartCoroutine(AllyIsActing(entityActing));
    }

    public void MagicButtonSelected()
    {
        if (entityActing.entityStatut == Statut.Silence)
        {
            StartCoroutine(MagicButtonFailed());
            return;

        }
         
        InterfaceManager.instance.ActionButtonsState(false); // Les boutons d'actions disparaissent
        InterfaceManager.instance.CancelButtonState(true);

        hasSpellBeenChosen = false;
        
        // Choix du sort
        
        InterfaceManager.instance.spellButtonsList.Clear();

        Vector3 buttonsPos = spellButtonPrefabInitTransform.position;
        
        for (int i = 0; i < entityActing.entitySpells.Count; i++)
        {
            InterfaceManager.instance.spellButtonsList.Add(
                Instantiate(spellButtonPrefab, buttonsPos, quaternion.identity, 
                    spellButtonsParent));

            SpellButtonManager spellButtonManager = InterfaceManager.instance.spellButtonsList[i].GetComponent<SpellButtonManager>();
            
            spellButtonManager.associatedSpell = entityActing.entitySpells[i];
            spellButtonManager.SetNameOnDisplay();

            if (entityActing.entitySpells[i].cost > entityActing.entityMp)
            {
                spellButtonManager.GetComponent<Button>().interactable = false;
            }

            buttonsPos += new Vector3(0, 30, 0);
        }

        SpellCoroutine = StartCoroutine(SpellSelected());
    }

    public IEnumerator SpellSelected()
    {
        yield return new WaitUntil(() => hasSpellBeenChosen);

        hasMonsterBeenTargeted = false;
        
        // Choix de la cible

        if (spellSelected.doTargetEveryone)
        {
            if (spellSelected.helpingSpell)
            {
                entityTargeted = SpawningManager.instance.heroesInBattle[0].GetComponent<EntityManager>();
            }
            else
            {
                entityTargeted = SpawningManager.instance.monstersInBattle[0].GetComponent<EntityManager>();
            }
            hasMonsterBeenTargeted = true;
        }

        else
        {
            if (spellSelected.helpingSpell) // Sort de soutien allié
            {
                for (int i = 0; i < InterfaceManager.instance.alliesButtonsList.Count; i++)
                {
                    GameObject button = InterfaceManager.instance.alliesButtonsList[i];
                    button.SetActive(true);

                    EntityManager entityManager = SpawningManager.instance.heroesInBattle[i].GetComponent<EntityManager>();
                
                    if (spellSelected.onlyWorkOnDefeated)
                    {
                        button.GetComponent<Button>().interactable = entityManager.isDefeated;
                    }
                    else
                    {
                        button.GetComponent<Button>().interactable = !entityManager.isDefeated;
                    }
                }

            }
            else // Sort contre l'ennemi
            {
                for (int i = 0; i < SpawningManager.instance.monstersInBattle.Count; i++)
                {
                    EntityManager entityManager = SpawningManager.instance.monstersInBattle[i].GetComponent<EntityManager>();
                    if (!entityManager.isDefeated)
                    {
                        entityManager.IsMonsterSelectable(true);
                    }
                }
            }
        }
        
        MonsterSpellCoroutine = StartCoroutine(CastingSpell());
    }
    
    IEnumerator CastingSpell()
    {
        yield return new WaitUntil(() => hasMonsterBeenTargeted);
        
        InterfaceManager.instance.CancelButtonState(false);

        StartCoroutine(FightManager.instance.CastingSpell(entityActing, entityTargeted, spellSelected));
    }

    public void CancelAction()
    {
        if (AttackCoroutine != null)
        {
            StopCoroutine(AttackCoroutine);
        }
        if (SpellCoroutine != null)
        {
            StopCoroutine(SpellCoroutine);
        }
        if (MonsterSpellCoroutine != null)
        {
            StopCoroutine(MonsterSpellCoroutine);
        }
        
        for (int i = 0; i < SpawningManager.instance.monstersInBattle.Count; i++)
        {
            EntityManager entityManager = SpawningManager.instance.monstersInBattle[i].GetComponent<EntityManager>();
            if (!entityManager.isDefeated)
            {
                entityManager.IsMonsterSelectable(false);
            }
        }
        
        for (int i = 0; i < InterfaceManager.instance.spellButtonsList.Count; i++)
        {
            Destroy(InterfaceManager.instance.spellButtonsList[i]);
        }
        
        for (int i = 0; i < SpawningManager.instance.heroesInBattle.Count; i++)
        {
            InterfaceManager.instance.alliesButtonsList[i].SetActive(false);
        }
        
        InterfaceManager.instance.ActionButtonsState(true); // Les boutons d'actions réapparaissent
        InterfaceManager.instance.CancelButtonState(false);
    }
    
    IEnumerator MonsterIsActing(EntityManager monster)
    {
        entityActing = monster;

        EntityManager target;

        List<SpellSO> spells = new List<SpellSO>();

        StartCoroutine(OnStatutEffect(monster, monster.entityStatut));

        hasEntityStatutBeenVerified = false;

        if (monster.entityStatut != Statut.None)
        {
            Debug.Log("vérification statut de " + monster);
            yield return new WaitUntil(() => hasEntityStatutBeenVerified);
        }

        if (monster.isBlocked == true)
        {
            InterfaceManager.instance.Message(true, $"{monster.entityPronoun} {monster.entityName} ne peut pas bouger !");
            yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
            monster.isBlocked = false;
            hasEntityActed = true;
        }
        else if (monster.entityStatut == Statut.Endormi)
        {
            InterfaceManager.instance.Message(true, $"{monster.entityPronoun} {monster.entityName} dort à poings fermés !");
            yield return new WaitForSeconds(InterfaceManager.instance.time);
            hasEntityActed = true;
        }
        else
        {
            switch (entityActing.entityStrategy)
            {
                // Attaque ou lance une aptitude
                case EntityStrategy.Attaquant:
                    foreach (SpellSO spell in entityActing.entitySpells)
                    {
                        if (entityActing.entityMp >= spell.cost && entityActing.entityStatut != Statut.Silence)
                        {
                            spells.Add(spell);
                        }
                    }

                    foreach (GameObject hero in SpawningManager.instance.heroesInBattle)
                    {
                        EntityManager entityManager = hero.GetComponent<EntityManager>();
                        if (!entityManager.isDefeated)
                        {
                            targets.Add(hero.GetComponent<EntityManager>());
                        }
                    }

                    int aleaAction = Random.Range(1, 3);
                    if (spells.Count == 0 || aleaAction == 1) // Attaque normale
                    {
                        target = targets[Random.Range(0, targets.Count)];
                        StartCoroutine(FightManager.instance.Attacking(entityActing, target));
                    }
                    else // Lance une aptitude
                    {
                        SpellSO chosenSpell = spells[Random.Range(0, spells.Count)];
                        target = targets[Random.Range(0, targets.Count)];
                        StartCoroutine(FightManager.instance.CastingSpell(entityActing, target, chosenSpell));
                    }
                    break;

                // Ne lance que des sorts si possible
                case EntityStrategy.Mage:
                    foreach (SpellSO spell in entityActing.entitySpells)
                    {
                        if (entityActing.entityMp >= spell.cost) spells.Add(spell);
                    }

                    if (spells.Count != 0)
                    {
                        SpellSO selectedSpell = spells[Random.Range(0, spells.Count)];

                        if (selectedSpell.helpingSpell)
                        {
                            foreach (GameObject ally in SpawningManager.instance.monstersInBattle)
                            {
                                EntityManager entityManager = ally.GetComponent<EntityManager>();
                                if (!entityManager.isDefeated)
                                {
                                    targets.Add(ally.GetComponent<EntityManager>());
                                }
                            }
                        }
                        else
                        {
                            foreach (GameObject hero in SpawningManager.instance.heroesInBattle)
                            {
                                EntityManager entityManager = hero.GetComponent<EntityManager>();
                                if (!entityManager.isDefeated)
                                {
                                    targets.Add(hero.GetComponent<EntityManager>());
                                }
                            }
                        }
                        target = targets[Random.Range(0, targets.Count)];
                        StartCoroutine(FightManager.instance.CastingSpell(entityActing, target, selectedSpell));

                    }
                    else
                    {
                        foreach (GameObject hero in SpawningManager.instance.heroesInBattle)
                        {
                            EntityManager entityManager = hero.GetComponent<EntityManager>();
                            if (!entityManager.isDefeated)
                            {
                                targets.Add(hero.GetComponent<EntityManager>());
                            }
                        }
                        target = targets[Random.Range(0, targets.Count)];
                        StartCoroutine(FightManager.instance.Attacking(entityActing, target));
                    }
                    break;

                // Soigne un allié si nécessaire
                case EntityStrategy.Soigneur:

                    var cantHeal = false;

                    // Check les pv des aliés monstres

                    List<EntityManager> alliesToHeal = new List<EntityManager>();
                    for (int i = 0; i < SpawningManager.instance.monstersInBattle.Count; i++)
                    {
                        EntityManager ally = SpawningManager.instance.monstersInBattle[i].GetComponent<EntityManager>();

                        if (!ally.isDefeated && (float)ally.entityHp / ally.entityHpMax < .5f) // Allié à potentiellement soigner
                        {
                            alliesToHeal.Add(ally);
                        }
                    }

                    int aleaHeal = Random.Range(0, 5);

                    if (alliesToHeal.Count != 0 && aleaHeal != 0) // On soigne un ennemi dans la liste
                    {
                        foreach (SpellSO spell in entityActing.entitySpells)
                        {
                            if (entityActing.entityMp >= spell.cost)
                            {
                                spells.Add(spell);
                            }
                        }

                        if (spells.Count != 0) // On applique le sort de soin s'il y en a un de disponible
                        {
                            SpellSO selectedSpell = spells[Random.Range(0, spells.Count)];
                            target = alliesToHeal[Random.Range(0, alliesToHeal.Count)];
                            StartCoroutine(FightManager.instance.CastingSpell(entityActing, target, selectedSpell));
                        }
                        else
                        {
                            cantHeal = true;
                        }

                    }
                    else // On ne soigne personne
                    {
                        cantHeal = true;
                    }

                    // Sinon, attaque
                    if (cantHeal)
                    {
                        foreach (GameObject hero in SpawningManager.instance.heroesInBattle)
                        {
                            EntityManager entityManager = hero.GetComponent<EntityManager>();
                            if (!entityManager.isDefeated)
                            {
                                targets.Add(hero.GetComponent<EntityManager>());
                            }
                        }
                        target = targets[Random.Range(0, targets.Count)];
                        StartCoroutine(FightManager.instance.Attacking(entityActing, target));
                    }
                    break;

                // Ne fait que attaquer
                default:
                    foreach (GameObject hero in SpawningManager.instance.heroesInBattle)
                    {
                        EntityManager entityManager = hero.GetComponent<EntityManager>();
                        if (!entityManager.isDefeated)
                        {
                            targets.Add(hero.GetComponent<EntityManager>());
                        }
                    }
                    target = targets[Random.Range(0, targets.Count)];
                    StartCoroutine(FightManager.instance.Attacking(entityActing, target));
                    break;
            }
        }
        targets.Clear();
        yield return new WaitForSeconds(InterfaceManager.instance.time);
    }

    public IEnumerator OnStatutEffect(EntityManager entity, Statut statut)
    {
        switch (statut)
        {
            case Statut.None:

                hasEntityStatutBeenVerified = true;

                break;

            case Statut.Empoisonné:

                float poisonDamages;
                    
                if (entity.entityType == EntityType.Monster) poisonDamages = entity.entityAtk / 4;
                else poisonDamages = entity.entityHpMax / 16;

                int realPoisonDamages = (int)poisonDamages;
                if (realPoisonDamages >= 48) realPoisonDamages = 48;

                if (entity.entityHp <= realPoisonDamages)
                {
                    InterfaceManager.instance.Message(true, $"{entity.entityName} souffre du poison et perd {math.distance(entity.entityHp, 1)} PVs! Le poison s'est dissipé...");
                    entity.entityHp = 1;
                    entity.entityStatut = Statut.None;
                    StatDisplayManager.instance.DisplayStat(entity);
                    yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
                }
                else
                {
                    InterfaceManager.instance.Message(true, $"{entity.entityName} souffre du poison et perd {realPoisonDamages} PVs!");
                    entity.entityHp -= realPoisonDamages;
                    StatDisplayManager.instance.DisplayStat(entity);
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }
                hasEntityStatutBeenVerified = true;
                break;

            case Statut.Brûlé:

                float burnDamages;

                if (entity.entityType == EntityType.Monster) burnDamages = entity.entityAtk / 3;
                else burnDamages = entity.entityHpMax / 12;

                int realBurnDamages = (int)burnDamages;
                if (realBurnDamages >= 48) realBurnDamages = 48;

                if (entity.entityHp <= realBurnDamages) 
                {
                    InterfaceManager.instance.Message(true, $"{entity.entityName} souffre de la brûlure et perd {math.distance(entity.entityHp, 1)} PVs! La brûlure s'est dissipée...");
                    entity.entityHp = 1;
                    entity.entityStatut = Statut.None;
                    StatDisplayManager.instance.DisplayStat(entity);
                    yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
                }
                else
                {
                    InterfaceManager.instance.Message(true, $"{entity.entityName} souffre de la brûlure et perd {realBurnDamages} PVs!");
                    entity.entityHp -= realBurnDamages;
                    StatDisplayManager.instance.DisplayStat(entity);
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }
                hasEntityStatutBeenVerified = true;
                break;

            case Statut.EmpMagique:

                float magDamages = entity.entityMpMax / 16;
                int realMagDamages = (int)magDamages;
                if (realMagDamages >= 16) realMagDamages = 16;

                if (entity.entityMp <= realMagDamages)
                {
                    InterfaceManager.instance.Message(true, $"{entity.entityName} souffre du poison magique et perd {math.distance(entity.entityMp, 1)} PMs! Le poison magique s'est dissipé...");
                    entity.entityMp = 1;
                    entity.entityStatut = Statut.None;
                    StatDisplayManager.instance.DisplayStat(entity);
                    yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
                }
                else
                {
                    InterfaceManager.instance.Message(true, $"{entity.entityName} souffre du poison magique et perd {realMagDamages} PMs!");
                    entity.entityMp -= realMagDamages;
                    StatDisplayManager.instance.DisplayStat(entity);
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }
                hasEntityStatutBeenVerified = true;
                break;

            case Statut.Silence:

                entity.turnsBeforeRecovering -= 1;
                if (entity.turnsBeforeRecovering <= 0)
                {
                    InterfaceManager.instance.Message(true, $"{entity.entityName} peut à nouveau lancer des sorts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    entity.entityStatut = Statut.None;
                    StatDisplayManager.instance.DisplayStat(entity);
                }
                hasEntityStatutBeenVerified = true;
                break;

            case Statut.Endormi:

                entity.turnsBeforeRecovering -= 1;
                if (entity.turnsBeforeRecovering <= 0)
                {
                    Debug.Log("Réveil !");
                    InterfaceManager.instance.Message(true, $"{entity.entityName} se réveille !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    entity.entityStatut = Statut.None;
                    StatDisplayManager.instance.DisplayStat(entity);
                }
                hasEntityStatutBeenVerified = true;
                break;
        }
        yield return new WaitForSeconds(InterfaceManager.instance.time);
    }

    void CheckVictory()
    {
        // Check héros

        int countHeroes = 0;
        for (int i = 0; i < SpawningManager.instance.heroesInBattle.Count; i++)
        {
            EntityManager entityManager = SpawningManager.instance.heroesInBattle[i].GetComponent<EntityManager>();
            if (entityManager != null && entityManager.isDefeated)
            {
                countHeroes++;
            }
        }

        if (countHeroes == SpawningManager.instance.heroesInBattle.Count)
        {
            BattleIsOver(false);
            return;
        }

        // Check monstres

        int countMonsters = 0;
        for (int i = 0; i < SpawningManager.instance.monstersInBattle.Count; i++)
        {
            EntityManager entityManager = SpawningManager.instance.monstersInBattle[i].GetComponent<EntityManager>();
            if (entityManager != null && entityManager.isDefeated)
            {
                countMonsters++;
            }
        }
        
        if (countMonsters == SpawningManager.instance.monstersInBattle.Count)
        {
            BattleIsOver(true);
            return;
        }
        
        isVictoryChecked = true;
    }

    void BattleIsOver(bool hasTeamWon)
    {
        StopAllCoroutines();
        AudioManager.instance.Stop("BattleTheme");

        if (hasTeamWon)
        {
            StartCoroutine(TeamHasWon());
        }
        else
        {
            StartCoroutine(TeamHasBeenDefeated());
        }
    }

    IEnumerator NextTurnBeginning()
    {
        yield return new WaitUntil(() => isNextTurn);
        InterfaceManager.instance.Message(false, null);
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        InterfaceManager.instance.FirstPhaseButtonsState(true);
    }
    
    #endregion

    #region Battle ends

    IEnumerator TeamHasWon()
    {
        InterfaceManager.instance.Message(true, "Les ennemis sont vaincus !");
        AudioManager.instance.Play("Victory");

        yield return new WaitForSeconds(InterfaceManager.instance.time);

        if (hasGotItem)
        {
            InterfaceManager.instance.Message(true, $"{monsterItem} laisse tomber un objet...");
            yield return new WaitForSeconds(InterfaceManager.instance.time);
            InterfaceManager.instance.Message(true, $"L'équipe obtient : {gotItem.itemName}");
            yield return new WaitForSeconds(InterfaceManager.instance.time * 2);
        }
        
        StatDisplayManager.instance.goldTotal += goldEarned;
        StatDisplayManager.instance.xpTotal += xpEarned;
        
        InterfaceManager.instance.Message(true, $"L'équipe obtient {xpEarned} points d'expérience {goldEarned} pièces d'or.");
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        
        StatDisplayManager.instance.DisplayGoldXp();

        ResetBattle();
    }

    IEnumerator TeamHasBeenDefeated()
    {
        InterfaceManager.instance.Message(true, "L'équipe a été vaincue...");
        AudioManager.instance.Play("Defeat");

        yield return new WaitForSeconds(InterfaceManager.instance.time);

        for (int i = 0; i < SpawningManager.instance.heroesInBattle.Count; i++)
        {
            SpawningManager.instance.heroesInBattle[i].GetComponent<EntityManager>().ResetStat();
            StatDisplayManager.instance.DisplayStat(SpawningManager.instance.heroesInBattle[i].GetComponent<EntityManager>());
        }
        
        ResetBattle();
    }

    void ResetBattle()
    {

        for (int i = 0; i < SpawningManager.instance.heroesInBattle.Count; i++)
        {
            SpawningManager.instance.heroesInBattle[i].GetComponent<EntityManager>().StatAfterBattle();
        }
        for (int i = 0; i < InterfaceManager.instance.alliesButtonsList.Count; i++)
        {
            Destroy(InterfaceManager.instance.alliesButtonsList[i]);
        }
        for (int i = 0; i < SpawningManager.instance.monstersInBattle.Count; i++)
        {
            Destroy(SpawningManager.instance.monstersInBattle[i]);
        }
        InterfaceManager.instance.ActionButtonsState(false);
        InterfaceManager.instance.Start();
    }

    #endregion

}
