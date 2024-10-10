using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts; // For PhotonTeamsManager
using Photon.Realtime; // For PhotonTeam
using TMPro; // For TextMeshPro
using CJ;

namespace CJ
{
    public class Photon_Manager_Team : MonoBehaviourPunCallbacks
    {
        private PhotonView photonView;
        private GameObject photonManagerSpawn; // Store the reference to Photon_Manager_Spawn

        // Method to receive the reference to Photon_Manager_Spawn from another script
        public void SetPhotonManagerSpawn(GameObject managerSpawn)
        {
            photonManagerSpawn = managerSpawn;
            if (photonManagerSpawn != null)
            {
                Debug.Log("Photon_Manager_Spawn reference received.");
                photonView = photonManagerSpawn.GetComponent<PhotonView>();
                if (photonView != null && photonView.IsMine)
                {
                    SetupPlayerTeam();
                }
                else
                {
                    Debug.LogWarning("PhotonView is not mine or not found on Photon_Manager_Spawn.");
                }
            }
            else
            {
                Debug.LogError("Received Photon_Manager_Spawn reference is null.");
            }
        }

        // Function to set up the player's team (called after receiving the Photon_Manager_Spawn object)
        private void SetupPlayerTeam()
        {
            Debug.Log("Setting up player team after receiving Photon_Manager_Spawn.");
            Debug.Log("LISTING ALL CUSTOM PROPERTIES FOR THE LOCAL PLAYER:");
            foreach (var property in PhotonNetwork.LocalPlayer.CustomProperties)
            {
                Debug.Log($"Property Key: {property.Key}, Value: {property.Value}");
            }
            
            // Check the local player's team by the "_pt" property
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("_pt", out object teamCodeObject))
            {
                Debug.Log("Found _pt property.");

                int teamCode = 0;
                teamCode = (byte)teamCodeObject;

                // Simulate team assignment based on _pt value (1 or 2)
                string teamName = teamCode == 1 ? "Team 1" : teamCode == 2 ? "Team 2" : "No Team Found";
                Debug.Log($"Player is on team: {teamName}");
                
                // Call function to update UI or player settings based on team
                SetupPlayerForTeam(teamName);
            }
            else
            {
                Debug.LogWarning("_pt property not found for local player.");
                SetupPlayerForTeam("No Team Found"); // No team property found, set a default message
            }
        }

        private void SetupPlayerForTeam(string teamName)
        {
            if (photonManagerSpawn == null)
            {
                Debug.LogError("Photon_Manager_Spawn is not assigned.");
                return;
            }

            // Use the Photon_Manager_Spawn object as the local player reference
            GameObject localPlayer = photonManagerSpawn;

            if (localPlayer == null)
            {
                Debug.LogError("Local player object is NULL! Make sure the player prefab is properly instantiated.");
                return;
            }

            Debug.Log($"Setting up UI for player: {localPlayer.name}");

            // Get the Player_Handle_UI component
            Player_Handle_UI playerHandleUI = localPlayer.GetComponent<Player_Handle_UI>();
            if (playerHandleUI != null)
            {
                Debug.Log("Player_Handle_UI component found. Calling SetTeamText.");

                // Update the player's UI with the team name
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
                if (otherPlayer.CustomProperties.TryGetValue("_pt", out object otherTeamCode))
                {
                    int teamCode = 0;
                    teamCode = (byte)otherTeamCode;

                    string teamName = teamCode == 1 ? "Team 1" : teamCode == 2 ? "Team 2" : "No Team Found";
                    Debug.Log($"{otherPlayer.NickName} is on team: {teamName}");
                }
                else
                {
                    Debug.LogWarning($"_pt not found for {otherPlayer.NickName}");
                }
            }
        }
    }
}
