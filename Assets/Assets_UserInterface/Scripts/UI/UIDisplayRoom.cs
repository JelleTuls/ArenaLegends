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

namespace KnoxGameStudios
{
    public class UIDisplayRoom : MonoBehaviourPunCallbacks
    {
         public static UIDisplayRoom Instance { get; private set; }
        
        [SerializeField] private TMP_Text _roomTitleText;
        [SerializeField] private GameObject _startButton;
        [SerializeField] private GameObject _readyButton;
        [SerializeField] private GameObject _exitButton;
        [SerializeField] private GameObject _roomContainer;

        public static Action OnStartGame = delegate { };
        public static Action OnLeaveRoom = delegate { };

        private const string PLAYER_READY = "PlayerReady"; // Key for CustomProperties to store ready status

        private PhotonView photonView;

        private void Awake()
        {
            if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject); // Prevents multiple instances
        }
        else
        {
            Instance = this;
        }
        
            // Ensure that the PhotonView is attached and accessible
            photonView = GetComponent<PhotonView>();
            if (photonView == null)
            {
                Debug.LogError("PhotonView component is missing from UIDisplayRoom.");
            }

            PhotonRoomController.OnJoinRoom += HandleJoinRoom;
            PhotonRoomController.OnRoomLeft += HandleRoomLeft;
            PhotonRoomController.OnMasterOfRoom += HandleMasterOfRoom;
            PhotonRoomController.OnCountingDown += HandleCountingDown;
        }

        private void OnDestroy()
        {
            PhotonRoomController.OnJoinRoom -= HandleJoinRoom;
            PhotonRoomController.OnRoomLeft -= HandleRoomLeft;
            PhotonRoomController.OnMasterOfRoom -= HandleMasterOfRoom;
            PhotonRoomController.OnCountingDown -= HandleCountingDown;
        }

        private void HandleJoinRoom(GameMode gameMode)
        {
            _roomTitleText.SetText(PhotonNetwork.CurrentRoom.CustomProperties["GAMEMODE"].ToString());
            _exitButton.SetActive(true);
            _roomContainer.SetActive(true);
            ShowReadyButton();
        }

        private void HandleRoomLeft()
        {
            _startButton.SetActive(false);
            _readyButton.SetActive(false);
            _exitButton.SetActive(false);
            _roomTitleText.SetText("JOINING ROOM");
        }

        private void HandleMasterOfRoom(Player masterPlayer)
        {
            ShowReadyButton();
        }

        private void HandleCountingDown(float count)
        {
            _startButton.SetActive(false);
            _readyButton.SetActive(false);
            _exitButton.SetActive(false);
            _roomTitleText.SetText(count.ToString("F0"));
        }

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
            if (photonView == null)
            {
                Debug.LogError("PhotonView is null in ReadyUp function.");
                return;
            }

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

            // Notify the master client that this player is ready
            photonView.RPC("NotifyMasterClientReady", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.NickName);
        }

        [PunRPC]
        private void NotifyMasterClientReady(string playerName)
        {
            // Log the player that clicked ready on the master client
            Debug.Log($"{playerName} is ready.");
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

        // public override void OnPlayerEnteredRoom(Player newPlayer)
        // {
        //     base.OnPlayerEnteredRoom(newPlayer);
        //     Debug.Log($"Player joined: {newPlayer.NickName}");
        //     ShowReadyButton();
        // }

        // public override void OnPlayerLeftRoom(Player otherPlayer)
        // {
        //     base.OnPlayerLeftRoom(otherPlayer);
        //     Debug.Log($"Player left: {otherPlayer.NickName}");
        // }
    }
}
