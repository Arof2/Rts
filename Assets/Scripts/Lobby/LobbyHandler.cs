using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Linq;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using ParrelSync;
#endif

public class LobbyHandler : NetworkBehaviour
{
    private static UnityTransport transport;
    private bool fetchingLobbys = false;
    private float nextRefresh = 0;
    public GameObject LobbyPrefab;
    public Transform lobbyHolder;
    private Lobby connectedLobby;
    public string playerId;
    public static LobbyHandler instance;

    [SerializeField] private GameObject mainMenuHolder;

    //Required Password Screen
    [SerializeField] private GameObject requiredPasswordHolder;
    [SerializeField] private CheckPassword checkPassword;

    //Lobby Fetching Screen
    [SerializeField] private GameObject lobbyFetchingHolder;

    //In Lobby Screen
    [SerializeField] private TextMeshProUGUI inLobbyName, inLobbyPassword, inLobbyinfoText;
    [SerializeField] private Toggle inLobbypasswordProtected;
    [SerializeField] private GameObject inLobbyplayerInLobbyPrefab, inLobbyHolder;
    [SerializeField] private Transform inLobbyPrefabHolder;
    private IEnumerator Countdown;

    //Host Lobby Screen
    [SerializeField] private GameObject hostLobbyHolder;
    [SerializeField] private TMP_InputField hostLobbyName, hostLobbyPassword;
    [SerializeField] private TextMeshProUGUI hostLobbyInfo;
    [SerializeField] private Toggle hostLobbyPasswordToggle;
    [SerializeField] private Button hostLobbyButton;

    private async void Awake()
    {
        if (instance == null)
            instance = this;

        await Authenticate();

        transport = FindObjectOfType<UnityTransport>();

        Countdown = StartGame();
    }

    private async Task Authenticate()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            var options = new InitializationOptions();

#if UNITY_EDITOR
        if (ClonesManager.IsClone())
            options.SetProfile(ClonesManager.GetArgument());
        else
            options.SetProfile("Primary");
#endif

