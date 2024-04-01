using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class SpawningManager : MonoSingleton<SpawningManager>
{
    #region Variables
    
    public EntitySO[] heroes;
    
    public int howManyMonster;
    
    public ConfigSO[] configsInLocation;

    public BattleAreaSO[] allZones;
    public BattleAreaSO selectedZone;
    public bool isNight;

    private List<EntitySO> monstersFighting = new List<EntitySO>();

    public Transform centerBattle;
    private Vector3 centerBattleSave;

    public List<GameObject> heroesInBattle = new List<GameObject>();
    public List<GameObject> monstersInBattle = new List<GameObject>();

    [Header("Prefabs")]
    public GameObject heroPrefab;
    public GameObject monsterPrefab;

    #endregion

    private void Start()
    {
        centerBattleSave = centerBattle.position;
        CreatingHeroesInBattle();
    }

    public void Spawning()
    {
        InterfaceManager.instance.areas.gameObject.SetActive(false);
        monstersInBattle.Clear();
        centerBattle.position = centerBattleSave;

        InterfaceManager.instance.SetEnvironment(isNight ? selectedZone.fontZoneNight : selectedZone.fontZoneDay);

        configsInLocation = SelectingArray();
        DefiningMonster(configsInLocation);
        CreatingMonstersInBattle();
        StartCoroutine(BattleManager.instance.BattleInitialization());
    }

    void CreatingHeroesInBattle()
    {
        for (int i = 0; i < heroes.Length; i++)
        {
            if (heroes[i] == null) continue;
            GameObject hero = Instantiate(heroPrefab, transform.position, Quaternion.identity, centerBattle);
            EntityManager heroManager = hero.GetComponent<EntityManager>();
            heroManager.entitySO = heroes[i];
            heroManager.entityIndex = i;
            heroManager.LinkingStat();
            heroManager.LinkingEquipments();
            heroManager.LinkingToDisplay();
            heroManager.SetSpells();
            heroesInBattle.Add(hero);
        }
        
    } // Crée les héros et associe les statistiques

    ConfigSO[] SelectingArray()
    {
        if(isNight) return selectedZone.configsInZoneNight;
        return selectedZone.configsInZoneDay;
    } // Associe la liste d'ennemis à la zone où a lieu le combat
    
    void DefiningMonster(ConfigSO[] configs)
    {   
        monstersFighting.Clear();
        
        monstersFighting = configs[Random.Range(0, configs.Length)].monsters.ToList();

        howManyMonster = monstersFighting.Count;

    } // Defines the amount and type of encountered monsters

    void CreatingMonstersInBattle()
    {
        Vector3 monsterPos = centerBattle.position;
        for (int i = 0; i < monstersFighting.Count; i++)
        {
            GameObject enemy = Instantiate(monsterPrefab, monsterPos, Quaternion.identity, centerBattle);
            EntityManager enemyManager = enemy.GetComponent<EntityManager>();
            enemyManager.entitySO = monstersFighting[i];
            enemyManager.LinkingStat(); // Lie les stats du SO au GameObject
            enemyManager.LinkingImage(); // Affiche le sprite du monstre
            
            monstersInBattle.Add(enemy);
            
            monsterPos -= new Vector3(-150, 0, 0);
        }

        centerBattle.position -= new Vector3((howManyMonster - 1) * 150 * 0.5f, 0, 0);


    } // Fait apparaitre les ennemis en combat
}

public enum CombatZone
{
    NA, Région_Bondaibut, Royaume_Iadlajoie_Nord, Royaume_Iadlajoie_Sud, Région_Delsor, Ruines_Ancienne_Cité, Alentours_Monastère, Région_Port_Lemerche, Côtes_Rempart_Falaise, Royaume_Gulrur,
    Région_Osédur, Caverne_rite, Région_Port_Lotecque, Région_Ordréal, Epave, Océan_nord, Océan_sud, Îles_mer_intérieure, Péninsule_polaire, Presqu_île_Dalgor, Cap_Dalgorique, Région_Sanctum, 
    Île_chanceliers, Domarium_Alad,
}
