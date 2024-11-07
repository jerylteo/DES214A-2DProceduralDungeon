using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTargetLogic : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color hitColor = new Color(0.435f, 0.820f, 0.710f);
    public Color initialColor = new Color(0.953f,0.804f, 0f);
    public PlayerLevelGenerator playerLevelGenerator;

    public bool isShot = false;

    public float ResetTimer = 3f;
    private float CurrentResetTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = initialColor;
    }

    void Update() {
        if (isShot) {
            OnShot();
        }
    }

    public void OnShot()
    {
        if (!playerLevelGenerator.CheckAllTargetsHit()) {
            spriteRenderer.color = hitColor;

            int index = playerLevelGenerator.specialTargets.FindIndex(x => x.Target == gameObject);
            // Debug.Log(gameObject);
            if (index != -1) {
                // Debug.Log($"Special target {gameObject.name} was found in the list of special targets!");
                PlayerLevelGenerator.SpecialTarget updatedTarget = playerLevelGenerator.specialTargets[index];
                updatedTarget.IsShot = true;
                playerLevelGenerator.specialTargets[index] = updatedTarget;

                CurrentResetTimer += Time.deltaTime;
                if (CurrentResetTimer >= ResetTimer) {
                    Debug.Log($"Special target {gameObject.name} was reset!");
                    isShot = false;
                    spriteRenderer.color = initialColor;
                    updatedTarget.IsShot = false;
                    playerLevelGenerator.specialTargets[index] = updatedTarget;
                    CurrentResetTimer = 0f;
                }

                // Debug.Log($"Current reset timer: {CurrentResetTimer}");
                
            }
        }
        else {
            Destroy(this, 3);
        }
    }
}
