using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinish : MonoBehaviour
{
    public float delayAfterLevelCompleted = 1f; // Adjust this value as needed
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.tag=="Player")
        {
            Debug.Log("Level Finished");
            GameManager.instance.LevelCompleted();
            
            bool LoadSceneinit = false;

            if(!LoadSceneinit)
            {
                StartCoroutine(LoadNextLevelWithDelay());
                LoadSceneinit = true;
            }
                      
        }
    }

    IEnumerator LoadNextLevelWithDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delayAfterLevelCompleted);

        // Get the index of the current scene
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Load the next scene (assuming scenes are ordered in the build settings)
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
}
