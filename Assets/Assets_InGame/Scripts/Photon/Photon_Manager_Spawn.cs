using UnityEngine;
using UnityEngine.SceneManagement; // Required for SceneManager
using Photon.Pun;
using Photon.Realtime; // For Player class

namespace CJ
{
    public class Photon_Manager_Spawn : MonoBehaviourPunCallbacks
    {
//_____________________________________________________________________________________________________________________
// VARIABLES
//---------------------------------------------------------------------------------------------------------------------
        // GAME OBJECT
        public GameObject[] playerPrefabs; // Array to store different player prefabs

        // REFERENCE
        private PhotonView photonView; // Reference to the player's PhotonView component

        // BOOL
        private bool hasSpawnedPlayer = false;


//_____________________________________________________________________________________________________________________
// AWAKE FUNCTION
//---------------------------------------------------------------------------------------------------------------------
        private void Awake()
        {
            photonView = GetComponent<PhotonView>();

            if (photonView.Owner != null) // Make sure the PhotonView has an owner
            {
                // Rename the instantiated object to "Photon_Manager_<PlayerName>"
                this.name = $"Photon_Manager_{photonView.Owner.NickName}"; // Set name based on owner
                
                // Mark the object this script is attached to as "DontDestroyOnLoad"
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Debug.LogWarning("PhotonView has no owner yet.");
            }
        }


//_____________________________________________________________________________________________________________________
// START FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------
        private void Start()
        {
            // Register the OnSceneLoaded callback when the script is enabled
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


//_____________________________________________________________________________________________________________________
// CALLBACK FUNCTION
//---------------------------------------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            // Unregister the OnSceneLoaded callback when the script is destroyed
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }


        // This method is called every time a new scene is loaded
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("PLAY FUNCTION: OnSceneLoaded()");
            // Check if the scene name contains "PVP" and if the player hasn't spawned yet
            if (scene.name.Contains("PVP") && hasSpawnedPlayer == false)
            {
                Debug.Log("A scene with a name containing PVP is correctly recognized");
                //SpawnPlayer();  // Your custom method to spawn the player
                hasSpawnedPlayer = true; // Prevents multiple spawns
            }
        }
        

//_____________________________________________________________________________________________________________________
// SUPPORTING FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------
        // Spawns the player prefab across the network for the local player
        private void SpawnPlayer()
        {
            Debug.Log("PLAY FUNCTION: SpawnPlayer()");
            
            // Get the PhotonView component attached to the current GameObject
            PhotonView photonView = GetComponent<PhotonView>();

            // Check if the PhotonView has an owner
            if (photonView.Owner != null)
            {
                // Retrieve the custom properties of the player who owns this PhotonView
                if (photonView.Owner.CustomProperties.TryGetValue("CSN", out object selectedIndexObject))
                {
                    int selectedIndex = (int)selectedIndexObject;
                    Debug.Log($"Character selection index for {photonView.Owner.NickName}: {selectedIndex}");

                    if (selectedIndex >= 0 && selectedIndex < playerPrefabs.Length)
                    {
                        // Instantiate the player prefab across the network
                        GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefabs[selectedIndex].name, Vector3.zero, Quaternion.identity);
                        
                        // Mark this object to persist across scene loads
                        DontDestroyOnLoad(playerInstance);
                        
                        Debug.Log($"Player instance for {photonView.Owner.NickName} created and marked as DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.LogError($"Invalid character selection index for {photonView.Owner.NickName}.");
                    }
                }
                else
                {
                    Debug.LogError($"CSN property not found for player {photonView.Owner.NickName}.");
                }
            }
            else
            {
                Debug.LogError("PhotonView has no owner.");
            }
        }
    }
}
