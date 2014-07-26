using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelController : MonoBehaviour {

    public GameObject[] creepSpawnPoints;

    public Transform spawnPointRed;
    public Transform spawnPointBlue;

    public Transform redBase;
    public Transform blueBase;

    public GameObject redFlag;
    public GameObject blueFlag;

    public bool redFlagIsMissing = false;
    public bool blueFlagIsMissing = false;

    public float respawnTime = 5f;

    public Object[] heroPrefabs;

    private Dictionary<string, GameObject> heroes;

    private GameController gameController;
    private ScoreController scoreController;

	void Start ()
    {
        var gameControllerObj = GameObject.FindWithTag("GameController");
        if (gameControllerObj == null)
        {
            Application.LoadLevel(0);
            return;
        }
        gameController = gameControllerObj.GetComponent<GameController>();
        scoreController = GameObject.FindWithTag("ScoreController").GetComponent<ScoreController>();

        heroes = new Dictionary<string, GameObject>();
        creepSpawnPoints = GameObject.FindGameObjectsWithTag("CreepSpawner");

        var minimap = GameObject.FindWithTag("Minimap").GetComponent<MinimapController>();
        minimap.Track(redFlag.transform, MinimapIconType.RedFlag);
        minimap.Track(blueFlag.transform, MinimapIconType.BlueFlag);
	}
	
    [RPC]
    public void ReturnRedFlagToBase()
    {
        redFlag.transform.position = redBase.position;
    }

    [RPC]
    public void ReturnBlueFlagToBase()
    {
        blueFlag.transform.position = blueBase.position;
    }

    public void ScheduleForRespawn(NetworkPlayer player)
    {
        // var heroCtrl = heroes[player.guid].GetComponent<HeroController>();
        // heroCtrl.respawnTimer = respawnTime;
    }

    public void StartLevel()
    {
        for (int i = 0; i < creepSpawnPoints.Length; i++)
        {
            var spawner = creepSpawnPoints[i].GetComponent<CreepSpawnPoint>();
            var creep = (GameObject)Network.Instantiate(spawner.creepPrefab, spawner.transform.position, spawner.transform.rotation, 1);
            var creepNetworkView = creep.GetComponent<NetworkView>();
            creepNetworkView.RPC("SetName", RPCMode.AllBuffered, creep.name + i);
        }

        foreach (PlayerData player in gameController.Players)
        {
            var spawnPoint = spawnPointBlue.position;

            if (player.team == Team.Red)
                spawnPoint = spawnPointRed.position;

            var hero = (GameObject)Network.Instantiate(heroPrefabs[(int)player.hero], spawnPoint, Quaternion.identity, 1);

            var heroNetworkView = hero.GetComponent<NetworkView>();

            heroNetworkView.RPC("SetName", RPCMode.AllBuffered, player.name);
            heroNetworkView.RPC("SetTeam", RPCMode.AllBuffered, (int)player.team);
            heroNetworkView.RPC("SetOwner", RPCMode.AllBuffered, player.player);
        }
    }
}
