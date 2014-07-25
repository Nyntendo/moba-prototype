using UnityEngine;
using System.Collections;

public class ScoreController : MonoBehaviour {

    private int redTeamScore = 0;
    private int blueTeamScore = 0;

    public void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 30), string.Format("Red Team: {0} - Blue Team: {1}", redTeamScore, blueTeamScore));
    }

    [RPC]
    public void Score(int team)
    {
        if ((Team)team == Team.Red)
            redTeamScore++;
        else
            blueTeamScore++;
    }
}
