using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public enum Team
{
    Red = 0,
    Blue = 1,
    Creep = 2
}

public enum Hero
{
    Putte = 0,
    Harrald = 1,
	RedRidingHood = 2,
	BigBadWolf = 3
}

public enum GameState
{
    SelectingName = 0,
    CreatingGame = 1,
    SelectingHero = 2,
    LoadingLevel = 3,
    PlayingGame = 4
}

public class PlayerData
{
    public string name;
    public NetworkPlayer player;
    public Hero hero;
    public Team team;
    public bool ready;
    public bool hasLoadedLevel;
}

public class GameController : MonoBehaviour
{ 
    const string registeredGameName = "MobaOfDeathAndDestruction_v0.0.1";
    private string gameName = "Game name";
    private List<HostData> hostList;
    private string ip = "127.0.0.1";
    private int port = 25002;
    private bool useNAT = false;

    private string playerName = "Sven";

    private Dictionary<string, PlayerData> players;

    public GameState currentGameState = GameState.SelectingName;
    private int level = 1;

    public IEnumerable<PlayerData> Players
    {
        get
        {
            return players.Values;
        }
    }

    public void Start()
    {
        DontDestroyOnLoad(gameObject);

        hostList = new List<HostData>();
        players = new Dictionary<string, PlayerData>();
    }

