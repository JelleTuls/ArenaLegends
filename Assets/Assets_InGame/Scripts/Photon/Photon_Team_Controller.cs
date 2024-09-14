using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts; // For PhotonTeamsManager
using Photon.Realtime; // For PhotonTeam
using UnityEngine.UI; // For UI Text
using TMPro; // For TextMeshPro if using TextMeshPro

public class Photon_Team_Controller : MonoBehaviourPunCallbacks
{
    // UI reference to display the player's team
    public TextMeshProUGUI teamText; // Use Text if you don't have TextMeshPro, replace with Text

    private void Start()
    {
        // Check the local player's team when the game starts
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("TeamCode", out object teamCodeObject))
        {
            byte teamCode = (byte)teamCodeObject;

            // Retrieve the team using PhotonTeamsManager
            PhotonTeam playerTeam;
            if (PhotonTeamsManager.Instance.TryGetTeamByCode(teamCode, out playerTeam))
            {
                Debug.Log($"Player is on team: {playerTeam.Name}");

                // Display the team information on the UI
                UpdateTeamText(playerTeam.Name);

                // Perform any team-specific setup (optional)
                SetupPlayerForTeam(playerTeam);
            }
        }
    }

    private void UpdateTeamText(string teamName)
    {
        // Set the UI text to display the player's team
        if (teamText != null)
        {
            teamText.text = "Your Team: " + teamName;
        }
    }

    private void SetupPlayerForTeam(PhotonTeam team)
    {
        // Example of team-specific logic (optional)
        if (team.Name == "Team 1")
        {
            // Set player color or spawn them in a specific location
        }
        else if (team.Name == "Team 2")
        {
            // Set player color or spawn them in another location
        }
    }

    public void CheckAllPlayersTeams()
    {
        foreach (Player otherPlayer in PhotonNetwork.PlayerListOthers)
        {
            if (otherPlayer.CustomProperties.TryGetValue("TeamCode", out object otherTeamCode))
            {
                byte teamCode = (byte)otherTeamCode;
                PhotonTeam otherPlayerTeam;
                if (PhotonTeamsManager.Instance.TryGetTeamByCode(teamCode, out otherPlayerTeam))
                {
                    Debug.Log($"{otherPlayer.NickName} is on team: {otherPlayerTeam.Name}");

                    // Here you can decide if they are allies or enemies
                }
            }
        }
    }
}
