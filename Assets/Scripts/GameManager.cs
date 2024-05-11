using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

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
        return instance;
    }
}
