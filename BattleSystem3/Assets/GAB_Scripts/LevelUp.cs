using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using System.Reflection;

public class LevelUp : MonoBehaviour
{
    private bool levelRisingFinished;

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

    [SerializeField] private GameObject competenceArea;
    [SerializeField] private TextMeshProUGUI competence;
    [SerializeField] private GameObject weaponArea;
    [SerializeField] private GameObject selfArea;
    [SerializeField] private TextMeshProUGUI weapon;
    [SerializeField] private TextMeshProUGUI self;

    public int currentIndex;
    public int currentCompPoints;
    public int selfPointsGiven;
    public int weaponPointsGiven;

    public void LevellingUp(int index)
    {
        currentIndex = index;
        InterfaceManager.instance.isHeroChosen = true;

        weaponArea.GetComponent<Button>().interactable = true;
        weaponPointsGiven = 0;
        selfArea.GetComponent<Button>().interactable = true;
        selfPointsGiven = 0;

        AudioManager.instance.Play("LevelUp");

        EntityManager entity = SpawningManager.instance.heroesInBattle[index].GetComponent<EntityManager>();

        EntitySO so = entity.entitySO;

        int hpGot = Random.Range(so.hpLevelUp[0], so.hpLevelUp[1] + 1);
        int mpGot = Random.Range(so.mpLevelUp[0], so.mpLevelUp[1] + 1);
        int atkGot = Random.Range(so.atkLevelUp[0], so.atkLevelUp[1] + 1);
        int defGot = Random.Range(so.defLevelUp[0], so.defLevelUp[1] + 1);
        int agiGot = Random.Range(so.agiLevelUp[0], so.agiLevelUp[1] + 1);
        int compGot = Random.Range(6, 9);
        currentCompPoints = compGot;
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
        competence.text = $"Pts de compétence gagnés : {compGot}";

        self.text = $"Compétence {entity.role} ({entity.selfCompetence})";
        weapon.text = $"Compétence {entity.entityWeapon.weaponType} ({entity.weaponCompetence})";
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

    public void OnRisingWeaponCompetence()
    {
        weaponPointsGiven++;
        currentCompPoints--;

        EntityManager entity = SpawningManager.instance.heroesInBattle[currentIndex].GetComponent<EntityManager>();
        entity.weaponCompetence++;
        weapon.text = $"Compétence {entity.entityWeapon.weaponType} ({entity.weaponCompetence}) ( + {weaponPointsGiven})";

        foreach (CompetenceSpell spell in entity.entitySO.competenceSpells)
        {
            if (spell.learningPoints == entity.weaponCompetence && spell.weaponType != WeaponType.Self)
            {
                weapon.text = $"Compétence {entity.entityWeapon.weaponType} ({entity.weaponCompetence}) ( + {weaponPointsGiven}) Appris : {spell.learningSpell.name} !";
            }
        }
    }
    public void OnRisingSelfCompetence()
    {
        selfPointsGiven++;
        currentCompPoints--;

        EntityManager entity = SpawningManager.instance.heroesInBattle[currentIndex].GetComponent<EntityManager>();
        entity.selfCompetence++;
        self.text = $"Compétence {entity.role} ({entity.selfCompetence}) ( + {selfPointsGiven})";

        foreach (CompetenceSpell spell in entity.entitySO.competenceSpells)
        {
            if (spell.learningPoints == entity.selfCompetence && spell.weaponType == WeaponType.Self)
            {
                self.text = $"Compétence {entity.role} ({entity.selfCompetence}) ( + {selfPointsGiven}) Appris : {spell.learningSpell.name} !";
            }
        }
    }

    IEnumerator ShowingLevelStat()
    {
        foreach (var button in InterfaceManager.instance.heroesButton)
        {
            button.interactable = false;
        }

        statArea.SetActive(true);
        competenceArea.SetActive(true);
        weaponArea.SetActive(true);
        selfArea.SetActive(true);
        
        yield return new WaitUntil(() => currentCompPoints == 0);

        weaponArea.GetComponent<Button>().interactable = false;
        selfArea.GetComponent<Button>().interactable = false;

        yield return new WaitForSeconds(2);

        statArea.SetActive(false);
        competenceArea.SetActive(false);

        InterfaceManager.instance.isHeroChosen = false;
        InterfaceManager.instance.OnLevelUp();
    }

    private void Update()
    {
        //inputPressed = Input.GetKeyDown(KeyCode.Mouse0);
    }
}
