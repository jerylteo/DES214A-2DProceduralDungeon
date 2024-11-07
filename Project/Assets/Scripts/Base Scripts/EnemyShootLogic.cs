/*******************************************************************************
File:      EnemyChaseLogic.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/6/2018
Course:    CS186
Section:   Z

Description:
    This component is responsible for the shoot behavior on some enemies.

*******************************************************************************/
using UnityEngine;

[RequireComponent(typeof(EnemyChaseLogic))]
public class EnemyShootLogic : MonoBehaviour
{
    public enum ShotType { Single, Shotgun}

    public GameObject BulletPrefab;
    public ShotType Type = ShotType.Single;
    public int Power = 1;
    public float ShootCooldown = 1f;
    public float BulletSpeed = 8f;

    //Shotgun only properties
    public int ShotgunBullets = 3;
    public float ShotgunAngle = 0.5f;

    private EnemyChaseLogic ChaseBehavior;
    private float Timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        ChaseBehavior = GetComponent<EnemyChaseLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!ChaseBehavior.Aggroed)
            return;

        Timer += Time.deltaTime;

        if (Timer >= ShootCooldown)
        {
            if (Type == ShotType.Single)
            {
                var bullet = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
                bullet.transform.up = transform.up;
                bullet.GetComponent<Rigidbody2D>().velocity = transform.up * BulletSpeed;
                bullet.GetComponent<BulletLogic>().Power = Power;
            }
            else if (Type == ShotType.Shotgun)
            {
                for (int i = 0; i < ShotgunBullets; i++)
                {
                    var fwd = RotateVector(transform.up, -ShotgunAngle * ((float)ShotgunBullets / 2f) + (ShotgunAngle * i));
                    var bullet = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
                    bullet.transform.up = fwd.normalized;
                    bullet.GetComponent<Rigidbody2D>().velocity = fwd * BulletSpeed;
                    bullet.GetComponent<BulletLogic>().Power = Power;

                }
            }

            Timer = 0;
        }
    }

    Vector2 RotateVector(Vector2 vec, float Angle)
    {
        //x2 = cos(A) * x1 - sin(A) * y1
        var newX = Mathf.Cos(Angle) * vec.x - Mathf.Sin(Angle) * vec.y;

        //y2 = sin(A) * x1 + cos(B) * y1;
        var newY = Mathf.Sin(Angle) * vec.x + Mathf.Cos(Angle) * vec.y;

        
        return new Vector2(newX, newY);
    }
}
