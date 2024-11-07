/*******************************************************************************
File:      HeroShoot.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is added to the player and is responsible for the player's
    shoot ability and rotating the player to face the mouse position.

*******************************************************************************/
using UnityEngine;

[RequireComponent(typeof(HeroStats))]
public class HeroShoot : MonoBehaviour
{
    public GameObject BulletPrefab;
    public float BulletSpeed = 5.0f;
    public float ShotCooldown = 1.0f;

    private float Timer = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
    }
    
    public void LevelUp() {
        BulletSpeed += .2f;
        ShotCooldown -= 0.05f;
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate player towards mouse position
        var worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);
        transform.up = (worldMousePos - transform.position).normalized;

        Timer += Time.deltaTime;

        if (Timer >= ShotCooldown && Input.GetMouseButton(0))
        {
            //Spawn Bullet
            var obj = Instantiate(BulletPrefab, transform.position, Quaternion.identity);

            //Rotate bullet to match player direction
            obj.transform.up = transform.up;

            //Add bullet velocity
            obj.GetComponent<Rigidbody2D>().velocity = transform.up * BulletSpeed;

            //Set bullet's power
            obj.GetComponent<BulletLogic>().Power = GetComponent<HeroStats>().Power;

            //Reset shoot timer
            Timer = 0f;
        }
    }
}