            await UnityServices.InitializeAsync(options);

        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerId = AuthenticationService.Instance.PlayerId;
        }


    }

    public void EnableHostButton()
    {
        if (hostLobbyName.text != "")
        {
            if (hostLobbyPasswordToggle.isOn && hostLobbyPassword.text != "")
                hostLobbyButton.interactable = true;
            else if(!hostLobbyPasswordToggle.isOn)
                hostLobbyButton.interactable = true;
            else
                hostLobbyButton.interactable = false;
        }
        else
            hostLobbyButton.interactable = false;
    }

    public async void MakeLobby()
    {
        if(hostLobbyPasswordToggle.isOn)
            await CreateLobby(hostLobbyName.text, hostLobbyPasswordToggle.isOn, hostLobbyPassword.text);
        else
            await CreateLobby(hostLobbyName.text);
    }

    public void ToggleGameobject(GameObject G)
    {
        if (G.activeSelf)
            G.SetActive(false);
        else
            G.SetActive(true);
    }

    private async Task<Lobby> CreateLobby(string lobbyName, bool hasPassword = false, string password = "", int maxPlayers = 4)
    {
        try
        {
            SoundPlayer.instance.playSound("Gear 2");
            hostLobbyInfo.text = "Creating lobby";
            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joincode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "JoinCodeKey", new DataObject(DataObject.VisibilityOptions.Public, joincode) },
                    { "LobbyName", new DataObject(DataObject.VisibilityOptions.Public, lobbyName) },
                    { "PasswordProtected", new DataObject(DataObject.VisibilityOptions.Public, hasPassword ? "yes" : "no") },
                    { "Password", new DataObject(DataObject.VisibilityOptions.Public, password) }
                }
            };

            SoundPlayer.instance.playSound("Gear 1");
            var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            hostLobbyInfo.text = "Starting lobby";

            StartCoroutine(Heartbeat(lobby.Id, 15));

            transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
            NetworkManager.Singleton.StartHost();
            connectedLobby = lobby;

            prepareLobby(lobby);

            hostLobbyInfo.text = "";
            return lobby;
        }
        catch (Exception e)
        {
            Debug.Log(e);

            Debug.LogFormat("Failed creating a lobby");
            return null;
        }
    }

    private IEnumerator Heartbeat(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public async void GetLobbys()
    {
        try
        {
            nextRefresh = Time.time + 2;

            fetchingLobbys = true;

            var allLobbies = await GatherLobbies();

            foreach (Transform T in lobbyHolder)
            {
                if (T.gameObject != lobbyHolder.gameObject)
                    Destroy(T.gameObject);
            }

            foreach (var lobby in allLobbies)
            {
                GameObject G = Instantiate(LobbyPrefab, Vector3.zero, Quaternion.identity, lobbyHolder);
                LobbyInformation L = G.GetComponent<LobbyInformation>();
                L.joinButton.onClick.AddListener(StopFetchingLobbys);
                if (lobby.Data["PasswordProtected"].Value == "yes")
                    L.joinButton.onClick.AddListener(() => CheckPassword(lobby, L));
                else
                    L.joinButton.onClick.AddListener(() => JoinLobby(lobby, L));

                L.JoinCode = lobby.Data["JoinCodeKey"].Value;
                L.UpdateLobbyInformation(lobby.Data["LobbyName"].Value, lobby.Data["PasswordProtected"].Value == "yes" ? true : false, lobby.Data["Password"].Value, lobby.Players.Count);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Debug.Log("Couldnt Fetch lobbys");
        }
    }

    private void CheckPassword(Lobby Lby, LobbyInformation infos)
    {
        lobbyFetchingHolder.SetActive(false);
        requiredPasswordHolder.SetActive(true);
        checkPassword.currentLobby = Lby;
        checkPassword.infos = infos;

        ChangeBackgroundLobby.instance.ChangeTo(3);
    }

    public async void JoinLobby(Lobby Lby, LobbyInformation infos)
    {
        try
        {
            var a = await RelayService.Instance.JoinAllocationAsync(Lby.Data["JoinCodeKey"].Value);
            transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
            await LobbyService.Instance.JoinLobbyByIdAsync(Lby.Id);
            NetworkManager.Singleton.StartClient();
            connectedLobby = Lby;

            prepareLobby(Lby);

            
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Debug.Log("Couldnt join Lobby");
        }
    }


    public void Update()
    {
        if (fetchingLobbys && Time.time >= nextRefresh)
            GetLobbys();
    }

    IEnumerator StartGame()
    {
        ChangeinLobbyinfoTextServerRpc("Game Starts in 3");
        yield return new WaitForSeconds(1);
        ChangeinLobbyinfoTextServerRpc("Game Starts in 2");
        yield return new WaitForSeconds(1);
        ChangeinLobbyinfoTextServerRpc("Game Starts in 1");
        yield return new WaitForSeconds(1);
        ChangeinLobbyinfoTextServerRpc("Game Starts in 0");
        yield return new WaitForSeconds(1);
        ChangeSceneToGameServerRpc();
        ChangeinLobbyinfoTextServerRpc("Loading Game");
    }

    [ServerRpc]
    public void ChangeSceneToGameServerRpc()
    {
        NetworkManager.SceneManager.LoadScene("Game Scene", LoadSceneMode.Single);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeinLobbyinfoTextServerRpc(string text)
    {
        ChangeinLobbyinfoTextClientRpc(text);
    }

    [ClientRpc]
    private void ChangeinLobbyinfoTextClientRpc(string text)
    {
        inLobbyinfoText.text = text;
    }

    public void StopFetchingLobbys()
    {
        fetchingLobbys = false;
    }

    public static async Task<List<Lobby>> GatherLobbies()
    {
        //possible Filter Options
        var options = new QueryLobbiesOptions { };

        var allLobbies = await LobbyService.Instance.QueryLobbiesAsync(options);
        return allLobbies.Results;
    }

    public void ChangeReady()
    {
        foreach (GameObject G in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (G.GetComponent<Player>().owner)
            {
                G.GetComponent<Player>().ToggleReadyServerRpc();
            }

        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    public async void LeaveLobby()
    {
        if(connectedLobby != null)
        {
            try
            {
                if (Player.myPlayer.host)
                {
                    inLobbyinfoText.text = "Disconnecting Clients";
                    ShutDownLobbyClientRpc();

                    await checkForLobbyPlayers(); //wait until all players disconnect

                    inLobbyinfoText.text = "Closing Lobby";
                    connectedLobby = null;
                    mainMenuHolder.SetActive(true);
                    inLobbyHolder.gameObject.SetActive(false);
                    NetworkManager.Singleton.Shutdown();
                    StopAllCoroutines();
                }
                else
                {
                    inLobbyinfoText.text = "Leaving Lobby";
                    await LobbyService.Instance.RemovePlayerAsync(connectedLobby.Id, AuthenticationService.Instance.PlayerId);
                    connectedLobby = null;
                    NetworkManager.Singleton.Shutdown();
                    mainMenuHolder.gameObject.SetActive(true);
                    inLobbyHolder.gameObject.SetActive(false);

                    DestroyOneClientServerRpc(playerId);
                }
                inLobbyinfoText.text = "";
                foreach(Transform T in inLobbyPrefabHolder)
                {
                    if (T != inLobbyPrefabHolder)
                        Destroy(T.gameObject);
                }

                ChangeBackgroundLobby.instance.ChangeTo(0);
                SoundPlayer.instance.playSound("Main Menu Swipe");
            }
            catch(Exception e)
            {
                Debug.LogWarning("Couldnt Leave Lobby. Cause: " + e);
                if (Player.myPlayer.host)
                    inLobbyinfoText.text = "Couldnt Close Lobby";
                else
                    inLobbyinfoText.text = "Couldnt Leave Lobby";
            }
        }
    }

    [ClientRpc]
    public void ShutDownLobbyClientRpc()
    {
        if (!Player.myPlayer.host)
            LeaveLobby();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyOneClientServerRpc(string id)
    {
        DestroyOneClientClientRpc(id);
    }

    [ClientRpc]
    public void DestroyOneClientClientRpc(string id)
    {
        foreach (PlayerInLobbyInfo PI in FindObjectsOfType<PlayerInLobbyInfo>())
        {
            if (PI.myPlayer.playerId == id)
                Destroy(PI.gameObject);
        }
    }

    public async Task checkForLobbyPlayers()
    {
        while (FindObjectsOfType<Player>().Length > 1)
        {
            await Task.Delay(500);
        }
    }

    public void prepareLobby(Lobby lby)
    {
        //All possible JoinMenus
        lobbyFetchingHolder.SetActive(false);
        requiredPasswordHolder.SetActive(false);
        hostLobbyHolder.SetActive(false);

        inLobbyName.text = lby.Data["LobbyName"].Value;
        inLobbypasswordProtected.interactable = false;
        inLobbypasswordProtected.isOn = lby.Data["PasswordProtected"].Value == "yes" ? true : false;
        inLobbyPassword.text = lby.Data["Password"].Value;
        inLobbyHolder.SetActive(true);

        ChangeBackgroundLobby.instance.ChangeTo(4);
        SoundPlayer.instance.playSound("Main Menu Swipe");
    }

    public void ResetHostLobbyScreen()
    {
        hostLobbyInfo.text = "";
        hostLobbyPassword.text = "";
        hostLobbyName.text = "";
        hostLobbyPasswordToggle.isOn = false;
    }

    public void SpawnInAllPlayers()
    {
        foreach(Player P in FindObjectsOfType<Player>())
        {
            if(P != Player.myPlayer)
            {
                GameObject G = Instantiate(inLobbyplayerInLobbyPrefab, Vector3.zero, Quaternion.identity, inLobbyPrefabHolder);
                PlayerInLobbyInfo PI = G.GetComponent<PlayerInLobbyInfo>();
                PI.myPlayer = P;
                if (P.host)
                    PI.host.SetActive(true);
                if (P.owner)
                    PI.owner.SetActive(true);
                PI.Activate();
            }
        }

        SpawnInMyPlayerServerRpc(playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnInMyPlayerServerRpc(string Id)
    {
        SpawnInMyPlayerClientRpc(Id);
    }

    [ClientRpc]
    public void SpawnInMyPlayerClientRpc(string Id)
    {
        Player P = null;

        foreach (Player ppppp in FindObjectsOfType<Player>())
        {
            if (ppppp.playerId == Id)
                P = ppppp;
        }

        GameObject G = Instantiate(inLobbyplayerInLobbyPrefab, Vector3.zero, Quaternion.identity, inLobbyPrefabHolder);
        PlayerInLobbyInfo PI = G.GetComponent<PlayerInLobbyInfo>();
        PI.myPlayer = P;
        if (P.host)
            PI.host.SetActive(true);
        if (P.owner)
            PI.owner.SetActive(true);
        PI.Activate();
    }

    public void UpdatePlayerName(Player P)
    {
        foreach (PlayerInLobbyInfo PI in FindObjectsOfType<PlayerInLobbyInfo>())
        {
            if(PI.myPlayer == P)
                PI.inputName.text = P.PlayerName;
        }
    }

    public void GetReady()
    {
        ToggleReadyServerRpc(playerId, !Player.myPlayer.ready);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleReadyServerRpc(string id, bool state)
    {
        ToggleReadyClientRpc(id, state);
    }

    [ClientRpc]
    private void ToggleReadyClientRpc(string id, bool state)
    {
        foreach (PlayerInLobbyInfo PI in FindObjectsOfType<PlayerInLobbyInfo>())
        {
            if (PI.myPlayer.playerId == id)
            {
                PI.readyCheck.SetActive(state);
                PI.myPlayer.ready = state;

            } 
        }
        if(IsHost && CheckIfEveryoneIsReady())
        {
            StartCoroutine(Countdown);
        }
        else
        {
            StopCoroutine(Countdown);
            Countdown = StartGame();
            ChangeinLobbyinfoTextServerRpc("Waiting for all Players");
        }
    }

    public bool CheckIfEveryoneIsReady()
    {
        bool allready = true;
        foreach(Player P in FindObjectsOfType<Player>())
        {
            if(!P.ready)
            {
                allready = false;
                break;
            }
        }
        return allready;
    }
}
