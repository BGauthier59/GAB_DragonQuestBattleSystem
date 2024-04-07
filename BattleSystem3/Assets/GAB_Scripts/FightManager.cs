using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class FightManager : MonoSingleton<FightManager>
{
    public Sprite emptySprite;
    public Color hitColor;
    public Color healColor;
    public Color defBonusColor;
    public Color atkBonusColor;
    bool isCheckingSpecialAtk;

    public IEnumerator Attacking(EntityManager attacker, EntityManager target)
    {
        if (attacker.entityType == EntityType.Monster)
        {
            //attacker.entityAnim.Play("MonsterAttacks");
            AudioManager.instance.Play("Attack");
        }

        InterfaceManager.instance.Message(true, $"{attacker.entityName} attaque {target.entityName} !");
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        
        int attack = attacker.entityAtk;
        int defense = target.entityDef;

        float damages = ((Random.Range(attack * 0.9f, attack * 1.1f)) / 2) - (defense / 4);
        if (target.isDefending) damages /= 2;

        int realDamages = (int) damages;
        if (realDamages < 0) realDamages = 0;

        if (Dodging(target.entityDodge))
        {
            InterfaceManager.instance.Message(true, $"{target.entityName} a évité l'attaque !");
            yield return new WaitForSeconds(InterfaceManager.instance.time);
        }
        else if (realDamages == 0)
        {
            InterfaceManager.instance.Message(true, $"Mais {target.entityName} ne subit aucun dégât !");
            AudioManager.instance.Play("Miss");
            yield return new WaitForSeconds(InterfaceManager.instance.time);
        }
        else
        {
            if (CriticalHit(target.entityCritical))
            {
                InterfaceManager.instance.Message(true, "Coup critique !");
                yield return new WaitForSeconds(InterfaceManager.instance.time);
                realDamages *= 2;
            }
            
            AudioManager.instance.Play("Hit");
            target.entityImage.color = hitColor;

            InterfaceManager.instance.Message(true, $"{target.entityName} subit {realDamages} points de dégâts !");
            yield return new WaitForSeconds(InterfaceManager.instance.time);

            target.entityHp -= realDamages;
            
            if (target.entityHp <= 0)
            {
                target.entityHp = 0;
                TargetIsDefeated(target);

                InterfaceManager.instance.Message(true, $"{target.entityName} est vaincu(e) !");
                yield return new WaitForSeconds(InterfaceManager.instance.time);
            }
            else
            {
                StatDisplayManager.instance.DisplayStat(target);
                yield return new WaitForSeconds(InterfaceManager.instance.time);
                target.entityImage.color = Color.white;

                if (target.entityStatut == Statut.Endormi)
                {
                    target.entityStatut = Statut.None;
                    target.turnsBeforeRecovering = 0;
                    InterfaceManager.instance.Message(true, $"{target.entityName} se réveille !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }
            }
        }
        BattleManager.instance.hasEntityActed = true;
    }

    public void TargetIsDefeated(EntityManager defeated)
    {
        defeated.isDefeated = true;
        defeated.entityImage.sprite = emptySprite; // Image vide associée
        defeated.entityImage.color = Color.white;
        if (defeated.entityType == EntityType.Monster)
        {
            BattleManager.instance.goldEarned += defeated.gold;
            BattleManager.instance.xpEarned += defeated.xp;

            int randomItem = Random.Range(0, defeated.entitySO.probability);
            if (randomItem == 0 && !BattleManager.instance.hasGotItem)
            {
                BattleManager.instance.hasGotItem = true;
                BattleManager.instance.gotItem = defeated.entitySO.item;
                BattleManager.instance.monsterItem = defeated.entityName;
            }
        }
        StatDisplayManager.instance.DisplayStat(defeated);
    }

    public IEnumerator CastingSpell(EntityManager caster, EntityManager target, SpellSO spell)
    {
        if (caster.entityType == EntityType.Monster)
        {
            caster.entityAnim.Play("MonsterAttacks");
        }
        
        if (spell.spellType == SpellType.Spell)
        {
            AudioManager.instance.Play("SpellCast");
        }

        List<GameObject> targets = new List<GameObject>();
        targets.Clear();

        if (target.entityType == EntityType.Monster)
        {
            if (spell.helpingSpell && caster.entityType == EntityType.Ally) targets = SpawningManager.instance.heroesInBattle;
            else targets = SpawningManager.instance.monstersInBattle;
        }
        else if (target.entityType == EntityType.Ally)
        {
            if (spell.helpingSpell) targets = SpawningManager.instance.monstersInBattle;
            else targets = SpawningManager.instance.heroesInBattle;
        }

        if (caster.entityType == EntityType.Monster) InterfaceManager.instance.Message(true, $"{caster.entityPronoun} {caster.entityName} {spell.inBattleDescription}");
        else InterfaceManager.instance.Message(true, $"{caster.entityName} utilise {spell.spellName} !");
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        
        caster.entityMp -= spell.cost;
        if (spell.SFXname != null) AudioManager.instance.Play(spell.SFXname);

        StatDisplayManager.instance.DisplayStat(caster);

        if (spell.hasSpecialEffect)
        {
            if (!spell.doTargetEveryone)
            {
                if (target.isReflected == true && spell.spellType == SpellType.Spell)
                {
                    target = caster;
                    AudioManager.instance.Play("Réflexion");
                    InterfaceManager.instance.Message(true, $"Mais le miroir magique renvoie le sort !");
                    yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
                }
                StartCoroutine(SpellSpecialEffect(spell, caster, target, true));
            }
            else
            {
                foreach(GameObject t in targets)
                {
                    Debug.Log(t);

                    EntityManager currentTarget = t.GetComponent<EntityManager>();
                    if (!currentTarget.isDefeated)
                    {
                        isCheckingSpecialAtk = true;

                        if (target.isReflected == true && spell.spellType == SpellType.Spell)
                        {
                            target = caster;
                            AudioManager.instance.Play("Réflexion");
                            InterfaceManager.instance.Message(true, $"Mais le miroir magique renvoie le sort !");
                            yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
                        }

                        StartCoroutine(SpellSpecialEffect(spell, caster, currentTarget, false));
                        yield return new WaitWhile(() => isCheckingSpecialAtk);
                    }
                }
                BattleManager.instance.hasEntityActed = true;

            }
        }
        else
        {
            switch (spell.spellType)
            {
                case(SpellType.Aptitude):
                    
                    if (!spell.doTargetEveryone) // Une seule cible
                    {
                        int defense = target.entityDef;
                        if (target.isDefending) defense *= 2;

                        float damages = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (defense / 4)) * spell.factor * 0.01f;
                        int realDamages = (int) damages;
                        if (realDamages < 0) realDamages = Random.Range(0,2);

                        if (Dodging(target.entityDodge))
                        {
                            InterfaceManager.instance.Message(true, $"{target.entityName} a évité l'attaque !");
                            yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                        }
                        else
                        {
                            if (CriticalHit(target.entityCritical))
                            {
                                InterfaceManager.instance.Message(true, "Coup critique !");
                                yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                                realDamages *= 2;
                            }
                            
                            if (realDamages <= 0) AudioManager.instance.Play("Miss");
                            else AudioManager.instance.Play("Hit");
                            target.entityImage.color = hitColor;
                            InterfaceManager.instance.Message(true, $"{target.entityName} subit {realDamages} points de dégâts !");

                            target.entityHp -= realDamages;
                            
                            if (target.entityHp <= 0)
                            {
                                yield return new WaitForSeconds(InterfaceManager.instance.time);
                                target.entityHp = 0;
                                TargetIsDefeated(target);
                                InterfaceManager.instance.Message(true, $"{target.entityName} est vaincu(e) !");
                                yield return new WaitForSeconds(InterfaceManager.instance.time);
                            }
                            else
                            {
                                StatDisplayManager.instance.DisplayStat(target);
                                yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                                target.entityImage.color = Color.white;

                                if (target.entityStatut == Statut.Endormi)
                                {
                                    target.entityStatut = Statut.None;
                                    target.turnsBeforeRecovering = 0;
                                    InterfaceManager.instance.Message(true, $"{target.entityName} se réveille !");
                                    yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                                }

                            }
                            StatDisplayManager.instance.DisplayStat(target);
                        }
                    }
                    else // Plusieurs cibles
                    {
                        for (int i = 0; i < targets.Count; i++)
                        {
                            target = targets[i].GetComponent<EntityManager>();

                            if (!target.isDefeated)
                            {
                                int defense = target.entityDef;
                                if (target.isDefending) defense *= 2;
                            
                                float damages = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (defense / 4)) * spell.factor * 0.01f;
                                int realDamages = (int) damages;
                                if (realDamages < 0) realDamages = Random.Range(0,2);

                                if (Dodging(target.entityDodge))
                                {
                                    InterfaceManager.instance.Message(true, $"{target.entityName} a évité l'attaque !");
                                    yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                                }
                                else
                                {
                                    if (CriticalHit(target.entityCritical))
                                    {
                                        InterfaceManager.instance.Message(true, "Coup critique !");
                                        yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                                        realDamages *= 2;
                                    }

                                    if (realDamages <= 0) AudioManager.instance.Play("Miss");
                                    else AudioManager.instance.Play("Hit");
                                    target.entityImage.color = hitColor;
                                    Debug.Log(target);
                                    InterfaceManager.instance.Message(true, $"{target.entityName} subit {realDamages} points de dégâts !");

                                    target.entityHp -= realDamages;
                                    
                                    if (target.entityHp <= 0)
                                    {
                                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                                        target.entityHp = 0;
                                        TargetIsDefeated(target);
                                        InterfaceManager.instance.Message(true, $"{target.entityName} est vaincu(e) !");
                                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                                    }
                                    else
                                    {
                                        StatDisplayManager.instance.DisplayStat(target);
                                        yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                                        target.entityImage.color = Color.white;

                                        if (target.entityStatut == Statut.Endormi)
                                        {
                                            target.entityStatut = Statut.None;
                                            target.turnsBeforeRecovering = 0;
                                            InterfaceManager.instance.Message(true, $"{target.entityName} se réveille !");
                                            yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                                        }
                                    }
                                    StatDisplayManager.instance.DisplayStat(target);
                                }
                            }
                        }
                    }
                    break;
                
                case (SpellType.Spell):
                    
                    if (!spell.doTargetEveryone) // Une seule cible
                    {
                        float damages = (caster.entityMana * spell.strenght) * Random.Range(0.9f, 1.1f);
                        damages *= SpellElementFactor(spell, target);
                        int realDamages = (int) damages;
                        if (realDamages < 0) realDamages = Random.Range(0,2);

                        if (target.isReflected == true && spell.spellType == SpellType.Spell)
                        {
                            target = caster;
                            AudioManager.instance.Play("Réflexion");
                            InterfaceManager.instance.Message(true, $"Mais le miroir magique renvoie le sort !");
                            yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
                        }

                        AudioManager.instance.Play("Hit");
                        target.entityImage.color = hitColor;
                        InterfaceManager.instance.Message(true, $"{target.entityName} subit {realDamages} points de dégâts !");
                        target.entityHp -= realDamages;
                        StatDisplayManager.instance.DisplayStat(target);
                        yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                        target.entityImage.color = Color.white;

                        if (target.entityHp <= 0)
                        {
                            yield return new WaitForSeconds(InterfaceManager.instance.time);
                            target.entityHp = 0;
                            TargetIsDefeated(target);
                            InterfaceManager.instance.Message(true, $"{target.entityName} est vaincu(e) !");
                            yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                        }
                        StatDisplayManager.instance.DisplayStat(target);
                    }
                    else // Plusieurs cibles
                    {
                        for (int i = 0; i < targets.Count; i++)
                        {
                            target = targets[i].GetComponent<EntityManager>();

                            if (!target.isDefeated)
                            {
                                float damages = (caster.entityMana * spell.strenght) * Random.Range(0.9f, 1.1f);
                                damages *= SpellElementFactor(spell, target);
                                int realDamages = (int) damages;

                                if (target.isReflected == true && spell.spellType == SpellType.Spell)
                                {
                                    target = caster;
                                    AudioManager.instance.Play("Réflexion");
                                    InterfaceManager.instance.Message(true, $"Mais le miroir magique renvoie le sort !");
                                    yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
                                }

                                if (realDamages < 0) realDamages = Random.Range(0,2);

                                AudioManager.instance.Play("Hit");
                                target.entityImage.color = hitColor;
                                InterfaceManager.instance.Message(true, $"{target.entityName} subit {realDamages} points de dégâts !");
                                yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                                target.entityHp -= realDamages;

                                if (CheckDefeat(target))
                                {
                                    InterfaceManager.instance.Message(true, $"{target.entityName} est vaincu(e) !");
                                    yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                                }
                                else
                                {
                                    StatDisplayManager.instance.DisplayStat(target);
                                    yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                                    target.entityImage.color = Color.white;
                                }
                                StatDisplayManager.instance.DisplayStat(target);
                            }
                        }
                    }
                    break;
                
                default:
                    Debug.LogError("Type invalide");
                    break;
            }
            
            BattleManager.instance.hasEntityActed = true;
        }
    }

    private float SpellElementFactor(SpellSO spell, EntityManager target)
    {
        List<ElementType> noEffects = new List<ElementType>(); // Si la cible a ce type, il est insensible au sort
        List<ElementType> weaknesses = new List<ElementType>(); // Si la cible a ce type, il est faible au sort
        List<ElementType> resistances = new List<ElementType>(); // L'inverse
        
        switch (spell.elementType)
        {
            case ElementType.NA:
                return 1;
            
            case ElementType.Vent:
                weaknesses.Add(ElementType.Terre);
                resistances.Add(ElementType.Vent);
                break;
            
            case ElementType.Terre:
                noEffects.Add(ElementType.Vent);
                break;
            
            case ElementType.Plante:
                weaknesses.Add(ElementType.Eau);
                resistances.Add(ElementType.Feu);
                break;
            
            case ElementType.Feu:
                weaknesses.Add(ElementType.Plante);
                resistances.Add(ElementType.Eau);
                break;
            
            case ElementType.Eau:
                weaknesses.Add(ElementType.Feu);
                resistances.Add(ElementType.Plante);
                break;
            
            case ElementType.Obscur:
                weaknesses.Add(ElementType.Divin);
                resistances.Add(ElementType.Feu); resistances.Add(ElementType.Eau); resistances.Add(ElementType.Plante);
                noEffects.Add(ElementType.Obscur);
                break;
            
            case ElementType.Divin:
                weaknesses.Add(ElementType.Obscur);
                resistances.Add(ElementType.Feu); resistances.Add(ElementType.Eau); resistances.Add(ElementType.Plante);
                noEffects.Add(ElementType.Divin);
                break;
            
            default:
                Debug.LogError("Element Type invalide");
                return 1;
        }

        foreach (var noEffectType in noEffects)
        {
            if (target.entitySO.elementType == noEffectType) return 0;
        }
        foreach (var resistanceType in resistances)
        {
            if (target.entitySO.elementType == resistanceType) return .5f;
        }
        foreach (var weaknessType in weaknesses)
        {
            if (target.entitySO.elementType == weaknessType) return 2f;
            
        }
        return 1;
    }

    private IEnumerator SpellSpecialEffect(SpellSO spell, EntityManager caster, EntityManager targetEntity, bool checkOnce)
    {
        if (!spell.helpingSpell) targetEntity.entityImage.color = hitColor;
        else if (spell.spellIndex == 21) targetEntity.entityImage.color = Color.cyan;
        else if (spell.spellIndex == 23) targetEntity.entityImage.color = Color.yellow;
        else if (spell.spellIndex == 26) targetEntity.entityImage.color = Color.magenta;

        if (spell.SFXname != null) AudioManager.instance.Play(spell.SFXname);

        switch (spell.spellIndex)
        {
            case 1: // Heal

                if (!spell.doTargetEveryone)
                {
                    float heal = (spell.strenght * caster.entityMana) * Random.Range(.9f, 1.1f);
                    int realHeal = (int)heal;

                    Debug.Log(targetEntity);

                    targetEntity.entityHp += realHeal;
                    if (targetEntity.entityHp > targetEntity.entityHpMax) targetEntity.entityHp = targetEntity.entityHpMax;

                    targetEntity.entityImage.color = Color.green;
                    AudioManager.instance.Play("Heal");
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} a récupéré {realHeal} points de vie !");
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }
                else
                {
                    List<GameObject> targets = new List<GameObject>();
                    targets.Clear();

                    if (targetEntity.entityType == EntityType.Ally)
                    {
                        targets = SpawningManager.instance.heroesInBattle;
                    }
                    else
                    {
                        targets = SpawningManager.instance.monstersInBattle;
                    }

                    for (int i = 0; i < targets.Count; i++)
                    {
                        EntityManager healTarget = targets[i].GetComponent<EntityManager>();

                        if (!healTarget.isDefeated)
                        {
                            float heal = (spell.strenght * caster.entityMana) * Random.Range(.9f, 1.1f);
                            int realHeal = (int)heal;

                            healTarget.entityHp += realHeal;
                            if (healTarget.entityHp > healTarget.entityHpMax) healTarget.entityHp = healTarget.entityHpMax;


                            healTarget.entityImage.color = Color.green;

                            AudioManager.instance.Play("Heal");
                            InterfaceManager.instance.Message(true, $"{healTarget.entityName} a récupéré {realHeal} points de vie !");
                            StatDisplayManager.instance.DisplayStat(healTarget);

                            yield return new WaitForSeconds(InterfaceManager.instance.time);

                            healTarget.entityImage.color = Color.white;
                        }
                    }
                }

                break;

            case 2: // Resurgence

                int aleaRenaissance = Random.Range(0, 100);

                if (aleaRenaissance < spell.successRate) // Réussite
                {
                    if (targetEntity.entityType == EntityType.Monster)
                    {
                        targetEntity.entityImage.sprite = targetEntity.entitySO.sprite;
                    }
                    targetEntity.entityHp = (int)(targetEntity.entityHpMax * .5f);
                    targetEntity.isDefeated = false;
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est ressuscité(e) !");
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }
                else // Echec
                {
                    InterfaceManager.instance.Message(true, "Le sort a échoué...");
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }

                break;

            case 3: // MP Theft

                int aleaTheft = Random.Range(0, 100);
                int trueChanceToMPTheft = aleaTheft - targetEntity.entityResilienceToMPTheft;

                if (trueChanceToMPTheft <= 0)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} n'est pas affecté(e) !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else if (trueChanceToMPTheft >= spell.successRate)
                {
                    InterfaceManager.instance.Message(true, "Le sort a échoué...");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else
                {
                    int aleaMP = Random.Range(4, 16);
                    if (targetEntity.entityMp < aleaMP) aleaMP = targetEntity.entityMp;

                    targetEntity.entityMp -= aleaMP;
                    caster.entityMp += aleaMP;

                    if (caster.entityMp > caster.entityMpMax) caster.entityMp = caster.entityMpMax;

                    InterfaceManager.instance.Message(true, $"{caster.entityName} a récupéré {aleaMP} points de magie !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }

            case 4: // Protection

                if (!spell.doTargetEveryone)
                {
                    float boost = Random.Range(targetEntity.entityDefInit * 0.2f, targetEntity.entityDefInit * 0.25f);
                    int realBoost = (int)boost;

                    targetEntity.entityDef += realBoost;

                    NewEffect(targetEntity, 3, 1, realBoost);
                    InterfaceManager.instance.Message(true, $"La défense de {targetEntity.entityName} augmente de {realBoost} !");
                }
                else
                {
                    List<GameObject> targets = new List<GameObject>();
                    targets.Clear();

                    if (targetEntity.entityType == EntityType.Ally)
                    {
                        targets = SpawningManager.instance.heroesInBattle;
                    }
                    else
                    {
                        targets = SpawningManager.instance.monstersInBattle;
                    }

                    for (int i = 0; i < targets.Count; i++)
                    {
                        EntityManager spellTarget = targets[i].GetComponent<EntityManager>();

                        if (!spellTarget.isDefeated)
                        {
                            float boost = Random.Range(spellTarget.entityDefInit * 0.2f, spellTarget.entityDefInit * 0.25f);
                            int realBoost = (int)boost;

                            spellTarget.entityDef += realBoost;

                            NewEffect(spellTarget, 3, 1, realBoost);

                            if (i != 0) yield return new WaitForSeconds(InterfaceManager.instance.time);
                            InterfaceManager.instance.Message(true, $"La défense de {spellTarget.entityName} augmente de {realBoost} !");
                        }
                    }
                }
                break;

            case 5: // Coupe claire

                int aleaCritical = Random.Range(0, 100);
                if (aleaCritical >= spell.successRate)
                {
                    AudioManager.instance.Play("Miss");
                    InterfaceManager.instance.Message(true, "Mais l'attaque échoue !");
                    break;
                }
                else
                {
                    float damages5 = ((Random.Range(caster.entityAtk * 1.9f, caster.entityAtk * 2.1f)) / 2) - (targetEntity.entityDef / 4);
                    if (targetEntity.isDefending) damages5 /= 2;
                    int realDamages5 = (int)damages5;
                    if (realDamages5 <= caster.entityAtk) realDamages5 = caster.entityAtk;

                    AudioManager.instance.Play("CriticalHit");
                    InterfaceManager.instance.Message(true, "L'attaque frappe de plein fouet !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityImage.color = hitColor;
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages5} points de dégâts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityHp -= realDamages5;

                    if (CheckDefeat(targetEntity))
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                    }
                }
                targetEntity.entityImage.color = Color.white;
                StatDisplayManager.instance.DisplayStat(targetEntity);
                break;

            case 6: // Multicoups

                List<EntityManager> multiHitsTargets = new List<EntityManager>();
                EntityManager target;

                if (caster.entityType == EntityType.Monster)
                {
                    int hits = 2;

                    for (int i = 0; i <= hits; i++)
                    {
                        multiHitsTargets.Clear();
                        foreach (GameObject hero in SpawningManager.instance.heroesInBattle)
                        {
                            EntityManager entityManager = hero.GetComponent<EntityManager>();
                            if (entityManager.isDefeated) continue;
                            multiHitsTargets.Add(entityManager);
                        }
                        if (multiHitsTargets.Count <= 0)
                        {
                            break;
                        }
                        target = multiHitsTargets[Random.Range(0, multiHitsTargets.Count)];

                        float multiHitsDamages = ((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (target.entityDef / 4) * spell.factor * 0.01f;
                        if (target.isDefending) multiHitsDamages /= 2;
                        int realMultiHitsDamages = (int)multiHitsDamages;
                        if (realMultiHitsDamages <= 0) realMultiHitsDamages = 0;

                        target.entityHp -= realMultiHitsDamages;
                        if (realMultiHitsDamages == 0) AudioManager.instance.Play("Miss");
                        else AudioManager.instance.Play("Hit");

                        InterfaceManager.instance.Message(true, $"{target.entityName} subit {realMultiHitsDamages} points de dégâts !");
                        Debug.Log(target);
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        StatDisplayManager.instance.DisplayStat(target);

                        if (CheckDefeat(target))
                        {
                            InterfaceManager.instance.Message(true, $"{target.entityName} est vaincu(e) !");
                            yield return new WaitForSeconds(InterfaceManager.instance.time);
                        }
                    }
                }
                if (caster.entityType == EntityType.Ally)
                {
                    int hits = 2;

                    for (int i = 0; i <= hits; i++)
                    {
                        multiHitsTargets.Clear();
                        foreach (GameObject monster in SpawningManager.instance.monstersInBattle)
                        {
                            EntityManager entityManager = monster.GetComponent<EntityManager>();
                            if (entityManager.isDefeated) continue;
                            multiHitsTargets.Add(entityManager);
                        }
                        if (multiHitsTargets.Count <= 0)
                        {
                            break;
                        }
                        if (i == 0) target = targetEntity;
                        else target = multiHitsTargets[Random.Range(0, multiHitsTargets.Count)];

                        float multiHitsDamages = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (target.entityDef / 4)) * spell.factor * 0.01f;

                        if (target.isDefending) multiHitsDamages /= 2;
                        int realMultiHitsDamages = (int)multiHitsDamages;
                        if (realMultiHitsDamages <= 0) realMultiHitsDamages = 0;

                        target.entityHp -= realMultiHitsDamages;
                        if (realMultiHitsDamages == 0) AudioManager.instance.Play("Miss");
                        else AudioManager.instance.Play("Hit");

                        InterfaceManager.instance.Message(true, $"{target.entityName} subit {realMultiHitsDamages} points de dégâts !");
                        Debug.Log(target);
                        target.entityImage.color = hitColor;
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        StatDisplayManager.instance.DisplayStat(target);
                        target.entityImage.color = Color.white;

                        if (CheckDefeat(target))
                        {
                            InterfaceManager.instance.Message(true, $"{target.entityName} est vaincu(e) !");
                            yield return new WaitForSeconds(InterfaceManager.instance.time);
                        }
                    }
                }
                break;

            case 7: //Poison

                if (spell.hasEffectAndDamages)
                {
                    float damages7 = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (targetEntity.entityDef / 4)) * spell.factor * 0.01f;
                    if (targetEntity.isDefending) damages7 /= 2;
                    int realDamages7 = (int)damages7;
                    if (realDamages7 <= 0) realDamages7 = 0;

                    if (realDamages7 == 0) AudioManager.instance.Play("Miss");
                    else AudioManager.instance.Play("Hit");

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages7} points de dégâts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityHp -= realDamages7;

                    if (CheckDefeat(targetEntity))
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        break;
                    }

                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }

                if (targetEntity.entityStatut != Statut.None)
                {
                    if (!spell.hasEffectAndDamages)
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} souffre déjà d'un état de statut !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                    }
                    break;
                }

                int aleaPoison = Random.Range(0, 100);
                int truePoisonSpellSuccessRate = spell.successRate - targetEntity.entityResilienceToPoison;

                if (truePoisonSpellSuccessRate <= 0)
                {
                    if (!spell.hasEffectAndDamages)
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} n'est pas affect(é) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                    }
                    break;
                }
                if (aleaPoison >= truePoisonSpellSuccessRate)
                {
                    if (!spell.hasEffectAndDamages)
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} s'en sort indemne !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                    }
                    break;
                }
                else
                {
                    targetEntity.entityStatut = Statut.Empoisonné;

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est empoisonné !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }

            case 8: //Brûlure

                if (spell.hasEffectAndDamages)
                {
                    float damages8 = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (targetEntity.entityDef / 4)) * spell.factor * 0.01f;
                    if (targetEntity.isDefending) damages8 /= 2;
                    int realDamages8 = (int)damages8;
                    if (realDamages8 <= 0) realDamages8 = 0;

                    if (realDamages8 == 0) AudioManager.instance.Play("Miss");
                    else AudioManager.instance.Play("Hit");

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages8} points de dégâts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityHp -= realDamages8;

                    if (CheckDefeat(targetEntity))
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        break;
                    }

                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }

                StatDisplayManager.instance.DisplayStat(targetEntity);

                if (targetEntity.entityStatut != Statut.None)
                {
                    if (!spell.hasEffectAndDamages)
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} souffre déjà d'un état de statut !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                    }
                    break;
                }

                int aleaBurn = Random.Range(0, 100);
                int trueBurnSpellSuccessRate = spell.successRate - targetEntity.entityResilienceToBurn;

                if (trueBurnSpellSuccessRate <= 0)
                {
                    if (!spell.hasEffectAndDamages)
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} n'est pas affect(é) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                    }
                    break;
                }
                if (aleaBurn >= trueBurnSpellSuccessRate)
                {
                    if (!spell.hasEffectAndDamages)
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} s'en sort indemne !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                    }
                    break;
                }
                else
                {
                    targetEntity.entityStatut = Statut.Brûlé;

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est brûlé !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }

            case 9: //Poison magique

                if (spell.hasEffectAndDamages)
                {
                    float damages11 = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (targetEntity.entityDef / 4)) * spell.factor * 0.01f;
                    if (targetEntity.isDefending) damages11 /= 2;
                    int realDamages11 = (int)damages11;
                    if (realDamages11 <= 0) realDamages11 = 0;

                    if (realDamages11 == 0) AudioManager.instance.Play("Miss");
                    else AudioManager.instance.Play("Hit");

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages11} points de dégâts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityHp -= realDamages11;

                    if (CheckDefeat(targetEntity))
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        break;
                    }

                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }

                if (targetEntity.entityStatut != Statut.None)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} souffre déjà d'un état de statut !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }

                int aleaMagPoison = Random.Range(0, 100);
                int trueMagPoisonSpellSuccessRate = spell.successRate - targetEntity.entityResilienceToPoison;

                if (trueMagPoisonSpellSuccessRate <= 0)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} n'est pas affect(é) !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                if (aleaMagPoison >= trueMagPoisonSpellSuccessRate)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} s'en sort indemne !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else
                {
                    targetEntity.entityStatut = Statut.EmpMagique;

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} voit sa magie empoisonnée !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }

            case 10: //Silence

                if (spell.hasEffectAndDamages)
                {
                    float damages11 = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (targetEntity.entityDef / 4)) * spell.factor * 0.01f;
                    if (targetEntity.isDefending) damages11 /= 2;
                    int realDamages11 = (int)damages11;
                    if (realDamages11 <= 0) realDamages11 = 0;

                    if (realDamages11 == 0) AudioManager.instance.Play("Miss");
                    else AudioManager.instance.Play("Hit");

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages11} points de dégâts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityHp -= realDamages11;

                    if (CheckDefeat(targetEntity))
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        break;
                    }

                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }

                if (targetEntity.entityStatut != Statut.None)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} souffre déjà d'un état de statut !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }

                int aleaSilence = Random.Range(0, 100);
                int trueSilenceSpellSuccessRate = spell.successRate - targetEntity.entityResilienceToSilence;
                Debug.Log(trueSilenceSpellSuccessRate);

                if (trueSilenceSpellSuccessRate <= 0)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} n'est pas affect(é) !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                if (aleaSilence >= trueSilenceSpellSuccessRate)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} s'en sort indemne !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else
                {
                    targetEntity.entityStatut = Statut.Silence;
                    targetEntity.turnsBeforeRecovering = Random.Range(3, 7);
                    Debug.Log(targetEntity.turnsBeforeRecovering);
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est réduit(e) au silence !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }

            case 11: //Inaction

                if (spell.hasEffectAndDamages)
                {
                    float damages11 = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (targetEntity.entityDef / 4)) * spell.factor * 0.01f;
                    if (targetEntity.isDefending) damages11 /= 2;
                    int realDamages11 = (int)damages11;
                    if (realDamages11 <= 0) realDamages11 = 0;

                    if (realDamages11 == 0) AudioManager.instance.Play("Miss");
                    else AudioManager.instance.Play("Hit");

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages11} points de dégâts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityHp -= realDamages11;

                    if (CheckDefeat(targetEntity))
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        break;
                    }

                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }

                StatDisplayManager.instance.DisplayStat(targetEntity);

                int aleaInaction = Random.Range(0, 100);

                if (targetEntity.isBlocked == true && !spell.hasEffectAndDamages)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} n'est pas en mesure d'y prêter attention !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else if (aleaInaction >= spell.successRate)
                {
                    if (!spell.hasEffectAndDamages)
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} reste de marbre.");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                    }
                    break;
                }
                else
                {
                    targetEntity.isBlocked = true;

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} {spell.excuseForInaction}");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }

            case 12: //reflexion

                targetEntity = caster;
                caster.isReflected = true;
                InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est protégé par un voile de lumière !");
                yield return new WaitForSeconds(InterfaceManager.instance.time);
                break;

            case 13: //torpeur

                if (targetEntity.entityStatut != Statut.None)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} souffre déjà d'un état de statut !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }

                int aleaSleep = Random.Range(0, 100);
                int trueSleepSpellSuccessRate = spell.successRate - targetEntity.entityResilienceToSleep;

                if (trueSleepSpellSuccessRate <= 0 || targetEntity.isBlocked)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} n'est pas affect(é) !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                if (aleaSleep >= trueSleepSpellSuccessRate)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} s'en sort indemne !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else
                {
                    targetEntity.entityStatut = Statut.Endormi;
                    targetEntity.turnsBeforeRecovering = Random.Range(1, 4);
                    Debug.Log(targetEntity.turnsBeforeRecovering);
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} tombe dans un profond sommeil !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }

            case 14: //paralysie

                if (spell.hasEffectAndDamages)
                {
                    float damages14 = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (targetEntity.entityDef / 4)) * spell.factor * 0.01f;
                    if (targetEntity.isDefending) damages14 /= 2;
                    int realDamages14 = (int)damages14;
                    if (realDamages14 <= 0) realDamages14 = 0;

                    if (realDamages14 == 0) AudioManager.instance.Play("Miss");
                    else AudioManager.instance.Play("Hit");

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages14} points de dégâts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityHp -= realDamages14;

                    if (CheckDefeat(targetEntity))
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        break;
                    }

                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }

                if (targetEntity.entityStatut != Statut.None && !spell.hasEffectAndDamages)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} souffre déjà d'un état de statut !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }

                int aleaPara = Random.Range(0, 100);
                int trueParaSpellSuccessRate = spell.successRate - targetEntity.entityResilienceToSleep;

                if ((trueParaSpellSuccessRate <= 0 || targetEntity.isBlocked) && !spell.hasEffectAndDamages)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} n'est pas affect(é) !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                if (aleaPara >= trueParaSpellSuccessRate)
                {
                    if (!spell.hasEffectAndDamages)
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} s'en sort indemne !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        break;
                    }
                }
                else
                {
                    targetEntity.entityStatut = Statut.Paralysé;
                    targetEntity.turnsBeforeRecovering = Random.Range(1, 4);
                    Debug.Log(targetEntity.turnsBeforeRecovering);
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est paralysé(e) !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }
                break;

            case 15: //soigne du poison

                if (targetEntity.entityStatut == Statut.Empoisonné || targetEntity.entityStatut == Statut.EmpMagique)
                {
                    targetEntity.entityStatut = Statut.None;
                    targetEntity.turnsBeforeRecovering = 0;
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est guéri(e) du poison !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else
                {
                    InterfaceManager.instance.Message(true, $"Mais cela n'a aucun effet sur {targetEntity.entityName} !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }

            case 16: //soigne de la brûlure

                if (targetEntity.entityStatut == Statut.Brûlé)
                {
                    targetEntity.entityStatut = Statut.None;
                    targetEntity.turnsBeforeRecovering = 0;
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est guéri(e) de la brûlure !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else
                {
                    InterfaceManager.instance.Message(true, $"Mais cela n'a aucun effet sur {targetEntity.entityName} !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }

            case 17: //soigne du sommeil

                if (targetEntity.entityStatut == Statut.Endormi)
                {
                    targetEntity.entityStatut = Statut.None;
                    targetEntity.turnsBeforeRecovering = 0;
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est réveillé(e) par le sort !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else
                {
                    InterfaceManager.instance.Message(true, $"Mais cela n'a aucun effet sur {targetEntity.entityName} !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }

            case 18: //soigne de la paralysie

                if (targetEntity.entityStatut == Statut.Paralysé)
                {
                    targetEntity.entityStatut = Statut.None;
                    targetEntity.turnsBeforeRecovering = 0;
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est guéri(e) de la paralysie !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else
                {
                    InterfaceManager.instance.Message(true, $"Mais cela n'a aucun effet sur {targetEntity.entityName} !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }

            case 19: //soigne de tout

                if (targetEntity.entityStatut == Statut.Empoisonné || targetEntity.entityStatut == Statut.EmpMagique || targetEntity.entityStatut == Statut.Brûlé
                    || targetEntity.entityStatut == Statut.Endormi || targetEntity.entityStatut == Statut.Paralysé || targetEntity.entityStatut == Statut.Silence)
                {
                    targetEntity.entityStatut = Statut.None;
                    targetEntity.turnsBeforeRecovering = 0;
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est guéri(e) !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                else
                {
                    InterfaceManager.instance.Message(true, $"Mais cela n'a aucun effet sur {targetEntity.entityName} !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }

            case 20: //Coup miraculeux

                float damages20 = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (targetEntity.entityDef / 4)) * spell.factor * 0.01f;
                if (targetEntity.isDefending) damages20 /= 2;
                int realDamages20 = (int)damages20;
                if (realDamages20 <= 0) realDamages20 = 0;

                if (realDamages20 == 0) AudioManager.instance.Play("Miss");
                else AudioManager.instance.Play("Hit");

                InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages20} points de dégâts !");
                yield return new WaitForSeconds(InterfaceManager.instance.time);
                targetEntity.entityHp -= realDamages20;

                if (CheckDefeat(targetEntity))
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }

                StatDisplayManager.instance.DisplayStat(targetEntity);

                if (realDamages20 >= 3)
                {
                    float healDamages = realDamages20 * 0.3f;
                    int realHealDamages = (int)healDamages;
                    caster.entityHp += (int)realHealDamages;
                    if (caster.entityHp >= caster.entityHpMax) caster.entityHp = caster.entityHpMax;
                    AudioManager.instance.Play("Heal");
                    InterfaceManager.instance.Message(true, $"{caster.entityName} récupère {realHealDamages} PVs !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    StatDisplayManager.instance.DisplayStat(caster);
                }

                break;

            case 21: //augmente la défense

                if (targetEntity.defStatIndex == 2)
                {
                    InterfaceManager.instance.Message(true, $"La défense de {targetEntity.entityName} ne peut plus monter !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }
                else
                {
                    targetEntity.defStatIndex += 1;

                    if (targetEntity.defStatIndex == 0) InterfaceManager.instance.Message(true, $"La défense de {targetEntity.entityName} revient à la normale !");
                    else if (targetEntity.defStatIndex == 1 || targetEntity.defStatIndex == -1) InterfaceManager.instance.Message(true, $"La défense de {targetEntity.entityName} augmente légèrement !");
                    else if (targetEntity.defStatIndex == 2) InterfaceManager.instance.Message(true, $"La défense de {targetEntity.entityName} augmente nettement !");

                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.SetNewDef(targetEntity.defStatIndex);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    Debug.Log(targetEntity.defStatIndex + " + " + targetEntity.entityDef);
                }

                break;

            case 22: //diminue la défense

                if (spell.hasEffectAndDamages)
                {
                    float damages22 = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (targetEntity.entityDef / 4)) * spell.factor * 0.01f;
                    if (targetEntity.isDefending) damages22 /= 2;
                    int realDamages22 = (int)damages22;
                    if (realDamages22 <= 0) realDamages22 = 0;

                    if (realDamages22 == 0) AudioManager.instance.Play("Miss");
                    else AudioManager.instance.Play("Hit");

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages22} points de dégâts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityHp -= realDamages22;

                    if (CheckDefeat(targetEntity))
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        break;
                    }
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }

                if (targetEntity.defStatIndex == -2 && !spell.hasEffectAndDamages)
                {
                    InterfaceManager.instance.Message(true, $"La défense de {targetEntity.entityName} ne peut plus baisser !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }
                else
                {
                    int aleaDef = Random.Range(0, 100);
                    int trueDefSpellSuccessRate = spell.successRate - targetEntity.entityResilienceToStatDecrease;

                    if (aleaDef >= trueDefSpellSuccessRate)
                    {
                        if (!spell.hasEffectAndDamages)
                        {
                            InterfaceManager.instance.Message(true, $"{targetEntity.entityName} s'en sort indemne !");
                            yield return new WaitForSeconds(InterfaceManager.instance.time);
                        }
                    }
                    else
                    {
                        targetEntity.defStatIndex -= 1;

                        if (targetEntity.defStatIndex == 0) InterfaceManager.instance.Message(true, $"La défense de {targetEntity.entityName} revient à la normale !");
                        else if (targetEntity.defStatIndex == 1 || targetEntity.defStatIndex == -1) InterfaceManager.instance.Message(true, $"La défense de {targetEntity.entityName} diminue légèrement !");
                        else if (targetEntity.defStatIndex == -2) InterfaceManager.instance.Message(true, $"La défense de {targetEntity.entityName} diminue nettement !");

                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        targetEntity.SetNewDef(targetEntity.defStatIndex);
                        StatDisplayManager.instance.DisplayStat(targetEntity);
                        Debug.Log(targetEntity.defStatIndex + " + " + targetEntity.entityDef);
                    }
                }
                break;

            case 23: //augmente l'attaque

                if (targetEntity.atkStatIndex == 2)
                {
                    InterfaceManager.instance.Message(true, $"L'attaque de {targetEntity.entityName} ne peut plus monter !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }
                else
                {

                    if (targetEntity.atkStatIndex <= -1)
                    {
                        targetEntity.atkStatIndex = 0;
                        InterfaceManager.instance.Message(true, $"L'attaque de {targetEntity.entityName} revient à la normale !");
                    }
                    else
                    {
                        targetEntity.atkStatIndex = 2;
                        InterfaceManager.instance.Message(true, $"L'attaque de {targetEntity.entityName} augmente nettement !");
                    }

                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.SetNewAtk(targetEntity.atkStatIndex);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    Debug.Log(targetEntity.atkStatIndex + " + " + targetEntity.entityAtk);
                }

                break;

            case 24: //diminue l'attaque

                if (spell.hasEffectAndDamages)
                {
                    float damages24 = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (targetEntity.entityDef / 4)) * spell.factor * 0.01f;
                    if (targetEntity.isDefending) damages24 /= 2;
                    int realDamages24 = (int)damages24;
                    if (realDamages24 <= 0) realDamages24 = 0;

                    if (realDamages24 == 0) AudioManager.instance.Play("Miss");
                    else AudioManager.instance.Play("Hit");

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages24} points de dégâts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityHp -= realDamages24;

                    if (CheckDefeat(targetEntity))
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        break;
                    }
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }

                if (targetEntity.atkStatIndex == -2)
                {
                    InterfaceManager.instance.Message(true, $"L'attaque de {targetEntity.entityName} ne peut plus baisser !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }
                else
                {
                    int aleaAtk = Random.Range(0, 100);
                    int trueAtkSpellSuccessRate = spell.successRate - targetEntity.entityResilienceToStatDecrease;

                    if (aleaAtk >= trueAtkSpellSuccessRate)
                    {
                        if (!spell.hasEffectAndDamages)
                        {
                            InterfaceManager.instance.Message(true, $"{targetEntity.entityName} s'en sort indemne !");
                            yield return new WaitForSeconds(InterfaceManager.instance.time);
                        }
                    }
                    else
                    {
                        targetEntity.atkStatIndex -= 1;

                        if (targetEntity.atkStatIndex == 0) InterfaceManager.instance.Message(true, $"L'attaque de {targetEntity.entityName} revient à la normale !");
                        else if (targetEntity.atkStatIndex == 1 || targetEntity.atkStatIndex == -1) InterfaceManager.instance.Message(true, $"L'attaque de {targetEntity.entityName} diminue légèrement !");
                        else if (targetEntity.atkStatIndex == -2) InterfaceManager.instance.Message(true, $"L'attaque de {targetEntity.entityName} diminue nettement !");

                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        targetEntity.SetNewAtk(targetEntity.atkStatIndex);
                        StatDisplayManager.instance.DisplayStat(targetEntity);
                        Debug.Log(targetEntity.atkStatIndex + " + " + targetEntity.entityAtk);
                    }
                }

                break;

            case 25: //Ne fait rien

                break;

            case 26: //augmente le mana

                if (targetEntity.manaStatIndex == 2)
                {
                    InterfaceManager.instance.Message(true, $"Le mana de {targetEntity.entityName} ne peut plus monter !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }
                else
                {
                    targetEntity.manaStatIndex += 1;

                    if (targetEntity.manaStatIndex == 0) InterfaceManager.instance.Message(true, $"Le mana de {targetEntity.entityName} revient à la normale !");
                    else if (targetEntity.manaStatIndex == 1 || targetEntity.manaStatIndex == -1) InterfaceManager.instance.Message(true, $"Le mana de {targetEntity.entityName} augmente légèrement !");
                    else if (targetEntity.manaStatIndex == 2) InterfaceManager.instance.Message(true, $"Le mana de {targetEntity.entityName} augmente nettement !");

                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.SetNewMana(targetEntity.manaStatIndex);
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    Debug.Log(targetEntity.manaStatIndex + " + " + targetEntity.entityMana);
                }

                break;

            case 27://diminue le mana

                if (spell.hasEffectAndDamages)
                {
                    float damages27 = (((Random.Range(caster.entityAtk * 0.9f, caster.entityAtk * 1.1f)) / 2) - (targetEntity.entityDef / 4)) * spell.factor * 0.01f;
                    if (targetEntity.isDefending) damages27 /= 2;
                    int realDamages27 = (int)damages27;
                    if (realDamages27 <= 0) realDamages27 = 0;

                    if (realDamages27 == 0) AudioManager.instance.Play("Miss");
                    else AudioManager.instance.Play("Hit");

                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages27} points de dégâts !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    targetEntity.entityHp -= realDamages27;

                    if (CheckDefeat(targetEntity))
                    {
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        break;
                    }
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                }

                if (targetEntity.manaStatIndex == -2)
                {
                    InterfaceManager.instance.Message(true, $"Le mana de {targetEntity.entityName} ne peut plus baisser !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                }
                else
                {
                    int aleaMana = Random.Range(0, 100);
                    int trueManaSpellSuccessRate = spell.successRate - targetEntity.entityResilienceToStatDecrease;

                    if (aleaMana >= trueManaSpellSuccessRate)
                    {
                        if (!spell.hasEffectAndDamages)
                        {
                            InterfaceManager.instance.Message(true, $"{targetEntity.entityName} s'en sort indemne !");
                            yield return new WaitForSeconds(InterfaceManager.instance.time);
                        }
                    }
                    else
                    {
                        targetEntity.manaStatIndex -= 1;

                        if (targetEntity.manaStatIndex == 0) InterfaceManager.instance.Message(true, $"Le mana de {targetEntity.entityName} revient à la normale !");
                        else if (targetEntity.manaStatIndex == 1 || targetEntity.manaStatIndex == -1) InterfaceManager.instance.Message(true, $"Le mana de {targetEntity.entityName} diminue légèrement !");
                        else if (targetEntity.manaStatIndex == -2) InterfaceManager.instance.Message(true, $"Le mana de {targetEntity.entityName} diminue nettement !");

                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        targetEntity.SetNewMana(targetEntity.manaStatIndex);
                        StatDisplayManager.instance.DisplayStat(targetEntity);
                        Debug.Log(targetEntity.manaStatIndex + " + " + targetEntity.entityMana);
                    }
                }

                break;

            case 28: //Dégâts fixes

                float damages28 = Random.Range(spell.staticDamages * 0.9f, spell.staticDamages * 1.1f);
                if (targetEntity.isDefending) damages28 /= 2;
                int realDamages28 = (int)(damages28);

                if (realDamages28 == 0) AudioManager.instance.Play("Miss");
                else AudioManager.instance.Play("Hit");

                InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages28} points de dégâts !");
                yield return new WaitForSeconds(InterfaceManager.instance.shortTime);
                targetEntity.entityHp -= realDamages28;

                if (CheckDefeat(targetEntity))
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                StatDisplayManager.instance.DisplayStat(targetEntity);
                break;

            case 29: //augmente l'or et l'exp reçu mais inflige des dégâts

                float damages29 = (caster.gold + caster.xp) / 3;
                if (targetEntity.isDefending) damages29 /= 2;
                int realDamages29 = (int) damages29;

                if (realDamages29 == 0) AudioManager.instance.Play("Miss");
                else AudioManager.instance.Play("Hit");

                InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages29} points de dégâts !");
                yield return new WaitForSeconds(InterfaceManager.instance.time);
                targetEntity.entityHp -= realDamages29;

                if (CheckDefeat(targetEntity))
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    break;
                }
                StatDisplayManager.instance.DisplayStat(targetEntity);

                InterfaceManager.instance.Message(true, $"{caster.entityName} accumule de l'or et de l'expérience !");
                yield return new WaitForSeconds(InterfaceManager.instance.time);
                caster.xp += 10;
                caster.gold += 10;
                StatDisplayManager.instance.DisplayStat(caster);

                break;


            default:
                Debug.LogError("Index du sort invalide");
                break;

        }

        targetEntity.entityImage.color = Color.white;
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        targetEntity.entityImage.color = Color.white;
        if (checkOnce) BattleManager.instance.hasEntityActed = true;
        isCheckingSpecialAtk = false;
    }

    public bool CheckDefeat(EntityManager targetEntity)
    {
        if (targetEntity.entityHp <= 0)
        {
            targetEntity.entityHp = 0;
            TargetIsDefeated(targetEntity);
            return true;
        }
        else
        {
            StatDisplayManager.instance.DisplayStat(targetEntity);
            targetEntity.entityImage.color = Color.white;
            return false;
        }
    }

    public bool Dodging(int dodgeValue)
    {
        int dodge = Random.Range(0, 100);

        if (dodge < dodgeValue)
        {
            AudioManager.instance.Play("Miss");
            return true;
        }
        return false;
    }

    public bool CriticalHit(int criticalValue)
    {
        int critical = Random.Range(10, 100);

        if (critical < criticalValue)
        {
            Debug.Log("critical : + " + critical + " / criticalValue : " + criticalValue);
            AudioManager.instance.Play("CriticalHit");
            return true;
        }
        return false;
    }

    public void NewEffect(EntityManager entity, int duration, int indexStat, float boostValue)
    {
        entity.hasEffect.Add(true);
        entity.effectList.Add(duration);
        entity.statList.Add(indexStat);
        entity.boostList.Add(boostValue);
    }
}
