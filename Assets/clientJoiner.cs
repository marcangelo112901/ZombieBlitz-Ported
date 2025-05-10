using Unity.Netcode;
using UnityEngine;

public class clientJoiner : MonoBehaviour
{
    private NetworkManager networkManager;
    private void Start()
    {
        networkManager = NetworkManager.Singleton;
    }

    public void JoinClient()
    {
        networkManager.StartClient();
    }

    public void StartHost()
    {
        networkManager.StartHost();
    }
}
