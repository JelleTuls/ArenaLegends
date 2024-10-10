using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime; // For Player class
using ExitGames.Client.Photon; // For Photon Hashtable
using System.Collections;

namespace CJ
{
    public class Photon_Manager_Room : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
    {
//_____________________________________________________________________________________________________________________
// VARIABLES
//---------------------------------------------------------------------------------------------------------------------
        // GAME OBJECTS
        public GameObject[] playerPrefabs; // Array to store different player prefabs
        private GameObject playerInstance; // Reference to the local player's instantiated object

        // BOOLEANS
        private bool hasSpawnedPlayer = false; // To prevent multiple spawns

        // FLOATS
        private const float checkInterval = 1.0f; // Time interval between checks
        private const float totalCheckTime = 6f;  // Total time to perform checks

//_____________________________________________________________________________________________________________________
// START FUNCTION
//---------------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            Debug.Log("PLAY FUNCTION: Photon_Manager_Room/Start()");
            // INVOKE: Mark that this player has loaded the scene
            SetPlayerLoadedProperty();
        }

//_____________________________________________________________________________________________________________________
// CHECK LOADED FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------
        // Sets a custom property indicating that the local player has loaded the scene
        private void SetPlayerLoadedProperty()
        {
            Debug.Log("PLAY FUNCTION: Photon_Manager_Room/SetPlayerLoadedProperty()");
            // Create new Hashtable "PLAYER_LOADED"
            ExitGames.Client.Photon.Hashtable playerLoadedProperty = new ExitGames.Client.Photon.Hashtable
            {
                { "PLAYER_LOADED", true }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerLoadedProperty);

            // INVOKE: Check if all players have loaded the scene
            CheckAllPlayersLoaded();
        }


        // Every second: checks if all players have loaded the scene
        private void CheckAllPlayersLoaded()
        {
            Debug.Log("PLAY FUNCTION: Photon_Manager_Room/CheckAllPlayersLoaded()");
            // Wait for all players to have "PLAYER_LOADED" set to true
            if (PhotonNetwork.PlayerList.All(player => player.CustomProperties.TryGetValue("PLAYER_LOADED", out object loaded) && (bool)loaded))
            {
                Debug.Log("All players have loaded the scene.");
                // INVOKE: Instantiate Player on the PhotonNetwork
                SpawnPlayerObject();
            }
            else
            {
                Debug.Log("Waiting for all players to load the scene...");
                // Try again after 1 second
                Invoke(nameof(CheckAllPlayersLoaded), 1f); // Re-check every 1 second
            }
        }


//_____________________________________________________________________________________________________________________
// SPAWN PLAYER FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------
        // Spawns the player object for the local player on the PhotonNetwork
        private void SpawnPlayerObject()
        {
            Debug.Log("PLAY FUNCTION: Photon_Manager_Room/SpawnPlayerObject()");
            if (hasSpawnedPlayer) return; // Prevent multiple spawns

            // Get CSN (Character Selection Number) out of the CustomProperties
            // Assign the selected character to selectedIndexObject
            PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CSN", out object selectedIndexObject);

            if (selectedIndexObject != null)
            {
                int selectedIndex = (int)selectedIndexObject;

                // Instantiate the correct player prefab for the selected index with custom instantiation data
                object[] instantiationData = { PhotonNetwork.LocalPlayer.NickName }; // Add any data you want to pass to OnPhotonInstantiate
                playerInstance = PhotonNetwork.Instantiate(playerPrefabs[selectedIndex].name, Vector3.zero, Quaternion.identity, 0, instantiationData);
                hasSpawnedPlayer = true; // Mark that the player object has been spawned

                // Start the check to ensure all players have their objects instantiated, only if there are other players in the room
                if (PhotonNetwork.PlayerList.Length > 1)
                {
                    //StartCoroutine(VerifyAllPlayerObjects());
                }
                else
                {
                    Debug.Log("Single player in the room, no need to verify other players' objects.");
                }
            }
            else
            {
                Debug.LogError("CSN property not found for the local player. Cannot spawn player object.");
            }
        }


        // // Destroys all local player's objects that have PhotonView.IsMine
        // private void DestroyAllLocalObjects()
        // {
        //     Debug.Log("PLAY FUNCTION: DestroyAllLocalObjects()");
        //     var allObjects = FindObjectsOfType<PhotonView>();
        //     foreach (var obj in allObjects)
        //     {
        //         if (obj.IsMine)
        //         {
        //             Debug.Log($"Destroying object: {obj.gameObject.name}");
        //             PhotonNetwork.Destroy(obj.gameObject);
        //         }
        //     }
        // }


        // Function which plays the moment a player got instantiated
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            Debug.Log("PLAY FUNCTION: Photon_Manager_Room/OnPhotonInstantiate()");
            if (info.photonView.InstantiationData != null && info.photonView.InstantiationData.Length > 0) // Only if there is InstantiationData
            {
                string playerName = (string)info.photonView.InstantiationData[0];
                Debug.Log($"PLPAYER INSTANTIATED : {playerName}");

                // Rename instantiated object
                info.photonView.gameObject.name = $"Player_Object_{playerName}";
            }
        }


