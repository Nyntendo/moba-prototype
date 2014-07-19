using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum Team
{
    Red = 0,
    Blue = 1
}

enum NetworkGroups
{
    Server = 0,
    Player = 1
}

public class GameController : MonoBehaviour
{ 
    public Transform spawnPointRed;
    public Transform spawnPointBlue;
    public Object heroPrefab;

    public GameObject redFlag;
    public GameObject blueFlag;

    const string registeredGameName = "MobaOfDeathAndDestruction_v0.0.1";
    private string gameName = "Game name";
    private List<HostData> hostList;
    private string ip = "127.0.0.1";
    private int port = 25002;
    private bool useNAT = false;

    private List<NetworkPlayer> players;
    private Dictionary<string, GameObject> heroes;
    private List<string> redTeam;
    private List<string> blueTeam;
    private Dictionary<string, string> playerNames;

    private int redTeamScore = 0;
    private int blueTeamScore = 0;

    private bool gameStarted = false;

    public void Start()
    {
        hostList = new List<HostData>();
        players = new List<NetworkPlayer>();
        heroes = new Dictionary<string, GameObject>();
        redTeam = new List<string>();
        blueTeam = new List<string>();
    }

    public void OnGUI()
    {
        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            GUI.BeginGroup(new Rect(Screen.width/2 - 150, Screen.height/2 - 150, 300, 300));

            GUI.Box(new Rect(0, 0, 300, 300), "Menu");

            gameName = GUI.TextField(new Rect(50, 40, 100, 30), gameName);

            useNAT = GUI.Toggle(new Rect(175, 40, 75, 30), useNAT, "Use NAT");

            if (GUI.Button(new Rect(50, 80, 200, 30), "Create Server"))
            {
                Network.InitializeServer(32, port, useNAT);
            }

            ip = GUI.TextField(new Rect(50, 120, 100, 30), ip);
            if (GUI.Button(new Rect(175, 120, 75, 30), "Join"))
            {
                Network.Connect(ip, port);
            }

            if (GUI.Button(new Rect(50, 160, 200, 30), "Refresh"))
            {
                MasterServer.ClearHostList();
                MasterServer.RequestHostList(registeredGameName);
            }

            foreach (HostData host in hostList)
            {
                var y = 200 + hostList.IndexOf(host) * 40;
                GUI.Label(new Rect(50, y, 100, 30), host.gameName);
                if (GUI.Button(new Rect(175, y, 75, 30), "Join"))
                {
                    Network.Connect(host);
                }
            }

            GUI.EndGroup();
        }
        else if (Network.isClient)
        {
            GUI.Label(new Rect(10, 10, 300, 30), "Connected as client");
            if (GUI.Button(new Rect(10, 40, 200, 30), "Disconnect"))
            {
                Network.Disconnect(200);
            }
        }
        else if (Network.isServer)
        {
            GUI.Label(new Rect(10, 10, 300, 30), "Connected as server");
            if (GUI.Button(new Rect(10, 40, 200, 30), "Disconnect"))
            {
                Network.Disconnect(200);
                Reset();
            }

            if (!gameStarted)
            {
                GUI.BeginGroup(new Rect(Screen.width/2 - 150, Screen.height/2 - 150, 300, 300));

                GUI.Box(new Rect(0, 0, 300, 300), "Menu");

                if (GUI.Button(new Rect(50, 40, 200, 30), "Start"))
                {
                    StartGame();
                }

                GUI.Label(new Rect(25, 80, 100, 30), "Red Team");
                foreach (string guid in redTeam)
                {
                    var y = 120 + redTeam.IndexOf(guid) * 40;
                    GUI.Label(new Rect(25, y, 100, 30), guid);
                }

                GUI.Label(new Rect(175, 80, 100, 30), "Blue Team");
                foreach (string guid in blueTeam)
                {
                    var y = 120 + blueTeam.IndexOf(guid) * 40;
                    GUI.Label(new Rect(175, y, 100, 30), guid);
                }

                GUI.EndGroup();
            }
        }

        GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 30), string.Format("Red Team: {0} - Blue Team: {1}", redTeamScore, blueTeamScore));
    }

    private void Reset()
    {
        foreach (GameObject hero in heroes.Values)
        {
            var heroNetworkView = hero.GetComponent<NetworkView>();
            Network.RemoveRPCs(heroNetworkView.viewID);
            Network.Destroy(hero);
        }

        players.Clear();
        heroes.Clear();
        redTeam.Clear();
        blueTeam.Clear();
        redTeamScore = 0;
        blueTeamScore = 0;
        gameStarted = false;
        redFlag.SetActive(true);
        blueFlag.SetActive(true);
    }

    public void Update()
    {
        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            if (MasterServer.PollHostList().Length != 0)
            {
                hostList.Clear();
                var hostData = MasterServer.PollHostList();
                var i = 0;
                while (i < hostData.Length)
                {
                    hostList.Add(hostData[i]);
                    Debug.Log("Game: " + hostData[i].gameName + ", guid: " + hostData[i].guid);
                    i++;
                }

                MasterServer.ClearHostList();
            }
        }
    }

    public void OnServerInitialized()
    {
        Debug.Log("Server initialized!");
        MasterServer.RegisterHost(registeredGameName, gameName, "5v5 Capture the Flag");
        players.Add(Network.player);
        redTeam.Add(Network.player.guid);
        Spawn(Network.player);
    }

    public void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player connected!");
        players.Add(player);

        if (players.Count % 2 == 0)
        {
            blueTeam.Add(player.guid);
        }
        else
        {
            redTeam.Add(player.guid);
        }
    }

    [RPC]
    public void Score(int team)
    {
        if ((Team)team == Team.Red)
            redTeamScore++;
        else
            blueTeamScore++;
    }

    [RPC]
    public void Spawn(NetworkPlayer player)
    {
        if (!heroes.ContainsKey(player.guid) && !gameStarted)
        {
            Vector3 spawnPoint = Vector3.zero;
            Team team;
            if (redTeam.Contains(player.guid))
            {
                spawnPoint = spawnPointRed.position;
                team = Team.Red;
            }
            else
            {
                spawnPoint = spawnPointBlue.position;
                team = Team.Blue;
            }
            var hero = (GameObject)Network.Instantiate(heroPrefab, spawnPoint, Quaternion.identity, (int)NetworkGroups.Player);
            heroes.Add(player.guid, hero);
            var heroNetworkView = hero.GetComponent<NetworkView>();
            heroNetworkView.RPC("SetTeam", RPCMode.AllBuffered, (int)team);
        }
    }

    private void StartGame()
    {
        foreach (NetworkPlayer player in players)
        {
            if (!heroes.ContainsKey(player.guid))
            {
                return;
            }
        }

        foreach (KeyValuePair<string, GameObject> pair in heroes) 
        {
            var heroNetworkView =  pair.Value.GetComponent<NetworkView>();
            var player = players.First(p => p.guid == pair.Key);
            heroNetworkView.RPC("SetOwner", RPCMode.AllBuffered, player);
        }

        gameStarted = true;
    }

    public void OnMasterServerEvent(MasterServerEvent evt)
    {
        if (evt == MasterServerEvent.RegistrationSucceeded)
        {
            Debug.Log("Server registered!");
        }
    }

    public void OnConnectedToServer()
    {
        Debug.Log("Connected to server...");
        Network.isMessageQueueRunning = false;
        Application.LoadLevel(0);
    }

    public void OnLevelWasLoaded(int level)
    {
        if (level == 0 && Network.isClient)
        {
            Network.isMessageQueueRunning = true;
            networkView.RPC("Spawn", RPCMode.Server, Network.player);
        }
    }

    public void OnFailedToConnect(NetworkConnectionError error)
    {
        Debug.Log("Failed to connect to server, " + error);
    }

    public void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        if (Network.isServer)
        {
            Debug.Log("Local server connection disconnected");
        }
        else
        {
            if (info == NetworkDisconnection.LostConnection)
                Debug.Log("Lost connection to the server");
            else
                Debug.Log("Successfully diconnected from the server");
        }
    }

    public void OnPlayerDisconnected(NetworkPlayer player)
    {
        var hero = heroes[player.guid];
        if (hero != null)
        {
            var heroNetworkView = hero.GetComponent<NetworkView>();
            Network.RemoveRPCs(heroNetworkView.viewID);
            Network.Destroy(hero);
        }

        heroes.Remove(player.guid);
        players.Remove(player);
    }
}
