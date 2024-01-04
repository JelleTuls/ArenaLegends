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
        [SerializeField] private string nickName; 
        public static Action GetPhotonFriends = delegate { };
        public static Action OnLobbyJoined = delegate { };

//_____________________________________________________________________________________________________________________
//PRIVATE VOIDS/FUNCTIONS
//--------------------------------------------------------------------------------------------------------------------- 
        #region UNITY PRIVATE VOIDS/FUNCTIONS
        private void Awake()
        {
            nickName = PlayerPrefs.GetString("USERNAME");            
        }
        private void Start()
        {
            if (PhotonNetwork.IsConnectedAndReady || PhotonNetwork.IsConnected) return;

            ConnectToPhoton();
        }
        #endregion UNITY PRIVATE VOIDS/FUNCTIONS

        #region PRIVATE VOIDS/FUNCTIONS
            private void ConnectToPhoton()
            {
                Debug.Log($"Connect to Photon as {nickName}");
                PhotonNetwork.AuthValues = new AuthenticationValues(nickName);
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.NickName = nickName;
                PhotonNetwork.ConnectUsingSettings();
            }        
        #endregion PRIVATE VOIDS/FUNCTIONS

//_____________________________________________________________________________________________________________________
//OVERRIDE VOIDS/FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------        
        #region OVERRIDE FUNCTIONS
            public override void OnConnectedToMaster()
            {
                Debug.Log("You have connected to the Photon Master Server");
                if (!PhotonNetwork.InLobby)
                {
                    PhotonNetwork.JoinLobby();
                }
            }
            public override void OnJoinedLobby()
            {
                Debug.Log("You have connected to a Photon Lobby");
                Debug.Log("Invoking get Playfab friends");
                GetPhotonFriends?.Invoke();
                OnLobbyJoined?.Invoke();
            }
        #endregion OVERRIDE FUNCTIONS
    }
}