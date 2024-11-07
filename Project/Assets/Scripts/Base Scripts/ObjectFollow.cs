/*******************************************************************************
File:      ObjectFollow.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2016
Course:    CS186
Section:   Z

Description:
    This component is added to any object to have it follow a specified target.
    It follows the target using linear interpolation on FixedUpdate.

*******************************************************************************/
using UnityEngine;

public class ObjectFollow : MonoBehaviour
{
    //Public Properties
    public Transform ObjectToFollow;
    public Vector3 Offset;
    public float Interpolant;

    public Vector3 targetPos;
    public bool isZoomedOut = false;


    void Start()
    {
        targetPos = ObjectToFollow.position + Offset;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (ObjectToFollow == null)
            return;

        //Lerp towards object every frame

        if (!isZoomedOut)
            targetPos = ObjectToFollow.position + Offset;


        // transform.position = Vector3.Lerp(transform.position, targetPos, Interpolant);
        if (ObjectToFollow.tag == "EnemyHealthBar") {
            transform.position = Vector3.Lerp(transform.position, targetPos, Interpolant);
        }
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * Interpolant);

        // float distanceToTarget = Vector3.Distance(transform.position, targetPos);
        // float lerpFactor = Mathf.Clamp01(distanceToTarget / MaxDistance);
        // transform.position = Vector3.Lerp(transform.position, targetPos, lerpFactor * FollowSpeed * Time.deltaTime);
            
    }
}
