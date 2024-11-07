/*******************************************************************************
File:      CameraZoomAndRestart.cs
Author:    Victor Cecci
DP Email:  victor.cecci@digipen.edu
Date:      12/5/2018
Course:    CS186
Section:   Z

Description:
    This component is added to the camera prefab and allows the player to control
    the zoom and restart the level (used for help with grading).

*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraZoomAndRestart : MonoBehaviour
{
    public float MinSize = 5f;
    public float MaxSize = 50f;
    public float ZoomSpeed = .5f;
    public Vector3 CenterPosition;
    
    public Camera Cam;

    private bool isZoomedOut = false;
    private float targetSize;
    public Vector3 targetPos;

    private ObjectFollow objectFollow;
    private PlayerLevelGenerator playerLevelGenerator;

    // Start is called before the first frame update
    void Start()
    {
        Cam = GetComponent<Camera>();
        targetSize = MinSize;
        objectFollow = GetComponent<ObjectFollow>();
        playerLevelGenerator = GameObject.Find("Hero").GetComponent<PlayerLevelGenerator>();
        if (playerLevelGenerator == null) Debug.Log("idk");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.mouseScrollDelta.y > 0)
        {
            Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize - 1f, MinSize, MaxSize);
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.mouseScrollDelta.y < 0)
        {
            Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize + 1f, MinSize, MaxSize);
        }


        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.F2)) {
            List<PlayerLevelGenerator.RoomData> roomDatas = playerLevelGenerator.roomDatas;
            Vector3 lowerBound = roomDatas.Aggregate((min, x) => x.RoomParent.transform.position.y < min.RoomParent.transform.position.y ? x : min).RoomParent.transform.position;
            Vector3 upperBound = roomDatas.Aggregate((max, x) => x.RoomParent.transform.position.y > max.RoomParent.transform.position.y ? x : max).RoomParent.transform.position;

            int levelSize = (int)Vector3.Distance(lowerBound, upperBound);

            MaxSize = levelSize/2 + playerLevelGenerator.GridSize;
            CenterPosition = new Vector3((lowerBound.x + upperBound.x) / 2, (lowerBound.y + upperBound.y) / 2, 0);

            isZoomedOut = !isZoomedOut;
            if (isZoomedOut) {
                // Debug.Log($"Zooming out to {CenterPosition}");
                targetSize = MaxSize;
                objectFollow.isZoomedOut = true;
                objectFollow.targetPos = CenterPosition + objectFollow.Offset;
                // Debug.Log(CenterPosition);
            }
            else {
                targetSize = MinSize;
                objectFollow.isZoomedOut = false;
                // StartCoroutine(MoveToPosition(originalPosition));
            }
        }

        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, targetSize, Time.deltaTime * ZoomSpeed);
    }

}
