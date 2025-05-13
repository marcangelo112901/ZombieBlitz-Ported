using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;

public class MainMenuScript : NetworkBehaviour
{
    private Animation anim;

    [Header("Loading Screen Parameters")]
    [SerializeField] private BrokenProgressBar progressBar;
    [SerializeField] private TextMeshProUGUI loadingTMP;
    [SerializeField] private GameObject inviButton; 
    public bool multiplayerMode = false;

    [SerializeField] private GameObject SelectMultiplayer;
    [SerializeField] private Transform PlayerList;
    [SerializeField] private GameObject playerDataPrefab;
    [SerializeField] private TextMeshProUGUI IPAddressTMP;

    [SerializeField] private TMP_InputField playerNameIF;

    [SerializeField] private GameObject Lobby;
    [SerializeField] private GameObject LobbyStartButton;

    [SerializeField] private GameObject EnterIPUI;
    [SerializeField] private TMP_InputField IPAddressIF;


    private NetworkManager network;

    private AsyncOperation asyncOp;

    


    private void Awake()
    {
        anim = GetComponent<Animation>();
    }

    private void Start()
    {
        network = NetworkManager.Singleton;
    }

    private void Update()
    {
        if (asyncOp != null)
            progressBar.value = asyncOp.progress + 0.1f;
    }


    public void StartSinglePlayer()
    {
        multiplayerMode = false;
        network.StartHost();
        anim.Play("Loading Animation");
        StartCoroutine(SingleplayerAsync());
    }

    public void StartMultiplayer()
    {
        network.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        anim.Play("Loading Animation");
    }

    private IEnumerator SingleplayerAsync()
    {
        asyncOp = SceneManager.LoadSceneAsync("GameScene");
        asyncOp.allowSceneActivation = false;

        while (asyncOp.progress < 0.9f)
        {
            yield return null;
        }

        progressBar.gameObject.SetActive(false);
        loadingTMP.text = "Click To Start";
        inviButton.SetActive(true);
    }

    public void StartHost()
    {
        if (playerNameIF.text == "") return;
        multiplayerMode = true;
        SelectMultiplayer.SetActive(false);
        network.StartHost();
    }

    public void StartClient()
    {
        multiplayerMode = true;
        UnityTransport transport = network.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = IPAddressIF.text;
        network.StartClient();
    }

    public void OpenClientJoiner()
    {
        if (playerNameIF.text == "") return;
        SelectMultiplayer.SetActive(false);
        EnterIPUI.SetActive(true);
    }

    public void LoadSinglePlayer()
    {
        asyncOp.allowSceneActivation = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!multiplayerMode) return;
        network.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
        if (IsServer)
        {
            network.OnClientDisconnectCallback += OnClientDisconnected;
            IPAddressTMP.text = "Host IP: " + GetLocalIPAddress();
        }

        if (IsHost) LobbyStartButton.SetActive(true);
        else if (IsClient) LobbyStartButton.SetActive(false);

        if (!IsClient) return;

        Lobby.SetActive(true);
        EnterIPUI.SetActive(false);
        SendPlayerNameServerRpc(playerNameIF.text);
    }

    public void CloseLobby()
    {
        network.Shutdown();
        Lobby.SetActive(false);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        network.SceneManager.OnSceneEvent -= SceneManager_OnSceneEvent;
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        switch(sceneEvent.SceneEventType)
        {
            case SceneEventType.Load:
                asyncOp = sceneEvent.AsyncOperation;
                break;

            //case SceneEventType.LoadEventCompleted:

            //    if (IsClient)
            //        SceneManager.UnloadSceneAsync("MainMenuScene");
            //    break;


        }
    }

    public void OnClientDisconnected(ulong clientId)
    {
        if (PlayerDataScript.Instance.playerDictionary.ContainsKey(clientId))
        {
            PlayerDataScript.Instance.playerDictionary.Remove(clientId);

            string jsonPlayerNames = JsonConvert.SerializeObject(PlayerDataScript.Instance.playerDictionary.Values);
            UpdateClientListClientRpc(jsonPlayerNames);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPlayerNameServerRpc(string playerName, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        if (!PlayerDataScript.Instance.playerDictionary.ContainsKey(senderId))
            PlayerDataScript.Instance.playerDictionary.Add(senderId, playerName);

        string jsonPlayerNames = JsonConvert.SerializeObject(PlayerDataScript.Instance.playerDictionary.Values);
        UpdateClientListClientRpc(jsonPlayerNames);
    }

    [ClientRpc]
    private void UpdateClientListClientRpc(string jsonPlayerNames)
    {
        List<string> playerNames = JsonConvert.DeserializeObject<List<string>>(jsonPlayerNames);

        foreach (Transform child in PlayerList)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject playerData = Instantiate(this.playerDataPrefab, PlayerList);
            if (playerData.transform.GetChild(0).TryGetComponent(out TextMeshProUGUI textTMP))
            {
                textTMP.text = "Player " + (i + 1) + ": " + playerNames[i];
            }
        }
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString(); // IPv4 address
            }
        }
        return "127.0.0.1"; // fallback
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
