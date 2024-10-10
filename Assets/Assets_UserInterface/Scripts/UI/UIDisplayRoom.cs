/*
UIDisplayRoom.cs
Purpose:
    The UIDisplayRoom script manages the user interface elements associated with the room, 
    such as the room title, buttons (start, ready, exit), and provides logic for handling 
    when players are ready to start the game.

Key functionalities:
    - UI Control: It manages which buttons (start or ready) are visible based on the 
    player’s role (master client or regular player).
    - PhotonView Integration: It ensures the player’s actions (e.g., clicking "ready") are 
    synchronized across the network using Photon’s RPC (Remote Procedure Call).
    - Event Handling: It subscribes to and responds to events triggered by the PhotonRoomController 
    to update the UI when players join or leave the room, when the countdown starts, or when the 
    master client changes.
    - Ready/Start Mechanism:
        Non-master clients have a "Ready" button to indicate that they are ready to play.
        The master client has a "Start" button and can start the game once all players are ready.
        The master client also receives an RPC notification when a non-master player clicks "Ready."

Differences:
    This script is mainly focused on the UI and player readiness tracking. It does not handle the game 
    logic (e.g., loading scenes or room setup) but acts on instructions from the PhotonRoomController.
*/

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using TMPro;
using KnoxGameStudios;

namespace KnoxGameStudios
{
    public class UIDisplayRoom : MonoBehaviourPunCallbacks
    {
//_____________________________________________________________________________________________________________________
// VARIABLES:
//---------------------------------------------------------------------------------------------------------------------
        //TEXT OBJECTS
        [SerializeField] private TMP_Text _roomTitleText;

        //GAME OBJECTS
        [SerializeField] private GameObject _startButton;
        [SerializeField] private GameObject _readyButton;
        [SerializeField] private GameObject _exitButton;
        [SerializeField] private GameObject _roomContainer;

        [SerializeField] private GameObject roomControllerObject;

        //LISTS
        [SerializeField] private GameObject[] _hideObjects; // List of objects to hide when joining or leaving a room
        [SerializeField] private GameObject[] _showObjects; // List of objects to show when joining or leaving a room

        //PUBLIC STATIC (ACTIONS)
        public static UIDisplayRoom Instance { get; private set; }
        public static Action OnStartGame = delegate { };
        public static Action OnLeaveRoom = delegate { };

        //STRINGS
        private const string PLAYER_READY = "PlayerReady"; // Key for CustomProperties to store ready status

        //PHOTON
        //private PhotonView photonView;

        //REFERENCES
        private PhotonRoomController roomController;


//_____________________________________________________________________________________________________________________
// START FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Optional: if you want the instance to persist between scenes
            }
            else
            {
                Destroy(gameObject); // Ensure that there's only one instance
            }

            PhotonRoomController.OnJoinRoom += HandleJoinRoom;
            PhotonRoomController.OnRoomLeft += HandleRoomLeft;
            PhotonRoomController.OnMasterOfRoom += HandleMasterOfRoom;
            PhotonRoomController.OnCountingDown += HandleCountingDown;
        }


//_____________________________________________________________________________________________________________________
// ON DESTROY:
//---------------------------------------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            PhotonRoomController.OnJoinRoom -= HandleJoinRoom;
            PhotonRoomController.OnRoomLeft -= HandleRoomLeft;
            PhotonRoomController.OnMasterOfRoom -= HandleMasterOfRoom;
            PhotonRoomController.OnCountingDown -= HandleCountingDown;
        }


//_____________________________________________________________________________________________________________________
// HANDLE METHODS:
//---------------------------------------------------------------------------------------------------------------------
        private void HandleJoinRoom(GameMode gameMode)
        {
            _roomTitleText.SetText(PhotonNetwork.CurrentRoom.CustomProperties["GAMEMODE"].ToString());
            _exitButton.SetActive(true);
            _roomContainer.SetActive(true);

            // EXTRA CALL COMPARED TO THE ORIGINAL KNOX
            ShowReadyButton();
            
            foreach (GameObject obj in _hideObjects)
            {
                obj.SetActive(false);
            }
        }


        private void HandleRoomLeft()
        {
            _roomTitleText.SetText("JOINING ROOM");
            _startButton.SetActive(false);
            _readyButton.SetActive(false);
            _exitButton.SetActive(false);

            foreach (GameObject obj in _showObjects)
            {
                obj.SetActive(true);
            }
        }


        private void HandleMasterOfRoom(Player masterPlayer)
        {
            _roomTitleText.SetText(PhotonNetwork.CurrentRoom.CustomProperties["GAMEMODE"].ToString());

            ShowReadyButton();
        }


        private void HandleCountingDown(float count)
        {
            _startButton.SetActive(false);
            _readyButton.SetActive(false);
            _exitButton.SetActive(false);
            _roomTitleText.SetText(count.ToString("F0"));
        }


//_____________________________________________________________________________________________________________________
// START AND READY FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
        public void ShowReadyButton()
        {
            _startButton.SetActive(false);
            _readyButton.SetActive(true);
        }


        private void ShowPlayButton()
        {
            _startButton.SetActive(true);
            _readyButton.SetActive(false);
        }


        public void ReadyUp()
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CSN", out object selectedIndexObject) && selectedIndexObject != null)
            {
                // Set the player's "ready" status in their CustomProperties
                Hashtable readyProperty = new Hashtable { { PLAYER_READY, true } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(readyProperty);

                // Hide the ready button
                _readyButton.SetActive(false);

                // If the local player is the master client, show the Play button
                if (PhotonNetwork.IsMasterClient)
                {
                    ShowPlayButton();
                }

                // Find the GameObject by the full path in the hierarchy
                if (roomControllerObject != null)
                {
                    // Get the PhotonRoomController component from that GameObject
                    roomController = roomControllerObject.GetComponent<PhotonRoomController>();

                    if (roomController != null)
                    {
                        roomController.SendReadyStatusToMaster(); // Call the method
                    }
                    else
                    {
                        Debug.LogError("PhotonRoomController component not found on the GameObject.");
                    }
                }
                else
                {
                    Debug.LogError("GameObject 'Network Controllers/Photon/Room' not found in the scene.");
                }
            }
        }


        private bool CheckAllPlayersReady()
        {
            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (player.CustomProperties.TryGetValue(PLAYER_READY, out object isReady))
                {
                    if (!(bool)isReady)
                    {
                        Debug.Log("Player not ready: " + player.NickName);
                        return false;
                    }
                }
                else
                {
                    Debug.Log("Player not ready: " + player.NickName);
                    return false;
                }
            }

            // If all players are ready, return true
            return true;
        }


        public void LeaveRoom()
        {
            OnLeaveRoom?.Invoke();
        }


        public void StartGame()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                bool allPlayersReady = CheckAllPlayersReady();

                if (allPlayersReady)
                {
                    Debug.Log("All players are ready. Starting the game...");
                    OnStartGame?.Invoke();
                }
                else
                {
                    Debug.Log("Not all players are ready. Waiting...");
                }
            }
        }


    }
}
