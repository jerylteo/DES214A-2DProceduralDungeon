/*******************************************************************************
File:      BulletLogic.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is added to the bullet and controls all of its behavior,
    including how to handle when different objects are hit.

*******************************************************************************/
using UnityEngine;

public enum Teams { Player, Enemy }

public class BulletLogic : MonoBehaviour
{
    public Teams Team = Teams.Player;
    public int Power = 1;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (Team == Teams.Player && col.CompareTag("SpecialTarget"))
        {
            SpecialTargetLogic target = col.GetComponent<SpecialTargetLogic>();
            if (target != null) {
                target.isShot = true;
            }
        }

        if (col.isTrigger || col.tag == Team.ToString())
            return;

        Destroy(gameObject);
    }
}
