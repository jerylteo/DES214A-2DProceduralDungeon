/*******************************************************************************
File:      EnemyChaseLogic.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/6/2018
Course:    CS186
Section:   Z

Description:
    This component is responsible for the chase behavior on some enemies.

*******************************************************************************/
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaseLogic : MonoBehaviour
{
    public float AggroRange = 8f;
    public float DeaggroCooldown = 2f;
    public float AggroCooldown = 3f;
    private float CurrentDeaggroTime = 0f;
    private float CurrentAggroTime = 0f;
    public float DeaggroRange = 10f;
    public float MoveSpeed = 5f;
    public bool Aggroed = false;
    public bool isShot = false;

    private Transform Player;
    private Rigidbody2D RB;

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.Find("Hero");
        if (player != null) Player = player.transform;
        RB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame

    void Update()
    {
        //No reference to player, Nothing to chase
        if (Player == null || !Player.gameObject.activeSelf)
        {
            RB.velocity = Vector2.zero;
            Aggroed = false;
            return;
        }

        //If player is within aggro range, chase it!
        var dir = (Player.position - transform.position);
        if (dir.magnitude <= AggroRange || isShot)
        {
            //Rotate to face the player
            transform.up = dir.normalized;

            Aggroed = true;
            //Move at designated velocity
            RB.velocity = transform.up * MoveSpeed;
            // isShot = false;

            CurrentAggroTime += Time.deltaTime;
            if (CurrentAggroTime >= AggroCooldown)
            {
                isShot = false;
                CurrentAggroTime = 0f;
            }
        }

        if(dir.magnitude >= DeaggroRange)
        {
            if (!isShot) {
                Aggroed = false;
                RB.velocity = Vector2.zero;
            }
            else {
                CurrentDeaggroTime += Time.deltaTime;
                if (CurrentDeaggroTime >= DeaggroCooldown) {
                    isShot = false;
                    CurrentDeaggroTime = 0f;
                    // Aggroed = false;
                    // RB.velocity = Vector2.zero;
                }
                Debug.Log($"Deaggro Time: {CurrentDeaggroTime}");
            }
        }

    }
}
