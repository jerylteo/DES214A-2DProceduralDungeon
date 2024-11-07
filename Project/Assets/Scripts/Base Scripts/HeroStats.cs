/*******************************************************************************
File:      HeroStats.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is keeps track of all relevant hero stats. It also handles
    collisions with objects that would modify any stat.

    - MaxHealth = 5
    - Power = 1
    - Speed = 6

*******************************************************************************/
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class HeroStats : MonoBehaviour
{
    //Hero Stats
    public GameObject MainCameraPrefab;
    public GameObject UiCanvasPrefab;
    private UiStatsDisplay HeroStatsDisplay;
    private TopDownController playerController;
    private PlayerLevelGenerator playerLevelGenerator;
    private Kaching kaching;
    private HeroShoot heroShoot;

    public bool godMode {
        get { return _godMode; }
        set {
            _godMode = value;

            HealthBar healthBar = HeroStatsDisplay.HealthBarDisplay;

            if (healthBar != null) {
                healthBar.FilledTickColor = value ? Color.yellow : Color.red;
                healthBar.Health = _Health;
            }
        }
    }
    private bool _godMode = false;

    public int StartingHealth = 3;
    public int MaxHealth
    {
        get { return _MaxHealth; }

        set
        {
            HeroStatsDisplay.HealthBarDisplay.MaxHealth = value;
            _MaxHealth = value;
        }
    }
    private int _MaxHealth;

    public int Health
    {
        get { return _Health; }

        set
        {
            HeroStatsDisplay.HealthBarDisplay.Health = value;
            if (godMode) {
                // HeroStatsDisplay.HealthBarDisplay.HealthBarBackground.color = Color.green;
            }
            _Health = value;
        }

    }
    private int _Health;

    public int StartingSilverKeys = 0;
    public int SilverKeys
    {
        get { return _SilverKeys; }
        set
        {
            HeroStatsDisplay.SilverKeyDisplay.text = value.ToString();
            _SilverKeys = value;
        }
    }
    private int _SilverKeys;

    public int StartingGoldKeys = 0;
    public int GoldKeys
    {
        get { return _GoldKeys; }
        set
        {
            HeroStatsDisplay.GoldKeyDisplay.text = value.ToString();
            _GoldKeys = value;
        }
    }
    private int _GoldKeys;

    public int StartingPower = 1;
    public int Power
    {
        get { return _Power; }
        set
        {
            HeroStatsDisplay.PowerDisplay.text = value.ToString();
            _Power = value;
        }
    }
    private int _Power;

    public int StartingSpeed = 6;
    public int Speed {
        get { return _Speed; }
        set
        {
            _Speed = value;
            if (playerController != null) {
                playerController.Speed = value;
                HeroStatsDisplay.SpeedDisplay.text = value.ToString();
            }
        }
    }
    private int _Speed;

    public int StartingMoolah = 0;
    public int Moolah {
        get { return _Moolah; }
        set {
            _Moolah = value;

            if (_Moolah >= kaching.MoolahToLevel) {
                kaching.PlayerLevel++;
                kaching.MoolahToLevel += 5;
                _Moolah = 0;
                LevelUp();
            }

            HeroStatsDisplay.MoolahDisplay.text = value.ToString();
            HeroStatsDisplay.MoolahToLevelDisplay.text = kaching.MoolahToLevel.ToString();
        }
    }
    private int _Moolah;

    // Start is called before the first frame update
    void Start()
    {
        //Spawn canvas
        var canvas = Instantiate(UiCanvasPrefab);
        HeroStatsDisplay = canvas.GetComponent<UiStatsDisplay>();

        //Spawn main camera
        var cam = Instantiate(MainCameraPrefab);
        cam.GetComponent<ObjectFollow>().ObjectToFollow = transform;

        playerController = GetComponent<TopDownController>();
        playerLevelGenerator = GetComponent<PlayerLevelGenerator>();
        kaching = GetComponent<Kaching>();
        heroShoot = GetComponent<HeroShoot>();

        //Initialize stats
        MaxHealth = StartingHealth;
        Health = MaxHealth;
        SilverKeys = StartingSilverKeys;
        GoldKeys = StartingGoldKeys;
        Power = StartingPower;
        Speed = StartingSpeed;
        Moolah = 0;
    }

    void LevelUp() {
        MaxHealth++;
        Health = MaxHealth;
        if (kaching.PlayerLevel % 2 == 0) {
            Power++;
        }
        Speed++;
        heroShoot.LevelUp();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Check collision against collectibles
        var collectible = collision.gameObject.GetComponent<CollectibleLogic>();
        if (collectible != null)
        {
            //Increment relevant stat baed on Collectible type
            switch (collectible.Type)
            {
                case CollectibleTypes.HealthBoost:
                    ++MaxHealth;
                    Health = MaxHealth;
                    break;
                case CollectibleTypes.SilverKey:
                    ++SilverKeys;
                    break;
                case CollectibleTypes.GoldKey:
                    ++GoldKeys;
                    break;
                case CollectibleTypes.PowerBoost:
                    ++Power;
                    break;
                case CollectibleTypes.SpeedBoost:
                    ++Speed;
                    break;
                case CollectibleTypes.Heart:
                    if (Health == MaxHealth)
                        return;
                    ++Health;
                    break;
                case CollectibleTypes.Coin:
                    ++Moolah;
                    break;
            }

            //Destroy collectible
            Destroy(collectible.gameObject);

        }//Collectibles End

        //Check collsion against enemy bullets
        var bullet = collision.GetComponent<BulletLogic>();
        if (bullet != null && bullet.Team == Teams.Enemy && !godMode)
        {
            Health -= bullet.Power;

            if (Health <= 0)
            {
                gameObject.SetActive(false);
                Invoke("ResetLevel", 1.5f);
            }
        }

        // Check collision against Traps & Enemy
        if ((collision.gameObject.CompareTag("Trap") || collision.gameObject.CompareTag("Enemy")) && !godMode)
        {
            Health -= 1;

            if (Health <= 0)
            {
                gameObject.SetActive(false);
                Invoke("ResetLevel", 1.5f);
            }
        }
    }

    void ResetLevel()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        playerLevelGenerator.RegenerateLevel();
    }
}
