/*******************************************************************************
File:      EnemyStats.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component controls all behaviors for enemies in the game.

*******************************************************************************/
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public GameObject EnemyHealthBarPrefab;
    private GameObject HealthBar;
    private HealthBar HealthBarComp;

    private EnemyChaseLogic EnemyChaseLogicComp;

    private Kaching kaching;

    public int StartingHealth = 3;
    public int Health
    {
        get { return _Health; }

        set
        {
            HealthBarComp.Health = value;
            _Health = value;
        }

    }
    private int _Health;

    public string AssignedDifficulty;
    public int Difficulty
    {
        get { return _Difficulty; }
        set 
        {
            _Difficulty = value;
        }
    }
    private int _Difficulty;

    public string AssignedType;
    public string Type
    {
        get { return _Type; }
        set
        {
            _Type = value;
        }
    }
    private string _Type;

    public string AssignedModifiers;
    public string Modifiers
    {
        get { return _Modifiers; }
        set
        {
            _Modifiers = value;
        }
    }
    private string _Modifiers;



    // Start is called before the first frame update
    void Start()
    {
        //Initialize enemy health bar
        HealthBar = Instantiate(EnemyHealthBarPrefab);
        HealthBar.GetComponent<ObjectFollow>().ObjectToFollow = transform;
        HealthBarComp = HealthBar.GetComponent<HealthBar>();
        HealthBarComp.MaxHealth = StartingHealth;
        HealthBarComp.Health = StartingHealth;
        Health = StartingHealth;

        kaching = GetComponent<Kaching>();
        AssignedModifiers = Modifiers;

        EnemyChaseLogicComp = GetComponent<EnemyChaseLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var bullet = col.GetComponent<BulletLogic>();
        if (bullet != null && bullet.Team == Teams.Player) {
            Health -= bullet.Power;
            EnemyChaseLogicComp.isShot = true;

            if (Health <= 0)
            {
                kaching.Drop(Difficulty > 2 ? 2 : Difficulty);
                Destroy(gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        Destroy(HealthBar);
    }
}
