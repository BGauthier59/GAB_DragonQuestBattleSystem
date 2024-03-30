using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AllyButtonManager : MonoBehaviour
{
    public EntityManager associatedEntity;
    public TextMeshProUGUI text;

    public void SetNameOnDisplay()
    {
        text.text = associatedEntity.entityName;
    }
    public void OnAllySelected()
    {
        for (int i = 0; i < SpawningManager.instance.heroesInBattle.Count; i++)
        {
            InterfaceManager.instance.alliesButtonsList[i].SetActive(false);
        }

        BattleManager.instance.entityTargeted = associatedEntity;
        BattleManager.instance.hasMonsterBeenTargeted = true;
    }
}
