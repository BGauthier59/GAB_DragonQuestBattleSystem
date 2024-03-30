using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
    private bool inputPressed;

    [SerializeField] private GameObject statArea;
    
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI atk;
    [SerializeField] private TextMeshProUGUI def;
    [SerializeField] private TextMeshProUGUI agi;
    [SerializeField] private TextMeshProUGUI hp;
    [SerializeField] private TextMeshProUGUI mp;
    [SerializeField] private TextMeshProUGUI mana;
    [SerializeField] private TextMeshProUGUI lv;
    [SerializeField] private TextMeshProUGUI spells;

    public void LevellingUp(int index)
    {
        InterfaceManager.instance.isHeroChosen = true;
        
        AudioManager.instance.Play("LevelUp");

        EntityManager entity = SpawningManager.instance.heroesInBattle[index].GetComponent<EntityManager>();

        EntitySO so = entity.entitySO;

        int hpGot = Random.Range(so.hpLevelUp[0], so.hpLevelUp[1] + 1);
        int mpGot = Random.Range(so.mpLevelUp[0], so.mpLevelUp[1] + 1);
        int atkGot = Random.Range(so.atkLevelUp[0], so.atkLevelUp[1] + 1);
        int defGot = Random.Range(so.defLevelUp[0], so.defLevelUp[1] + 1);
        int agiGot = Random.Range(so.agiLevelUp[0], so.agiLevelUp[1] + 1);
        float manaGot = (float) Math.Round(Random.Range(so.manaLevelUp[0], so.manaLevelUp[1]), 2);
        string learnedSpell = null;

        foreach (var learning in entity.entitySO.learnSpells)
        {
            if (entity.entitytLv == learning.learningLevel)
            {
                if (learnedSpell != null) learnedSpell += ", ";
                learnedSpell += $"{learning.learningSpell.spellName}";
            }
        }

        entity.OnNewLevel(hpGot, mpGot, atkGot, defGot, agiGot, manaGot);
        entity.SetSpells();
        
        name.text = entity.entityName;
        hp.text = $" HP : {entity.entityHpMax}  ( + {hpGot})";
        mp.text = $" MP : {entity.entityMpMax} ( + {mpGot})";
        atk.text = $" ATK : {entity.entityAtkInit} ( + {atkGot})";
        def.text = $" DEF : {entity.entityDefInit} ( + {defGot})";
        agi.text = $" AGI : {entity.entityAgiInit} ( + {agiGot})";
        mana.text = $" MANA : {entity.entityManaInit} ( + {manaGot})";
        lv.text = $"Lv : {entity.entitytLv}";
        if (learnedSpell != null)
        {
            spells.text = $"{entity.entityName} a appris : {learnedSpell}";
        }
        else
        {
            spells.text = null;
        }

        StatDisplayManager.instance.DisplayStat(entity);

        StartCoroutine(ShowingLevelStat());
    }

    IEnumerator ShowingLevelStat()
    {
        foreach (var button in InterfaceManager.instance.heroesButton)
        {
            button.interactable = false;
        }

        statArea.SetActive(true);
        
        yield return new WaitUntil(() => inputPressed);

        statArea.SetActive(false);

        InterfaceManager.instance.isHeroChosen = false;
        InterfaceManager.instance.OnLevelUp();
    }

    private void Update()
    {
        inputPressed = Input.GetKeyDown(KeyCode.Mouse0);
    }
}
