using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelController : MonoBehaviour {

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

	void Start () {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        scoreController = GameObject.FindWithTag("ScoreController").GetComponent<ScoreController>();
        heroes = new Dictionary<string, GameObject>();
	}
	
	void Update () {
        // if (Network.isServer)
        // {
        //     foreach (GameObject hero in heroes.Values)
        //     {
        //         var heroCtrl = hero.GetComponent<HeroController>();

        //         if (heroCtrl.dead)
        //         {
        //             heroCtrl.respawnTimer -= Time.deltaTime;
                    
        //             if (heroCtrl.respawnTimer <= 0f)
        //             {
        //                 Vector3 spawnPoint = Vector3.zero;
        //                 if (heroCtrl.team == Team.Red)
        //                     spawnPoint = spawnPointRed.position;
        //                 else
        //                     spawnPoint = spawnPointBlue.position;

        //                 hero.networkView.RPC("Respawn", RPCMode.AllBuffered, spawnPoint, Quaternion.identity);
        //             }
        //         }
        //     }
        // }
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
        foreach (PlayerData player in gameController.Players)
        {
            var spawnPoint = spawnPointBlue.position;

            if (player.team == Team.Red)
                spawnPoint = spawnPointRed.position;

            var hero = (GameObject)Network.Instantiate(heroPrefabs[(int)player.hero], spawnPoint, Quaternion.identity, 1);

            var heroNetworkView = hero.GetComponent<NetworkView>();
            heroNetworkView.RPC("SetTeam", RPCMode.AllBuffered, (int)player.team);
            heroNetworkView.RPC("SetOwner", RPCMode.AllBuffered, player.player);
        }
    }
}
