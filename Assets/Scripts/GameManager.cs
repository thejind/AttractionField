using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public delegate void PolarityChangedDelegate(EPolarity newPolarity);

    public delegate void LevelCompleteDelegate();
    
    public delegate void PauseDelegate();

    public event PolarityChangedDelegate OnPolarityChanged;

    public event LevelCompleteDelegate OnLevelCompleted;

    public event PauseDelegate OnPausePressed;

    EPolarity playerPolarity;

    int sceneIndex = 0;
    

    public void setPlayerPolarity(EPolarity newPolarity)
    {
        Debug.Log("Change Delegated");
        playerPolarity = newPolarity;
        OnPolarityChanged?.Invoke(playerPolarity);
    }

    public void LevelCompleted()
    {
        Debug.Log("Level Complete Game Manger ");
        OnLevelCompleted?.Invoke();
        
    }

    public int getSceneIndex()
    {
        return sceneIndex;
    }
    public void setSceneIndex(int newSceneIndex)
    {
        sceneIndex = newSceneIndex;
    }
    
    public void onPausePressed()
    {
        OnPausePressed?.Invoke();
    }

    public EPolarity getPLayerPolarity()
    {

        return playerPolarity;
    }

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
        {
            // If instance is null, set it to this instance
            instance = this;
            // Keep the GameManager object alive between scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If another instance already exists, destroy this one
            Destroy(gameObject);
        }
    }

    public static GameManager GetInstance()
    {
        // Static method to get the instance of the GameManager
        return GameManager.instance;
    }

    
}
