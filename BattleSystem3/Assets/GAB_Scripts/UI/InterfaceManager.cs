using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InterfaceManager : MonoSingleton<InterfaceManager>
{
    public float time = 1.0f;
    public float needToReadTime = 1.5f;

    public TMP_Dropdown areas;
    public Image environment;

    public GameObject dialogueObject;
    public TextMeshProUGUI menuDialogue;

    public Button battleButton;
    
    public Button fightButton;
    public Button attackButton;
    public Button defendButton;
    public Button magicButton;
    public Button infoButton;
    public Button fleeButton;

    public Button cancelActionButton;

    public List<GameObject> spellButtonsList = new List<GameObject>();
    public List<GameObject> alliesButtonsList = new List<GameObject>();

    public Button levelUpButton;
    public bool isLevellingUp;
    public bool isHeroChosen;
    public Button[] heroesButton = new Button[4];
    public Toggle nightToggle;

    
    public void Start()
    {
        Initialization();
    }

    public void Initialization()
    {
        dialogueObject.SetActive(false);
        
        areas.gameObject.SetActive(true);
    }

    public void OnAreaSelected(int index)
    {
        if (index <= SpawningManager.instance.allZones.Length && index > 0)
        {
            SpawningManager.instance.selectedZone = SpawningManager.instance.allZones[index - 1];
        }
    }

    public void OnLevelUp()
    {
        isLevellingUp = !isLevellingUp;
        
        levelUpButton.interactable = !isLevellingUp;
        battleButton.interactable = !isLevellingUp;

        for (int i = 0; i < SpawningManager.instance.heroesInBattle.Count; i++)
        {
            heroesButton[i].interactable = isLevellingUp;
        }
    }

    public void SetEnvironment(Sprite sprite)
    {
        environment.sprite = sprite;
    }

    private void Update()
    {
        if (isLevellingUp && !isHeroChosen && Input.GetKeyDown(KeyCode.Mouse1))
        {
            OnLevelUp();
        }
    }

    public void BattleStarts()
    {
        if (SpawningManager.instance.selectedZone != null)
        {
            StopAllCoroutines();
            foreach (var button in heroesButton)
            {
                button.interactable = false;
            }

            SpawningManager.instance.isNight = nightToggle.isOn;
            SpawningManager.instance.Spawning();
        }
    }

    public void HealEveryone()
    {
        AudioManager.instance.Play("Heal");
        for (int i = 0; i < SpawningManager.instance.heroesInBattle.Count; i++)
        {
            EntityManager entity = SpawningManager.instance.heroesInBattle[i].GetComponent<EntityManager>();
            entity.ResetStat();
            StatDisplayManager.instance.DisplayStat(entity);
        }
    }

    public void Message(bool active, string message)
    {
        dialogueObject.SetActive(active);
        menuDialogue.text = message;
    }

    public void FirstPhaseButtonsState(bool active)
    {
        fightButton.gameObject.SetActive(active);
        fightButton.interactable = active;
        
        fleeButton.gameObject.SetActive(active);
        fleeButton.interactable = active;
    }
    
    public void ActionButtonsState(bool active)
    {
        attackButton.gameObject.SetActive(active);
        defendButton.gameObject.SetActive(active);
        magicButton.gameObject.SetActive(active);
        infoButton.gameObject.SetActive(active);
        
        attackButton.interactable = active;
        defendButton.interactable = active;
        magicButton.interactable = active;
    }

    public void CancelButtonState(bool active)
    {
        cancelActionButton.gameObject.SetActive(active);
        cancelActionButton.interactable = active;
    }
}
