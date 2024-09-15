using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic; // Required for KeyValuePair
using Photon.Pun.UtilityScripts; // Required for PhotonTeam extensions
using TMPro;

public class Photon_Scene_Logging : MonoBehaviour
{
    //_____________________________________________________________________________________________________________________
    // VARIABLES:
    //---------------------------------------------------------------------------------------------------------------------
    public float debugInterval = 10.0f; // Time interval between each player list update (in seconds)
    private TMP_Text roomNameText; // Reference to the TextMeshPro UI element to display the current room name

    //_____________________________________________________________________________________________________________________
    // VOID START:
    //---------------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        // Start the coroutine to periodically log player information
        StartCoroutine(DebugPlayerListPeriodically());
    }

    //_____________________________________________________________________________________________________________________
    // FUNCTIONS:
    //---------------------------------------------------------------------------------------------------------------------
    // Function to debug and log the player list in the current room
    private void DebugPlayerList()
    {
        if (PhotonNetwork.InRoom)
        {
            string playersInfo = "Players in the room:\n";
            foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
            {
                Player player = playerInfo.Value;
                string playerTeam = "No Team";

                // Check if the player is on a PhotonTeam (if using teams)
                if (player.GetPhotonTeam() != null)
                {
                    playerTeam = player.GetPhotonTeam().Name;
                }

                // Append player info to the log message
                playersInfo += $"Player Name: {player.NickName}, Player ID: {player.ActorNumber}, Team: {playerTeam}\n";
            }

            // Output the player list to the console log
            Debug.Log(playersInfo);
        }
        else
        {
            Debug.Log("Not connected to any room.");
        }
    }

    // Function to search the entire hierarchy for the local player's Main_UI_Canvas and its Room_Display child
    private void FindRoomDisplayInHierarchy()
    {
        // Search for the Main_UI_Canvas in the scene
        GameObject uiCanvas = GameObject.Find("Main_UI_Canvas");

        if (uiCanvas != null)
        {
            Debug.Log("Main_UI_Canvas found.");

            // Now get the PhotonView from the Main_UI_Canvas
            PhotonView canvasPhotonView = uiCanvas.GetComponent<PhotonView>();

            if (canvasPhotonView != null && canvasPhotonView.IsMine)
            {
                // This UI canvas belongs to the local player
                Debug.Log("Main_UI_Canvas belongs to the local player.");

                // Find the Room_Display child and assign its TMP_Text component
                TMP_Text foundText = uiCanvas.transform.Find("Room_Display")?.GetComponent<TMP_Text>();

                if (foundText != null)
                {
                    roomNameText = foundText;
                    Debug.Log("Room_Display found and TMP_Text component assigned.");
                }
                else
                {
                    Debug.LogError("Room_Display text object not found under Main_UI_Canvas.");
                }
            }
            else
            {
                Debug.LogWarning("Main_UI_Canvas does not belong to the local player.");
            }
        }
        else
        {
            Debug.LogError("Main_UI_Canvas not found in the scene.");
        }
    }

    // Function to update the room name text in the UI
    private void UpdateRoomName()
    {
        if (roomNameText != null)
        {
            if (PhotonNetwork.InRoom)
            {
                // Set the room name in the UI text
                roomNameText.text = $"Room: {PhotonNetwork.CurrentRoom.Name}";
                Debug.Log($"Updated Room Name Text: {PhotonNetwork.CurrentRoom.Name}");
            }
            else
            {
                // Set a default message when not in a room
                roomNameText.text = "Not connected to any room.";
                Debug.Log("Not connected to any room.");
            }
        }
        else
        {
            Debug.LogWarning("Room name text UI element is not assigned.");
        }
    }

    //_____________________________________________________________________________________________________________________
    // IENUMERATOR FUNCTIONS:
    //---------------------------------------------------------------------------------------------------------------------
    // Coroutine that logs player information at regular intervals
    private IEnumerator DebugPlayerListPeriodically()
    {
        while (true)
        {
            FindRoomDisplayInHierarchy();
            DebugPlayerList();
            UpdateRoomName();
            yield return new WaitForSeconds(debugInterval);
        }
    }
}
