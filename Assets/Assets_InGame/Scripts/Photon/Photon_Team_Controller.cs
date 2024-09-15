using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts; // For PhotonTeamsManager
using Photon.Realtime; // For PhotonTeam
using TMPro; // For TextMeshPro
using CJ;

namespace CJ
{
    public class Photon_Team_Controller : MonoBehaviourPunCallbacks
    {
        private void Start()
        {
            Debug.Log("Photon_Team_Controller Start called");

            // Check if local player exists
            if (PhotonNetwork.LocalPlayer == null)
            {
                Debug.LogError("Local player is NULL!");
                return;
            }

            // Check the local player's team when the game starts
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("TeamCode", out object teamCodeObject))
            {
                byte teamCode = (byte)teamCodeObject;

                // Retrieve the team using PhotonTeamsManager
                if (PhotonTeamsManager.Instance.TryGetTeamByCode(teamCode, out PhotonTeam playerTeam))
                {
                    Debug.Log($"Player is on team: {playerTeam.Name}");

                    // Perform team-specific setup (e.g., set player color, spawn point, etc.)
                    SetupPlayerForTeam(playerTeam);
                }
                else
                {
                    Debug.LogWarning("Player's team could not be found in PhotonTeamsManager.");
                    SetupPlayerForTeam(null); // No team found, set a default message
                }
            }
            else
            {
                Debug.LogWarning("TeamCode property not found for local player.");
                SetupPlayerForTeam(null); // No team property found, set a default message
            }
        }

        private void SetupPlayerForTeam(PhotonTeam team)
        {
            // Get the local player object
            GameObject localPlayer = PhotonNetwork.LocalPlayer.TagObject as GameObject;

            if (localPlayer == null)
            {
                Debug.LogError("Local player object is NULL! Make sure the player prefab is properly instantiated and TagObject is set.");
                return;
            }

            Debug.Log($"Setting up UI for player: {localPlayer.name}");

            // Get the Player_Handle_UI component
            Player_Handle_UI playerHandleUI = localPlayer.GetComponent<Player_Handle_UI>();
            if (playerHandleUI != null)
            {
                // Log that the Player_Handle_UI was found and will call SetTeamText
                Debug.Log("Player_Handle_UI component found. Calling SetTeamText.");

                // If the team is not null, update the player's UI with the team name
                string teamName = team != null ? team.Name : "No Team Found";
                playerHandleUI.SetTeamText(teamName);
            }
            else
            {
                Debug.LogError("Player_Handle_UI component not found on local player.");
            }
        }

        public void CheckAllPlayersTeams()
        {
            foreach (Player otherPlayer in PhotonNetwork.PlayerListOthers)
            {
                if (otherPlayer.CustomProperties.TryGetValue("TeamCode", out object otherTeamCode))
                {
                    byte teamCode = (byte)otherTeamCode;
                    if (PhotonTeamsManager.Instance.TryGetTeamByCode(teamCode, out PhotonTeam otherPlayerTeam))
                    {
                        Debug.Log($"{otherPlayer.NickName} is on team: {otherPlayerTeam.Name}");

                        // Here you can decide if they are allies or enemies
                    }
                    else
                    {
                        Debug.LogWarning($"Team for {otherPlayer.NickName} could not be found.");
                    }
                }
                else
                {
                    Debug.LogWarning($"TeamCode not found for {otherPlayer.NickName}");
                }
            }
        }
    }
}
