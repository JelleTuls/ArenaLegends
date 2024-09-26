/*
PhotonRoomController.cs

Purpose:
    The PhotonRoomController script manages the overall room logic, including player readiness checks, scene transitions, 
    and game mode selection. It communicates with Photon Networking to handle multiplayer room events and synchronizes 
    gameplay across all connected players.

Key functionalities:
    - Room Management: It handles joining, creating, and leaving rooms.
    - Player Readiness Checks: The master client checks if all players are ready before starting the game.
    - Scene Synchronization: It loads the game scene for all players once the game starts. The master client 
        controls when the scene transition happens.
    - Event Handling: It responds to events like when players join or leave the room, the master client changes, 
        or room properties are updated.
    - Photon Callbacks: It overrides various Photon callbacks to react to changes in the room state (e.g., when a 
        player leaves, the room properties change, etc.).

Differences:
    This script handles the actual game management logic. It ensures all players are in sync, manages scene transitions, 
    and checks for player readiness. It differs from UIDisplayRoom because it controls the room's backend logic rather 
    than the user interface.
*/

using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace KnoxGameStudios
{
    public class PhotonRoomController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject playerProfilePrefab; // Reference to the Player Profile prefab for instantiation

        [SerializeField] private GameMode _selectedGameMode;
        [SerializeField] private GameMode[] _availableGameModes;
        [SerializeField] private bool _startGame;
        [SerializeField] private float _currentCountDown;
        [SerializeField] private int _gameSceneIndex;

        private const string GAME_MODE = "GAMEMODE";
        private const string START_GAME = "STARTGAME";
        private const string PLAYER_READY = "PlayerReady";
        private const string SCENE_NAME = "PVP_Arena_Mobile"; // Your target scene name
        private const float GAME_COUNT_DOWN = 10f;

        public static Action<GameMode> OnJoinRoom = delegate { };
        public static Action<bool> OnRoomStatusChange = delegate { };
        public static Action OnRoomLeft = delegate { };
        public static Action<Player> OnOtherPlayerLeftRoom = delegate { };
        public static Action<Player> OnMasterOfRoom = delegate { };
        public static Action<float> OnCountingDown = delegate { };

        private void Awake()
        {
            UIGameMode.OnGameModeSelected += HandleGameModeSelected;
            UIInvite.OnRoomInviteAccept += HandleRoomInviteAccept;
            PhotonConnector.OnLobbyJoined += HandleLobbyJoined;
            UIDisplayRoom.OnLeaveRoom += HandleLeaveRoom;
            UIDisplayRoom.OnStartGame += HandleStartGame;
            UIFriend.OnGetRoomStatus += HandleGetRoomStatus;
            UIPlayerSelection.OnKickPlayer += HandleKickPlayer;

            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void OnDestroy()
        {
            UIGameMode.OnGameModeSelected -= HandleGameModeSelected;
            UIInvite.OnRoomInviteAccept -= HandleRoomInviteAccept;
            PhotonConnector.OnLobbyJoined -= HandleLobbyJoined;
            UIDisplayRoom.OnLeaveRoom -= HandleLeaveRoom;
            UIDisplayRoom.OnStartGame -= HandleStartGame;
            UIFriend.OnGetRoomStatus -= HandleGetRoomStatus;
            UIPlayerSelection.OnKickPlayer -= HandleKickPlayer;
        }

        private void Update()
        {
            // This method handles the game countdown logic for starting the game.
            if (!_startGame) return;

            if (_currentCountDown > 0)
            {
                OnCountingDown?.Invoke(_currentCountDown);
                _currentCountDown -= Time.deltaTime;
            }
            else
            {
                _startGame = false;
                // Load the game scene only if the player is the Master Client
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.LoadLevel(SCENE_NAME); // Load the target game scene
                }
            }
        }

        #region Handle Methods
        private void HandleGameModeSelected(GameMode gameMode)
        {
            if (!PhotonNetwork.IsConnectedAndReady) return;
            if (PhotonNetwork.InRoom) return;

            _selectedGameMode = gameMode;
            Debug.Log($"Joining new {_selectedGameMode.Name} game");
            JoinPhotonRoom();
        }

        private void HandleRoomInviteAccept(string roomName)
        {
            PlayerPrefs.SetString("PHOTONROOM", roomName);
            if (PhotonNetwork.InRoom)
            {
                OnRoomLeft?.Invoke();
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                if (PhotonNetwork.InLobby)
                {
                    PhotonNetwork.JoinRoom(roomName);
                    PlayerPrefs.SetString("PHOTONROOM", "");
                }
            }
        }

        private void HandleLobbyJoined()
        {
            string roomName = PlayerPrefs.GetString("PHOTONROOM");
            if (!string.IsNullOrEmpty(roomName))
            {
                PhotonNetwork.JoinRoom(roomName);
                PlayerPrefs.SetString("PHOTONROOM", "");
            }
        }

        private void HandleLeaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                OnRoomLeft?.Invoke();
                PhotonNetwork.LeaveRoom();
            }
        }

        private void HandleGetRoomStatus()
        {
            OnRoomStatusChange?.Invoke(PhotonNetwork.InRoom);
        }

        private void HandleStartGame()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
                {
                    if (AllPlayersReady())
                    {
                        Debug.Log("All players are ready. Starting the game...");
                        StartGameAsMaster();
                    }
                    else
                    {
                        Debug.Log("Waiting for all players to be ready...");
                    }
                }
                else
                {
                    Debug.Log("Only one player in the room. Starting the game...");
                    StartGameAsMaster();
                }
            }
            else
            {
                SendReadyStatusToMaster();
            }
        }

        private void StartGameAsMaster()
        {
            Hashtable startRoomProperty = new Hashtable
            {
                {START_GAME, true}
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(startRoomProperty);
        }

        private bool AllPlayersReady()
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!player.CustomProperties.ContainsKey(PLAYER_READY) || !(bool)player.CustomProperties[PLAYER_READY])
                {
                    return false; // A player is not ready
                }
            }
            return true; // All players are ready
        }

        private void SendReadyStatusToMaster()
        {
            Hashtable playerReadyProperty = new Hashtable
            {
                {PLAYER_READY, true}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerReadyProperty);
            Debug.Log("Ready status sent to the master client.");
        }

        private void HandleKickPlayer(Player kickedPlayer)
        {
            if (PhotonNetwork.LocalPlayer.Equals(kickedPlayer))
            {
                HandleLeaveRoom();
            }
        }
        #endregion

        #region Private Methods
        private void JoinPhotonRoom()
        {
            Hashtable expectedCustomRoomProperties = new Hashtable
            {
                {GAME_MODE, _selectedGameMode.Name}
            };

            PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
        }

        private void CreatePhotonRoom()
        {
            string roomName = Guid.NewGuid().ToString();
            RoomOptions ro = GetRoomOptions();

            PhotonNetwork.JoinOrCreateRoom(roomName, ro, TypedLobby.Default);
        }

        private RoomOptions GetRoomOptions()
        {
            RoomOptions ro = new RoomOptions
            {
                IsOpen = true,
                IsVisible = true,
                MaxPlayers = _selectedGameMode.MaxPlayers
            };

            string[] roomProperties = { GAME_MODE };

            Hashtable customRoomProperties = new Hashtable
            {
                {GAME_MODE, _selectedGameMode.Name}
            };

            ro.CustomRoomPropertiesForLobby = roomProperties;
            ro.CustomRoomProperties = customRoomProperties;

            return ro;
        }

        private void DebugPlayerList()
        {
            string players = "";
            foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
            {
                players += $"{player.Value.NickName}, ";
            }
            Debug.Log($"Current Room Players: {players}");
        }

        private GameMode GetRoomGameMode()
        {
            string gameModeName = (string)PhotonNetwork.CurrentRoom.CustomProperties[GAME_MODE];
            GameMode gameMode = null;
            for (int i = 0; i < _availableGameModes.Length; i++)
            {
                if (string.Compare(_availableGameModes[i].Name, gameModeName) == 0)
                {
                    gameMode = _availableGameModes[i];
                    break;
                }
            }
            return gameMode;
        }

        private void AutoStartGame()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= _selectedGameMode.MaxPlayers)
                HandleStartGame();
        }

        private Transform GetPlayersContainer()
        {
            // Navigate through the known hierarchy to find 'UI Team_ArenaLegends(Clone)' objects
            Transform teamsContainer = GameObject.Find("All In One Canvas/Main Layout/Middle/Main Content/Room Content/Teams")?.transform;

            if (teamsContainer == null)
            {
                Debug.LogError("'Teams' object not found in the hierarchy.");
                return null;
            }

            // Find all 'UI Team_ArenaLegends(Clone)' children under 'Teams'
            List<Transform> teamObjects = new List<Transform>();
            foreach (Transform child in teamsContainer)
            {
                if (child.name.Contains("UI Team_ArenaLegends(Clone)"))
                {
                    teamObjects.Add(child);
                }
            }

            if (teamObjects.Count == 0)
            {
                Debug.LogError("No 'UI Team_ArenaLegends(Clone)' objects found under 'Teams'.");
                return null;
            }

            // Sort teamObjects by sibling index (higher in the hierarchy comes first)
            teamObjects.Sort((a, b) => a.GetSiblingIndex().CompareTo(b.GetSiblingIndex()));

            // Iterate through team objects to find the first available 'Players_Container' with less than 3 children
            foreach (Transform teamObject in teamObjects)
            {
                Transform playersContainer = teamObject.Find("Players_Container");
                if (playersContainer != null && playersContainer.childCount < 3)
                {
                    return playersContainer;
                }
            }

            // If all are full, return the 'Players_Container' from the second 'UI Team_ArenaLegends(Clone)'
            if (teamObjects.Count > 1)
            {
                Transform fallbackContainer = teamObjects[1].Find("Players_Container");
                if (fallbackContainer != null)
                {
                    return fallbackContainer;
                }
            }

            Debug.LogError("No valid 'Players_Container' found.");
            return null;
        }

        private void SpawnImageForPlayer(Player player)
        {
            if (playerProfilePrefab == null)
            {
                Debug.LogError("Player Profile prefab is not assigned in the inspector.");
                return;
            }

            // Get the correct Players_Container to attach the image
            Transform playersContainer = GetPlayersContainer();
            if (playersContainer == null)
            {
                Debug.LogError("Players_Container not found.");
                return;
            }

            // Instantiate the player profile using PhotonNetwork.Instantiate
            GameObject spawnedProfile = PhotonNetwork.Instantiate(playerProfilePrefab.name, Vector3.zero, Quaternion.identity);

            // Set the profile as a child of the Players_Container object
            spawnedProfile.transform.SetParent(playersContainer, false);

            // Optionally, you can assign the profile a name or label it with the player's nickname
            spawnedProfile.name = $"Player_Profile_{player.NickName}";

            Debug.Log($"Spawned Player Profile for player: {player.NickName} as child of {playersContainer.name}");
        }
        #endregion

        #region Photon Callbacks
        public override void OnCreatedRoom()
        {
            Debug.Log($"You have created a Photon Room named {PhotonNetwork.CurrentRoom.Name}");
            OnMasterOfRoom?.Invoke(PhotonNetwork.LocalPlayer);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log($"You have joined the Photon room {PhotonNetwork.CurrentRoom.Name}");
            DebugPlayerList();

            // Set up the selected game mode from room properties
            _selectedGameMode = GetRoomGameMode();
            OnJoinRoom?.Invoke(_selectedGameMode);
            OnRoomStatusChange?.Invoke(PhotonNetwork.InRoom);

            // Ensure the local player is initialized properly before doing anything else
            InitializeLocalPlayer();

            // Handle room synchronization if other players are already in the room
            if (!PhotonNetwork.IsMasterClient)
            {
                // Synchronize room data or request room properties from the master client if needed
                Debug.Log("You are not the Master Client. Syncing data with the Master Client.");
                SyncWithMasterClient();
            }

            // Spawn the image for the local player
            SpawnImageForPlayer(PhotonNetwork.LocalPlayer);
        }

        private void InitializeLocalPlayer()
        {
            // Assuming UIPlayerSelection is the script handling the player's selection UI
            UIPlayerSelection playerSelection = FindObjectOfType<UIPlayerSelection>();

            if (playerSelection != null)
            {
                // Initialize the player selection for the local player
                playerSelection.Initialize(PhotonNetwork.LocalPlayer);
            }
            else
            {
                Debug.LogError("UIPlayerSelection not found for the local player.");
            }

            // You can also initialize other Photon properties here if necessary
        }

        private void SyncWithMasterClient()
        {
            // Placeholder method: Here you might send an RPC to the master client requesting current game state or room properties.
            // Example:
            // photonView.RPC("RequestRoomState", PhotonNetwork.MasterClient);

            // Or you could use `PhotonNetwork.CurrentRoom.CustomProperties` to retrieve any room properties or state information.
            Debug.Log("Synchronizing data with Master Client.");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            Debug.Log($"Player joined: {newPlayer.NickName}");

            // Spawn an image object for the new player
            SpawnImageForPlayer(newPlayer);

            // Ensure all existing players update their UI to reflect the new player joining
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player != newPlayer)  // Ensure we don't update the UI for the new player itself
                {
                    photonView.RPC("UpdatePlayerUIForOthers", RpcTarget.All, player.ActorNumber);
                }
            }

            // Initialize the new player's UI
            UIDisplayRoom.Instance.ShowReadyButton();
        }

        public override void OnLeftRoom()
        {
            Debug.Log("You have left a Photon Room");
            _selectedGameMode = null;
            _startGame = false;
            OnRoomStatusChange?.Invoke(PhotonNetwork.InRoom);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log($"OnJoinRandomFailed {message}");
            CreatePhotonRoom();
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"You failed to join a Photon room: {message}");
        }

        [PunRPC]
        private void UpdatePlayerUIForOthers(int playerActorNumber)
        {
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber);
            if (player != null)
            {
                Debug.Log($"Updating UI for player {player.NickName}");

                UIPlayerSelection playerSelection = FindPlayerSelectionUI(player);
                if (playerSelection != null)
                {
                    playerSelection.Initialize(player);  // Initialize UI elements for the player
                }
            }
        }

        private UIPlayerSelection FindPlayerSelectionUI(Player player)
        {
            UIPlayerSelection[] playerSelectionUIs = FindObjectsOfType<UIPlayerSelection>();

            foreach (var playerSelection in playerSelectionUIs)
            {
                if (playerSelection.Owner == player)
                {
                    return playerSelection;
                }
            }
            return null;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"Player has left the room {otherPlayer.NickName}");
            OnOtherPlayerLeftRoom?.Invoke(otherPlayer);
            DebugPlayerList();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log($"New Master Client is {newMasterClient.NickName}");
            OnMasterOfRoom?.Invoke(newMasterClient);
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (propertiesThatChanged.ContainsKey(START_GAME))
            {
                if (propertiesThatChanged[START_GAME] is bool startGame)
                {
                    _startGame = startGame;

                    if (_startGame)
                    {
                        _currentCountDown = GAME_COUNT_DOWN;
                        Debug.Log("Game is starting. Countdown initiated.");

                        if (PhotonNetwork.IsMasterClient)
                        {
                            PhotonNetwork.CurrentRoom.IsVisible = false;
                            PhotonNetwork.CurrentRoom.IsOpen = false;
                            Debug.Log("Room is now closed and hidden as the game is starting.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("START_GAME flag is set to false.");
                    }
                }
                else
                {
                    Debug.LogError("START_GAME property exists but is not a boolean. Value: " + propertiesThatChanged[START_GAME]);
                }
            }
            else
            {
                Debug.LogError("START_GAME property was updated but is missing from the properties.");
            }
        }
        #endregion
    }
}
