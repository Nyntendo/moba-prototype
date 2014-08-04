using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerStats
{
    public int kills = 0;
    public int deaths = 0;
    public int captures = 0;
    public int returns = 0;
    public string name;
    public Hero hero;
    public Team team;

    public PlayerStats(string name, Hero hero, Team team)
    {
        this.name = name;
        this.hero = hero;
        this.team = team;
    }
}

public class ScoreController : MonoBehaviour {

    private int redTeamScore = 0;
    private int blueTeamScore = 0;
    private bool showStats = false;
    private Dictionary<string, PlayerStats> stats;
    private GameController gameController;

    public void Start()
    {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        stats = new Dictionary<string, PlayerStats>();
        foreach (PlayerData player in gameController.Players)
        {
            stats[player.name] = new PlayerStats(player.name, player.hero, player.team);
        }
    }

    public void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 30), string.Format("Red Team: {0} - Blue Team: {1}", redTeamScore, blueTeamScore));
        if (showStats)
        {
            GUI.BeginGroup(new Rect(Screen.width/2 - 300, Screen.height/2 - 150, 600, 300));

            GUI.Box(new Rect(0, 0, 600, 300), "Match statistics");

            GUI.BeginGroup(new Rect(40, 30, 520, 40));
            GUI.Label(new Rect(0, 0, 100, 40), "Player");
            GUI.Label(new Rect(100, 0, 100, 40), "Kills");
            GUI.Label(new Rect(200, 0, 100, 40), "Deaths");
            GUI.Label(new Rect(300, 0, 100, 40), "Flag captures");
            GUI.Label(new Rect(400, 0, 100, 40), "Flag returns");
            GUI.EndGroup();

            var y = 2;

            GUI.Label(new Rect(40, (y++ * 30), 100, 40), "Red team");

            foreach (PlayerStats player in stats.Values.Where(p => p.team == Team.Red))
            {
                GUI.BeginGroup(new Rect(40, (y++ * 30), 520, 40));
                GUI.Label(new Rect(0, 0, 100, 40), string.Format("{0} ({1})", player.name, player.hero));
                GUI.Label(new Rect(100, 0, 100, 40), player.kills.ToString());
                GUI.Label(new Rect(200, 0, 100, 40), player.deaths.ToString());
                GUI.Label(new Rect(300, 0, 100, 40), player.captures.ToString());
                GUI.Label(new Rect(400, 0, 100, 40), player.returns.ToString());
                GUI.EndGroup();
            }

            GUI.Label(new Rect(40, (y++ * 30), 100, 40), "Blue team");

            foreach (PlayerStats player in stats.Values.Where(p => p.team == Team.Blue))
            {
                GUI.BeginGroup(new Rect(40, (y++ * 30), 520, 40));
                GUI.Label(new Rect(0, 0, 100, 40), string.Format("{0} ({1})", player.name, player.hero));
                GUI.Label(new Rect(100, 0, 100, 40), player.kills.ToString());
                GUI.Label(new Rect(200, 0, 100, 40), player.deaths.ToString());
                GUI.Label(new Rect(300, 0, 100, 40), player.captures.ToString());
                GUI.Label(new Rect(400, 0, 100, 40), player.returns.ToString());
                GUI.EndGroup();
            }

            GUI.EndGroup();
        }
    }

    public void Update()
    {
        if (Input.GetButton("Stats"))
        {
            showStats = true;
        }
        else
        {
            showStats = false;
        }
    }

    [RPC]
    public void AddCapture(string player)
    {
        stats[player].captures++;

        if (stats[player].team == Team.Red)
            redTeamScore++;
        else
            blueTeamScore++;
    }

    [RPC]
    public void AddReturn(string player)
    {
        stats[player].returns++;
    }

    [RPC]
    public void AddKill(string killer, string victim)
    {
        stats[killer].kills++;
        stats[victim].deaths++;
    }

    [RPC]
    public void AddDeath(string player)
    {
        stats[player].deaths++;
    }
}
