using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaching : MonoBehaviour
{
    public GameObject moolahPrefab;
    public GameObject HeartPickupPrefab;

    private HeroStats heroStats;

    public int MoolahToLevel {
        get {
            return _moolahToLevel;
        }
        set {
            _moolahToLevel = value;
        }
    }
    private int _moolahToLevel = 5;

    public int PlayerLevel {
        get {
            return _playerLevel;
        }
        set {
            _playerLevel = value;
        }
    }
    private int _playerLevel = 1;

    // Start is called before the first frame update
    void Start()
    {
        heroStats = GetComponent<HeroStats>();
    }

    public void Drop(int xpCount) {
        for (int i = 0; i <= xpCount; i++) {
            Vector3 randomPosition = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            Instantiate(moolahPrefab, randomPosition, Quaternion.identity);
        }
        Instantiate(HeartPickupPrefab, transform.position, Quaternion.identity);
    }

    public void Drop(Vector3 position, GameObject parent) {
        Instantiate(moolahPrefab, position, Quaternion.identity, parent.transform);
        // Instantiate(HeartPickupPrefab, position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
