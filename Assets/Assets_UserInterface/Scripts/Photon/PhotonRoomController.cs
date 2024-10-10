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
using CJ;

namespace KnoxGameStudios
{
    public class PhotonRoomController : MonoBehaviourPunCallbacks
    {
//_____________________________________________________________________________________________________________________
// VARIABLES
//---------------------------------------------------------------------------------------------------------------------
        // GAMEOBJECT:
        [SerializeField] private GameObject playerProfilePrefab; // Reference to the Player Profile prefab for instantiation
        [SerializeField] private GameObject _Photon_Manager_Spawn; // Reference to the prefab you want to instantiate

        // GAMEMODE:
        [SerializeField] private GameMode _selectedGameMode;
        [SerializeField] private GameMode[] _availableGameModes;
        
        // BOOL:
        [SerializeField] private bool _startGame;
        
        // FLOAT
        [SerializeField] private float _currentCountDown;
        private const float GAME_COUNT_DOWN = 10f;

        // INT
        [SerializeField] private int _gameSceneIndex;

        // STRING
        private const string GAME_MODE = "GAMEMODE";
        private const string START_GAME = "STARTGAME";
        private const string PLAYER_READY = "PlayerReady";
        private const string SCENE_NAME = "PVP_Arena_Mobile"; // Your target scene name

        // STATIC
        public static Action<GameMode> OnJoinRoom = delegate { };
        public static Action<bool> OnRoomStatusChange = delegate { };
        public static Action OnRoomLeft = delegate { };
        public static Action<Player> OnOtherPlayerLeftRoom = delegate { };
        public static Action<Player> OnMasterOfRoom = delegate { };
        public static Action<float> OnCountingDown = delegate { };


//_____________________________________________________________________________________________________________________
// START FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------
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

            _startGame = false;
        }


//_____________________________________________________________________________________________________________________
// ON DESTROY:
//---------------------------------------------------------------------------------------------------------------------
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


//_____________________________________________________________________________________________________________________
// VOID UPDATE
//---------------------------------------------------------------------------------------------------------------------
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
                Debug.Log("STARTING GAME!!");
                _startGame = false;
                
                Debug.Log("Loading Arena Scene");
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.LoadLevel(SCENE_NAME); // Load the target game scene
            }
        }


//_____________________________________________________________________________________________________________________
// HANDLE METHODS
//---------------------------------------------------------------------------------------------------------------------
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

        // Function invoked by the "START BUTTON" & "READY BUTTON"
        private void HandleStartGame()
        {
        // If: IsMasterClient ==> Start Game
        // If: !IsMasterClient ==> Ready Up
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
        }


        private void HandleKickPlayer(Player kickedPlayer)
        {
            if (PhotonNetwork.LocalPlayer.Equals(kickedPlayer))
            {
                HandleLeaveRoom();
            }
        }


//_____________________________________________________________________________________________________________________
// STARTING GAME FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
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


        public void SendReadyStatusToMaster()
        {
            // Check if the "CSN" property exists and is not null or empty
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CSN", out object selectedIndexObject) && selectedIndexObject != null)
            {
                Hashtable playerReadyProperty = new Hashtable
                {
                    { PLAYER_READY, true }
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerReadyProperty);
                Debug.Log("Ready status sent to the master client.");
                SpawnPhotonManager();
            }
            else
            {
                Debug.LogWarning("CSN property is either null or does not exist. Ready status not sent.");
            }
        }


        private void StartGameAsMaster()
        {
            // Set the start game property to true for all players
            Hashtable startRoomProperty = new Hashtable()
            {
                { START_GAME, true }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(startRoomProperty);
        }



        private void AutoStartGame()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= _selectedGameMode.MaxPlayers)
                Debug.Log("The room is full. Waiting for the master client to start the game.");
                // The game will now wait for the Master Client to manually start it.
        }


//_____________________________________________________________________________________________________________________
// ROOM FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
        private void CreatePhotonRoom()
        {
            string roomName = Guid.NewGuid().ToString();
            RoomOptions ro = GetRoomOptions();

            PhotonNetwork.JoinOrCreateRoom(roomName, ro, TypedLobby.Default);
        }
        
        
        private void JoinPhotonRoom()
        {
            Hashtable expectedCustomRoomProperties = new Hashtable
            { {GAME_MODE, _selectedGameMode.Name} };

            PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
        }


        private RoomOptions GetRoomOptions()
        {
            RoomOptions ro = new RoomOptions();
            ro.IsOpen = true;
            ro.IsVisible = true;
            ro.MaxPlayers = _selectedGameMode.MaxPlayers;

            string[] roomProperties = { GAME_MODE };

            Hashtable customRoomProperties = new Hashtable
            { {GAME_MODE, _selectedGameMode.Name} };

            ro.CustomRoomPropertiesForLobby = roomProperties;
            ro.CustomRoomProperties = customRoomProperties;

            return ro;
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
        

        private void DebugPlayerList()
        {
            string players = "";
            foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
            {
                players += $"{player.Value.NickName}, ";
            }
            Debug.Log($"Current Room Players: {players}");
        }


//_____________________________________________________________________________________________________________________
// SUPPORT FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------
        private void SpawnPhotonManager()
        {
            // Instantiate the Photon_Manager_Spawn object for the local player
            GameObject playerSpawn = PhotonNetwork.Instantiate(_Photon_Manager_Spawn.name, Vector3.zero, Quaternion.identity);
            Debug.Log("Local player object spawned");
        }


//_____________________________________________________________________________________________________________________
// OVERRIDE FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            object startGameObject;
            if (propertiesThatChanged.TryGetValue(START_GAME, out startGameObject))
            {
                _startGame = (bool)startGameObject;
                if (_startGame)
                {
                    _currentCountDown = GAME_COUNT_DOWN;
                }
                if (_startGame && PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.CurrentRoom.IsVisible = true;
                    PhotonNetwork.CurrentRoom.IsOpen = true;
                }
            }   
        }

        
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
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"You failed to join a Photon room: {message}");
        }


        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log($"OnJoinRandomFailed {message}");
            CreatePhotonRoom();
        }


        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"Another player has joined the room {newPlayer.NickName}");
            DebugPlayerList();
            AutoStartGame();

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


    }
}
