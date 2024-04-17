using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;

//_____________________________________________________________________________________________________________________
//NOTES
//---------------------------------------------------------------------------------------------------------------------
//NOTE: This script connects the player to the Photon Network using a nickName

namespace KnoxGameStudios
{
    public class PhotonConnector : MonoBehaviourPunCallbacks
    {
//_____________________________________________________________________________________________________________________
//VARIABLES
//---------------------------------------------------------------------------------------------------------------------
        [SerializeField] private string nickName;  // Changable variable holding player's nickName (Used for Photon/PlayFab)
        public static Action GetPhotonFriends = delegate { }; // (CUSTOM PLAYFAB ACTION) Invoking get Playfab friends
        public static Action OnLobbyJoined = delegate { };  // (PHOTON ACTION) Invoke any additional methods or events subscribed to OnJoinedLobby

//_____________________________________________________________________________________________________________________
//PRIVATE VOIDS/FUNCTIONS
//--------------------------------------------------------------------------------------------------------------------- 
        #region UNITY PRIVATE VOIDS/FUNCTIONS
            private void Awake() // Plays before start
            {
                nickName = PlayerPrefs.GetString("USERNAME"); // Set nickName to Photon "USERNAME"   
            }
            private void Start() // Plays only at the first frame (start)
            {
                if (PhotonNetwork.IsConnectedAndReady || PhotonNetwork.IsConnected) return; // Check if connected to Photon --> Don't connect again

                ConnectToPhoton(); // Otherwise play ConnecctToPhoton function
            }
        #endregion UNITY PRIVATE VOIDS/FUNCTIONS

        #region PRIVATE VOIDS/FUNCTIONS
            private void ConnectToPhoton() // Private function: Connect to Photon
            {
                Debug.Log($"Connect to Photon as {nickName}"); // Logging
                PhotonNetwork.AuthValues = new AuthenticationValues(nickName); // Set authentication information to our set nickName
                PhotonNetwork.AutomaticallySyncScene = true; // Synchronize current scene
                PhotonNetwork.NickName = nickName; // Set the Photon NickName equal to nickName variable
                PhotonNetwork.ConnectUsingSettings(); // Connect to Photon using the above settings
            }        
        #endregion PRIVATE VOIDS/FUNCTIONS

//_____________________________________________________________________________________________________________________
//OVERRIDE VOIDS/FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------        
        #region OVERRIDE FUNCTIONS
            public override void OnConnectedToMaster() // Override function which plays when connected to the Photon Master Server (No hosted server!!)
            {
                Debug.Log("You have connected to the Photon Master Server"); // Logging
                if (!PhotonNetwork.InLobby) // If not yet in the master lobby
                {
                    PhotonNetwork.JoinLobby(); // Connect to the master lobby
                }
            }
            public override void OnJoinedLobby() // Override function which plays when joined a lobby
            {
                Debug.Log("You have connected to a Photon Lobby");
                Debug.Log("Invoking get Playfab friends");
                GetPhotonFriends?.Invoke(); // (CUSTOM PLAYFAB ACTION) Get all friends from the PlayFab database
                OnLobbyJoined?.Invoke(); // Invoke any additional methods or events subscribed to OnJoinedLobby
            }
        #endregion OVERRIDE FUNCTIONS
    }
}