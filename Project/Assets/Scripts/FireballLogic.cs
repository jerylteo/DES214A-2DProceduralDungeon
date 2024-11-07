using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballLogic : MonoBehaviour
{

    public float fireballSpeed = 5.0f;
    public float changeDirectionDelay = 1.0f;

    private enum Directions { Up, Right, Down, Left };
    private Directions currentDirection = Directions.Down;
    private Vector3 intialPosition;
    private Vector3 targetPosition;
    private float directionChangeTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        intialPosition = transform.position;
        targetPosition = intialPosition + Vector3.down * 2;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, fireballSpeed * Time.deltaTime);

        if (transform.position == targetPosition) {
            directionChangeTimer += Time.deltaTime;

            if (directionChangeTimer >= changeDirectionDelay) {
                currentDirection = (Directions)(((int)currentDirection + 1) % 4);

                switch (currentDirection) {
                    case Directions.Up:
                        targetPosition = transform.position + Vector3.up * 2;
                        break;
                    case Directions.Down:
                        targetPosition = transform.position + Vector3.down * 2;
                        break;
                    case Directions.Left:
                        targetPosition = transform.position + Vector3.left * 2;
                        break;
                    case Directions.Right:
                        targetPosition = transform.position + Vector3.right * 2;
                        break;
                }

                directionChangeTimer = 0.0f;
            }
        }
    }
}
