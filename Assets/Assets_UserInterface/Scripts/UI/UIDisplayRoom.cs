// Necessary using directives for Photon and Unity functionalities
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;

namespace KnoxGameStudios
{
    // UIDisplayRoom class to handle UI updates related to room status in a multiplayer game
    public class UIDisplayRoom : MonoBehaviour
    {
        // Serialized fields to be assigned in the Unity Editor
        [SerializeField] private TMP_Text _roomTitleText;  // Text component to display the room title
        [SerializeField] private GameObject _startButton;  // Button to start the game
        [SerializeField] private GameObject _exitButton;  // Button to leave the room
        [SerializeField] private GameObject _roomContainer;  // Container for room-related UI elements
        [SerializeField] private GameObject[] _hideObjects;  // Array of objects to hide on joining a room
        [SerializeField] private GameObject[] _showObjects;  // Array of objects to show on leaving a room

        // Static Action events to notify when the game starts or when a player leaves the room
        public static Action OnStartGame = delegate { };
        public static Action OnLeaveRoom = delegate { };

        // Method called when the script instance is being loaded
        private void Awake()
        {
            // Subscribe to PhotonRoomController events
            PhotonRoomController.OnJoinRoom += HandleJoinRoom;
            PhotonRoomController.OnRoomLeft += HandleRoomLeft;
            PhotonRoomController.OnMasterOfRoom += HandleMasterOfRoom;
            PhotonRoomController.OnCountingDown += HandleCountingDown;
        }

        // Method called when the script instance is being destroyed
        private void OnDestroy()
        {
            // Unsubscribe from PhotonRoomController events
            PhotonRoomController.OnJoinRoom -= HandleJoinRoom;
            PhotonRoomController.OnRoomLeft -= HandleRoomLeft;
            PhotonRoomController.OnMasterOfRoom -= HandleMasterOfRoom;
            PhotonRoomController.OnCountingDown -= HandleCountingDown;
        }

        // Method to handle actions when a player joins a room
        private void HandleJoinRoom(GameMode gameMode)
        {
            // Set the room title to the game mode from the room's custom properties
            _roomTitleText.SetText(PhotonNetwork.CurrentRoom.CustomProperties["GAMEMODE"].ToString());

            // Show the exit button and room container
            _exitButton.SetActive(true);
            _roomContainer.SetActive(true);

            // Hide objects specified in the _hideObjects array
            foreach (GameObject obj in _hideObjects)
            {
                obj.SetActive(false);
            }
        }

        // Method to handle actions when a player leaves a room
        private void HandleRoomLeft()
        {
            // Set the room title to "JOINING ROOM"
            _roomTitleText.SetText("JOINING ROOM");

            // Hide the start and exit buttons and room container
            _startButton.SetActive(false);
            _exitButton.SetActive(false);
            _roomContainer.SetActive(false);

            // Show objects specified in the _showObjects array
            foreach (GameObject obj in _showObjects)
            {
                obj.SetActive(true);
            }
        }

        // Method to handle actions when a player becomes the master of the room
        private void HandleMasterOfRoom(Player masterPlayer)
        {
            // Set the room title to the game mode from the room's custom properties
            _roomTitleText.SetText(PhotonNetwork.CurrentRoom.CustomProperties["GAMEMODE"].ToString());

            // If the local player is the master, show the start button, otherwise hide it
            if (PhotonNetwork.LocalPlayer.Equals(masterPlayer))
            {
                _startButton.SetActive(true);
            }
            else
            {
                _startButton.SetActive(false);
            }
        }

        // Method to handle actions when the game is counting down to start
        private void HandleCountingDown(float count)
        {
            // Hide the start and exit buttons
            _startButton.SetActive(false);
            _exitButton.SetActive(false);

            // Set the room title to the countdown timer
            _roomTitleText.SetText(count.ToString("F0"));
        }

        // Method to handle leaving the room, invoked by a UI button
        public void LeaveRoom()
        {
            // Invoke the OnLeaveRoom event
            OnLeaveRoom?.Invoke();
        }

        // Method to handle starting the game, invoked by a UI button
        public void StartGame()
        {
            // Log the start of the game and invoke the OnStartGame event
            Debug.Log("Starting game...");
            OnStartGame?.Invoke();
        }
    }
}
