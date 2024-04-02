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

    public IEnumerator Attacking(EntityManager attacker, EntityManager target)
    {
        if (attacker.entityType == EntityType.Monster)
        {
            attacker.entityAnim.Play("MonsterAttacks");
        }
        
        AudioManager.instance.Play("Attack");
        
        InterfaceManager.instance.Message(true, $"{attacker.entityName} attaque {target.entityName} !");
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        
        int attack = attacker.entityAtk;
        int defense = target.entityDef;
        if (target.isDefending) defense *= 2;

        float damages = ((Random.Range(attack * 0.9f, attack * 1.1f)) / 2) - (defense / 4);
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
                yield return new WaitForSeconds(InterfaceManager.instance.time);
                target.entityImage.color = Color.white;
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
            BattleManager.instance.goldEarned += defeated.entitySO.gold;
            BattleManager.instance.xpEarned += defeated.entitySO.xp;

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

        if (target.entityType == EntityType.Ally)
        {
            targets = SpawningManager.instance.heroesInBattle;
        }
        else
        {
            targets = SpawningManager.instance.monstersInBattle;
        }
        
        if (caster.entityType == EntityType.Ally)
        {
            InterfaceManager.instance.Message(true, $"{caster.entityName} utilise {spell.spellName} !");
            yield return new WaitForSeconds(InterfaceManager.instance.time);
        }
        else
        {
            InterfaceManager.instance.Message(true, $"{caster.entityPronoun} {caster.entityName} {spell.inBattleDescription}");
            yield return new WaitForSeconds(InterfaceManager.instance.needToReadTime);
        }

        caster.entityMp -= spell.cost;
        
        StatDisplayManager.instance.DisplayStat(caster);

        if (spell.hasSpecialEffect)
        {
            StartCoroutine(SpellSpecialEffect(spell, caster, target));
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

                        float damages = (caster.entityAtk * Random.Range(0.9f, 1.1f) - (defense * 2)) *
                                        (spell.factor * 0.01f);
                        int realDamages = (int) damages;
                        if (realDamages < 0) realDamages = Random.Range(0,2);

                        if (Dodging(target.entityDodge))
                        {
                            InterfaceManager.instance.Message(true, $"{target.entityName} a évité l'attaque !");
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
                                yield return new WaitForSeconds(InterfaceManager.instance.time);
                                target.entityImage.color = Color.white;
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
                            
                                float damages = (caster.entityAtk * Random.Range(0.9f, 1.1f) - defense * 2) *
                                                (spell.factor * 0.01f);
                                int realDamages = (int) damages;
                                if (realDamages < 0) realDamages = Random.Range(0,2);

                                if (Dodging(target.entityDodge))
                                {
                                    InterfaceManager.instance.Message(true, $"{target.entityName} a évité l'attaque !");
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
                                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                                        target.entityImage.color = Color.white;
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

                        AudioManager.instance.Play("Hit");
                        target.entityImage.color = hitColor;
                        InterfaceManager.instance.Message(true, $"{target.entityName} subit {realDamages} points de dégâts !");
                        target.entityHp -= realDamages;
                        StatDisplayManager.instance.DisplayStat(target);
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        target.entityImage.color = Color.white;

                        if (target.entityHp <= 0)
                        {
                            yield return new WaitForSeconds(InterfaceManager.instance.time);
                            target.entityHp = 0;
                            TargetIsDefeated(target);
                            InterfaceManager.instance.Message(true, $"{target.entityName} est vaincu(e) !");
                            yield return new WaitForSeconds(InterfaceManager.instance.time);
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

                                if (realDamages < 0) realDamages = Random.Range(0,2);

                                AudioManager.instance.Play("Hit");
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
                                    yield return new WaitForSeconds(InterfaceManager.instance.time);
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

    private IEnumerator SpellSpecialEffect(SpellSO spell, EntityManager caster, EntityManager targetEntity)
    {
        switch (spell.spellIndex)
        {
            case 1: // Heal

                if (!spell.doTargetEveryone)
                {
                    float heal = (spell.strenght * caster.entityMana) * Random.Range(.9f, 1.1f);
                    int realHeal = (int) heal;
                
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
                        EntityManager target = targets[i].GetComponent<EntityManager>();

                        if (!target.isDefeated)
                        {
                            float heal = (spell.strenght * caster.entityMana) * Random.Range(.9f, 1.1f);
                            int realHeal = (int) heal;
                
                            target.entityHp += realHeal;
                            if (target.entityHp > target.entityHpMax) target.entityHp = target.entityHpMax;
                            

                            target.entityImage.color = Color.green;
                            
                            AudioManager.instance.Play("Heal");
                            InterfaceManager.instance.Message(true, $"{target.entityName} a récupéré {realHeal} points de vie !");
                            StatDisplayManager.instance.DisplayStat(target);
                            
                            yield return new WaitForSeconds(InterfaceManager.instance.time);
                            
                            target.entityImage.color = Color.white;
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
                    targetEntity.entityHp = (int) (targetEntity.entityHpMax * .5f);
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
                int trueChanceToWork = aleaTheft - targetEntity.entityResilienceToMPTheft;
                if (trueChanceToWork <= 0)
                {
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} n'est pas affectée !");
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }
                else if (trueChanceToWork >= spell.successRate)
                {
                    InterfaceManager.instance.Message(true, "Le sort a échoué...");
                    StatDisplayManager.instance.DisplayStat(targetEntity);
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
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }
            
            case 4: // Protection
                
                if (!spell.doTargetEveryone)
                {
                    float boost = Random.Range(targetEntity.entityDefInit * 0.2f, targetEntity.entityDefInit * 0.25f);
                    int realBoost = (int) boost;

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
                        EntityManager target = targets[i].GetComponent<EntityManager>();

                        if (!target.isDefeated)
                        {
                            float boost = Random.Range(target.entityDefInit * 0.2f, target.entityDefInit * 0.25f);
                            int realBoost = (int) boost;

                            target.entityDef += realBoost;
                    
                            NewEffect(target, 3, 1, realBoost);

                            if(i != 0) yield return new WaitForSeconds(InterfaceManager.instance.time);
                            InterfaceManager.instance.Message(true, $"La défense de {target.entityName} augmente de {realBoost} !");
                        }
                    }
                }
                
                break;

            case 5: // Coupe claire

                int aleaCritical = Random.Range(0, 100);
                Debug.Log(aleaCritical);
                if (aleaCritical >= spell.successRate)
                {
                    AudioManager.instance.Play("Miss");
                    InterfaceManager.instance.Message(true, "Mais l'attaque échoue !");
                    StatDisplayManager.instance.DisplayStat(targetEntity);
                    break;
                }
                else
                {
                    float damages = ((Random.Range(caster.entityAtk * 1.9f, caster.entityAtk * 2.1f)) / 2) - (targetEntity.entityDef / 4);
                    int realDamages = (int) damages;
                    if (realDamages <= caster.entityAtk) realDamages = caster.entityAtk;

                    AudioManager.instance.Play("CriticalHit");
                    InterfaceManager.instance.Message(true, "L'attaque frappe de plein fouet !");
                    yield return new WaitForSeconds(InterfaceManager.instance.time);
                    InterfaceManager.instance.Message(true, $"{targetEntity.entityName} subit {realDamages} points de dégâts !");
                    targetEntity.entityHp -= realDamages;

                    if (targetEntity.entityHp <= 0)
                    {
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        targetEntity.entityHp = 0;
                        TargetIsDefeated(targetEntity);
                        InterfaceManager.instance.Message(true, $"{targetEntity.entityName} est vaincu(e) !");
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                    }
                    else
                    {
                        StatDisplayManager.instance.DisplayStat(targetEntity);
                        yield return new WaitForSeconds(InterfaceManager.instance.time);
                        targetEntity.entityImage.color = Color.white;
                    }
                }

                break;

            case 6:

            break;


            default:
                Debug.LogError("Index du sort invalide");
                break;
        }
        
        yield return new WaitForSeconds(InterfaceManager.instance.time);
        targetEntity.entityImage.color = Color.white;
        BattleManager.instance.hasEntityActed = true;
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
        int critical = Random.Range(1, 100);

        if (critical < criticalValue)
        {
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
