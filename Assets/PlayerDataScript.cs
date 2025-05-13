using System.Collections.Generic;
using UnityEngine;

public class PlayerDataScript : MonoBehaviour
{
    public static PlayerDataScript Instance { private set; get; }
    public SortedDictionary<ulong, string> playerDictionary = new SortedDictionary<ulong, string>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Instance.playerDictionary.Clear();
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
