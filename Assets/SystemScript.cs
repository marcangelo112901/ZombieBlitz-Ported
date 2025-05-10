using System.Collections.Generic;
using UnityEngine;

public class SystemScript : MonoBehaviour
{
    public static SystemScript Instance { private set; get; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public List<GameObject> players;
}