//_____________________________________________________________________________________________________________________
// VERIFICATION FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------
        // // Coroutine to verify that all players have their objects instantiated across all clients
        // private IEnumerator VerifyAllPlayerObjects()
        // {
        //     Debug.Log("PLAY FUNCTION: Photon_Manager_Room/VerifyAllPlayerObjects()");
        //     float elapsedTime = 0f;

        //     // Check for a total of 5 seconds, every 0.5 seconds
        //     while (elapsedTime < totalCheckTime)
        //     {
        //         // Wait for 0.5 seconds between each check
        //         yield return new WaitForSeconds(checkInterval);
        //         elapsedTime += checkInterval;

        //         if (CheckAllPlayersInstantiatedObjects())
        //         {
        //             Debug.Log("All player objects are correctly instantiated and visible on all clients.");
        //             yield break; // Stop the coroutine if all objects are present
        //         }
        //         else
        //         {
        //             Debug.LogWarning("Some player objects are missing. Updating visibility status and re-instantiating if necessary.");

        //             // Update custom properties based on current visibility of all players
        //             UpdatePlayerVisibilityProperties();

        //             // Destroy all local player's objects with PhotonView.IsMine if necessary
        //             if (ShouldReinstantiateLocalPlayer())
        //             {
        //                 DestroyAllLocalObjects();
        //                 // Re-instantiate the local player object
        //                 hasSpawnedPlayer = false;
        //                 SpawnPlayerObject();
        //             }
        //         }
        //     }

        //     Debug.LogError("Failed to verify that all player objects are instantiated within the given time.");
        // }


        // // Checks if all player objects are instantiated and visible across the network based on PhotonView ownership
        // private bool CheckAllPlayersInstantiatedObjects()
        // {
        //     Debug.Log("PLAY FUNCTION: Photon_Manager_Room/CheckAllPlayersInstantiatedObjects()");
        //     bool allPlayersVisible = true;

        //     // Find all objects in the scene that have a PhotonView component
        //     var photonViews = FindObjectsOfType<PhotonView>();

        //     // Go through each player in the room and check if their object is instantiated by comparing PhotonView ownership
        //     foreach (Player player in PhotonNetwork.PlayerList)
        //     {
        //         bool isVisible = photonViews.Any(view => view.Owner != null && view.Owner == player);

        //         // If not visible, update the visibility flag
        //         if (!isVisible)
        //         {
        //             allPlayersVisible = false;
        //         }

        //         // Update custom properties to track the visibility of this player's object
        //         string key = $"Player_{player.NickName}_Instantiated";
        //         ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable { { key, isVisible } };
        //         PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        //     }

        //     return allPlayersVisible;
        // }

        // // Updates custom properties to indicate which players' objects are visible
        // private void UpdatePlayerVisibilityProperties()
        // {
        //     Debug.Log("PLAY FUNCTION: Photon_Manager_Room/UpdatePlayerVisibilityProperties()");
        //     var photonViews = FindObjectsOfType<PhotonView>();

        //     foreach (Player player in PhotonNetwork.PlayerList)
        //     {
        //         bool isVisible = photonViews.Any(view => view.Owner != null && view.Owner == player);

        //         // Set a custom property for the local player indicating whether the target player's object is visible
        //         string key = $"Player_{player.NickName}_Instantiated";
        //         ExitGames.Client.Photon.Hashtable visibilityProps = new ExitGames.Client.Photon.Hashtable { { key, isVisible } };
        //         PhotonNetwork.LocalPlayer.SetCustomProperties(visibilityProps);
        //     }
        // }

        // // Checks if the local player needs to re-instantiate their object based on other players' visibility data
        // private bool ShouldReinstantiateLocalPlayer()
        // {
        //     Debug.Log("PLAY FUNCTION: Photon_Manager_Room/ShouldReinstantiateLocalPlayer()");
        //     foreach (Player otherPlayer in PhotonNetwork.PlayerListOthers)
        //     {
        //         // Check if this player can see the local player's object
        //         string key = $"Player_{PhotonNetwork.LocalPlayer.NickName}_Instantiated";
        //         if (otherPlayer.CustomProperties.TryGetValue(key, out object isVisible) && !(bool)isVisible)
        //         {
        //             Debug.LogWarning($"Player {otherPlayer.NickName} cannot see the local player's object. Re-instantiating.");
        //             return true;
        //         }
        //     }

        //     return false;
        // }

        // public override void OnPlayerEnteredRoom(Player newPlayer)
        // {
        //     Debug.Log($"New player entered the room: {newPlayer.NickName}");
            
        //     // If the local player is the master client, instantiate the object for the new player
        //     if (PhotonNetwork.IsMasterClient)
        //     {
        //         Debug.Log("Master client is handling the new player instantiation.");
        //         CheckAndSpawnPlayerObject(newPlayer);
        //     }
        // }

        // // Method to check and spawn the player object for the specific player
        // private void CheckAndSpawnPlayerObject(Player player)
        // {
        //     bool playerObjectExists = FindObjectsOfType<PhotonView>().Any(view => view.Owner == player);

        //     if (!playerObjectExists)
        //     {
        //         Debug.Log($"Instantiating player object for {player.NickName}.");
        //         PhotonNetwork.Instantiate(playerPrefabs[0].name, Vector3.zero, Quaternion.identity);
        //     }
        //     else
        //     {
        //         Debug.Log($"Player object already exists for {player.NickName}.");
        //     }
        // }

    }
}
