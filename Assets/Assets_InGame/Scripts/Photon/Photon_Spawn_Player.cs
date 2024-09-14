using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class Photon_Spawn_Player : MonoBehaviour
{
    // Array to hold references to the different player prefabs
    public GameObject[] playerPrefabs;

    private void Awake()
    {
        Debug.Log("SpawnPlayers script instantiated.");
    }

    private void Start()
    {
        Debug.Log("SpawnPlayers Start method called.");
        Debug.Log($"Photon Network connection state: {PhotonNetwork.NetworkClientState}");

        // Check if we are connected to a room
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("We are in a Photon room.");
        }
        else
        {
            Debug.LogError("Not in a Photon room.");
            return;
        }

        // Log all custom properties for debugging
        Debug.Log("Listing all custom properties:");
        foreach (var prop in PhotonNetwork.LocalPlayer.CustomProperties)
        {
            Debug.Log($"Property: {prop.Key} = {prop.Value}");
        }

        // Retrieve the selected character index from Photon custom properties
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CSN", out object selectedIndexObject))
        {
            int selectedIndex = (int)selectedIndexObject;
            Debug.Log($"Character selection index found: {selectedIndex}");

            // Ensure the index is within the bounds of the array
            if (selectedIndex >= 0 && selectedIndex < playerPrefabs.Length)
            {
                Vector3 spawnPosition = new Vector3(0f, 0f, 0f);
                PhotonNetwork.Instantiate(playerPrefabs[selectedIndex].name, spawnPosition, Quaternion.identity);
                Debug.Log($"Instantiated player prefab: {playerPrefabs[selectedIndex].name}");
            }
            else
            {
                Debug.LogError("Selected index is out of bounds of the playerPrefabs array.");
            }
        }
        else
        {
            Debug.LogError("Character selection index not found in custom properties.");
        }
    }

}
