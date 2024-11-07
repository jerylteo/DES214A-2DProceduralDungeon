using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hoppy : MonoBehaviour
{
    
    // Start is called before the first frame update

    private HeroStats heroStats;
    private PlayerLevelGenerator playerLevelGenerator;

    void Start()
    {
        heroStats = GetComponent<HeroStats>();
        playerLevelGenerator = GetComponent<PlayerLevelGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        // F1 to restart the level
        // F2 to zoom out showing the level (in CameraZoomAndRestart.cs)
        // F3 to toggle God Mode

        if (Input.GetKeyDown(KeyCode.F1))
        {
            // Restart the level
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            playerLevelGenerator.RegenerateLevel();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            // Toggle God Mode
            heroStats.godMode = !heroStats.godMode;
            if (heroStats.godMode) {
                heroStats.SilverKeys += 5;
                heroStats.Power += 10;
            }
            else {
                heroStats.SilverKeys -= 5;
                heroStats.Power -= 10;
            }
        }
    }
}
