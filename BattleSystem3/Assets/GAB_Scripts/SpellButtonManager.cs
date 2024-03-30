using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpellButtonManager : MonoBehaviour
{
    public SpellSO associatedSpell;
    public TextMeshProUGUI text;
    
    public GameObject descriptionObj;
    public TextMeshProUGUI descriptionText;
    public void SetNameOnDisplay()
    {
        text.text = associatedSpell.spellName;
        descriptionText.text = $"{associatedSpell.spellDescription} | {associatedSpell.spellType} | MP : {associatedSpell.cost}" ;
    }
    public void OnSpellSelected()
    {
        BattleManager.instance.spellSelected = associatedSpell;
        
        for (int i = 0; i < InterfaceManager.instance.spellButtonsList.Count; i++)
        {
            Destroy(InterfaceManager.instance.spellButtonsList[i]);
        }
        
        BattleManager.instance.hasSpellBeenChosen = true;
    }

    public void DisplayDescription(bool active)
    {
        descriptionObj.SetActive(active);
    }
}