    public void OnGUI()
    {
        if (currentGameState == GameState.SelectingName)
        {
            // Set player name dialog
            GUI.BeginGroup(new Rect(Screen.width/2 - 150, Screen.height/2 - 150, 300, 300));

            GUI.Box(new Rect(0, 0, 300, 300), "Player name");

            GUI.Label(new Rect(50, 40, 100, 30), "Enter your name");
            playerName = GUI.TextField(new Rect(50, 80, 100, 30), playerName);
            if (GUI.Button(new Rect(175, 80, 75, 30), "Ok"))
            {
                currentGameState = GameState.CreatingGame;
            }
            GUI.EndGroup();
        }
        else if (currentGameState == GameState.CreatingGame)
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
        else if (currentGameState == GameState.SelectingHero)
        {

            GUI.BeginGroup(new Rect(Screen.width/2 - 150, Screen.height/2 - 150, 300, 300));

            GUI.Box(new Rect(0, 0, 300, 300), "Menu");

            if (Network.isServer)
            {
                if (GUI.Button(new Rect(50, 40, 200, 30), "Start"))
                {
                    StartGame();
                }
            }

            if (Network.isClient)
            {
                if (GUI.Button(new Rect(50, 40, 200, 30), "Ready"))
                {
                    networkView.RPC("SetPlayerReady", RPCMode.Server, Network.player.guid, true);
                }
            }

            GUI.Label(new Rect(25, 80, 100, 30), "Red Team");

            var redTeamIndex = 0;
            var blueTeamIndex = 0;
            foreach (PlayerData playerData in players.Values.Where(pd => pd.team == Team.Red))
            {
                var y = 120 + redTeamIndex * 40;
                GUI.Label(new Rect(25, y, 150, 30), string.Format("{0} ({1})", playerData.name, playerData.hero));
                redTeamIndex++;
            }

            GUI.Label(new Rect(175, 80, 100, 30), "Blue Team");
            foreach (PlayerData playerData in players.Values.Where(pd => pd.team == Team.Blue))
            {
                var y = 120 + blueTeamIndex * 40;
                GUI.Label(new Rect(175, y, 150, 30), string.Format("{0} ({1})", playerData.name, playerData.hero));
                blueTeamIndex++;
            }

            GUI.EndGroup();

            GUI.BeginGroup(new Rect(Screen.width/2 - 300, Screen.height/2 + 175, 600, 100));

            GUI.Box(new Rect(0, 0, 600, 100), "Choose Hero");

            for (int i = 0; i < Enum.GetValues(typeof(Hero)).Length; i++)
            {
                var x = 40 + 115 * i;
                if (GUI.Button(new Rect(x, 40, 100, 30), ((Hero)i).ToString()))
                {
                    if (Network.isClient)
                    {
                        networkView.RPC("SetPlayerHero", RPCMode.Server, Network.player.guid, i);
                    }
                    else if (Network.isServer)
                    {

                        SetPlayerHero(Network.player.guid, i);
                    }
                }
            }

            GUI.EndGroup();
        }
        else if (currentGameState == GameState.PlayingGame)
        {
            if (Network.isClient)
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
            }
        }
    }

    private void Reset()
    {
        players.Clear();
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
                    hostList.Add(hostData[i]); Debug.Log("Game: " + hostData[i].gameName + ", guid: " + hostData[i].guid);
                    i++;
                }

                MasterServer.ClearHostList();
            }
        }

        if (currentGameState == GameState.LoadingLevel && Network.isServer)
        {
            var allReady = true;
            foreach (PlayerData player in players.Values)
            {
                if (!player.hasLoadedLevel)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady)
            {
                networkView.RPC("SetGameState", RPCMode.AllBuffered, (int)GameState.PlayingGame);
                var levelController = GameObject.FindWithTag("LevelController").GetComponent<LevelController>();
                levelController.StartLevel();
            }
        }
    }

    public void OnServerInitialized()
    {
        Debug.Log("Server initialized!");
        // MasterServer.RegisterHost(registeredGameName, gameName, "5v5 Capture the Flag");

        players[Network.player.guid] = new PlayerData {
            name = playerName,
            player = Network.player,
            hero = Hero.Putte,
            team = Team.Red,
            ready = true,
            hasLoadedLevel = false
        };
        OnPlayerDataUpdated(Network.player.guid);
        currentGameState = GameState.SelectingHero;
    }

    private void OnPlayerDataUpdated(string guid)
    {
        networkView.RPC("PlayerDataUpdated", RPCMode.AllBuffered, guid, players[guid].name, players[guid].player, (int)players[guid].hero, (int)players[guid].team, players[guid].ready, players[guid].hasLoadedLevel);
    }

    public void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player connected!");

        var team = Team.Red;

        if (players.Count % 2 == 1)
        {
            team = Team.Blue;
        }

        players[player.guid] = new PlayerData {
            name = null,
            player = player,
            hero = Hero.Harrald,
            team = team,
            ready = false,
            hasLoadedLevel = false
        };
        OnPlayerDataUpdated(Network.player.guid);
    }

    [RPC]
    public void PlayerDataUpdated(string guid, string name, NetworkPlayer player, int hero, int team, bool ready, bool hasLoadedLevel)
    {
        players[guid] = new PlayerData {
            name = name,
            player = player,
            hero = (Hero)hero,
            team = (Team)team,
            ready = ready,
            hasLoadedLevel = hasLoadedLevel
            };
    }

    [RPC]
    public void SetPlayerName(string guid, string name)
    {
        players[guid].name = name;
        OnPlayerDataUpdated(guid);
    }

    [RPC]
    public void SetPlayerHero(string guid, int hero)
    {
        players[guid].hero = (Hero)hero;
        OnPlayerDataUpdated(guid);
    }

    [RPC]
    public void SetPlayerReady(string guid, bool ready)
    {
        players[guid].ready = ready;
        OnPlayerDataUpdated(guid);
    }

    [RPC]
    public void SetPlayerHasLoadedLevel(string guid, bool hasLoadedLevel)
    {
        Debug.Log(string.Format("Player: {0} sent loaded level: {1}", guid, hasLoadedLevel));
        players[guid].hasLoadedLevel = hasLoadedLevel;
        OnPlayerDataUpdated(guid);
    }

    [RPC]
    public void LoadLevel(int level)
    {
        currentGameState = GameState.LoadingLevel;
        this.level = level;
        Application.LoadLevel(level);
    }

    [RPC]
    public void SetGameState(int gameState)
    {
        currentGameState = (GameState)gameState;
    }

    private void StartGame()
    {
        foreach (PlayerData player in players.Values)
        {
            if (!player.ready)
                return;
        }
        networkView.RPC("LoadLevel", RPCMode.AllBuffered, level);
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

        networkView.RPC("SetPlayerName", RPCMode.Server, Network.player.guid, playerName);
        currentGameState = GameState.SelectingHero;
    }

    public void OnLevelWasLoaded(int level)
    {
        if (level == this.level)
        {
            if (Network.isClient)
                networkView.RPC("SetPlayerHasLoadedLevel", RPCMode.Server, Network.player.guid, true);
            else if (Network.isServer)
                SetPlayerHasLoadedLevel(Network.player.guid, true);
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

        Reset();
        currentGameState = GameState.CreatingGame;
    }

    public void OnPlayerDisconnected(NetworkPlayer player)
    {
        players.Remove(player.guid);
    }
}
