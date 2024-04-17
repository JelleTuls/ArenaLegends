using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

//_____________________________________________________________________________________________________________________
//NOTES
//---------------------------------------------------------------------------------------------------------------------
//NOTE: 

namespace KnoxGameStudios
{
    public class PhotonTeamController : MonoBehaviourPunCallbacks
    {

//_____________________________________________________________________________________________________________________
//GENERAL VARIABLES:
//---------------------------------------------------------------------------------------------------------------------
        [SerializeField] private List<PhotonTeam> _roomTeams; // List to store teams in the room
        [SerializeField] private int _teamSize; // Variable to set max size of each team
        [SerializeField] private PhotonTeam _priorTeam; // Variable to store the previous team when swapping teams

        public static Action<List<PhotonTeam>, GameMode> OnCreateTeams = delegate { }; // Action <-- when creating a team
        public static Action<Player, PhotonTeam> OnSwitchTeam = delegate { }; // Action <-- When switching team
        public static Action<Player> OnRemovePlayer = delegate { }; // Action <-- When player got removed from lobby
        public static Action OnClearTeams = delegate { }; // Action <-- When fully clear a team/all teams


//_____________________________________________________________________________________________________________________
//GENERAL VOIDS:
//---------------------------------------------------------------------------------------------------------------------
        private void Awake() // WHEN TEAM MANAGER OBJECT GOT INSTANTIATED
        {
            // Subscribe to PhotonRoomController events for room-related actions
            UITeam.OnSwitchToTeam += HandleSwitchTeam; 
            PhotonRoomController.OnJoinRoom += HandleCreateTeams;
            PhotonRoomController.OnRoomLeft += HandleLeaveRoom;
            PhotonRoomController.OnOtherPlayerLeftRoom += HandleOtherPlayerLeftRoom;

            _roomTeams = new List<PhotonTeam>(); // Initialize the list of room teams
        }

        private void OnDestroy() // WHEN TEAM MANAGER OBJECT (LOBBY/ROOM) GOT DESTROYED
        {
            // Unsubscribe from the UITeam event for team switching
            UITeam.OnSwitchToTeam -= HandleSwitchTeam;
            PhotonRoomController.OnJoinRoom -= HandleCreateTeams;
            PhotonRoomController.OnRoomLeft -= HandleLeaveRoom;
            PhotonRoomController.OnOtherPlayerLeftRoom -= HandleOtherPlayerLeftRoom;
        }


//_____________________________________________________________________________________________________________________
//PUBLIC & PRIVATE VOIDS/FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------   
        

        //_____________________________________________________________________________________________________________________
        //TEAM MANAGEMENT
        //--------------------------------------------------------------------------------------------------------------------- 
        private void CreateTeams(GameMode gameMode) // Create Teams (used in HandleCreateTeams)
        {
            _teamSize = gameMode.TeamSize; // Set team size based on gameMode object settings
            int numberOfTeams = gameMode.MaxPlayers; // Set numberOfTeams equal gameMode object settings (Default value)
            if (gameMode.HasTeams) // Check if the game mode is team based (Photon Function)
            {
                numberOfTeams = gameMode.MaxPlayers / gameMode.TeamSize; // Calculate numberOfTeams (MaxPlayers / TeamSize)
            }

            // This loop is creating numberOfTeams instances of the PhotonTeam class and 
            // adding them to the _roomTeams list. Each team is given a unique name based 
            // on the loop counter i and is assigned a code corresponding to its position in the loop.
            for (int i = 1; i <= numberOfTeams; i++) // 
            {
                _roomTeams.Add(new PhotonTeam
                {
                    Name = $"Team {i}",
                    Code = (byte)i
                });
            }
        }

        private void HandleCreateTeams(GameMode gameMode) // Handles the creations of teams when joining a room
        {
            CreateTeams(gameMode); // Play function CreateTeams()

            OnCreateTeams?.Invoke(_roomTeams, gameMode); // Invoke the event to notify about team creation

            AutoAssignPlayerToTeam(PhotonNetwork.LocalPlayer, gameMode); // Auto-assign the local player to a team
        }
        
        private void HandleSwitchTeam(PhotonTeam newTeam) // Handling players switching to different teams
        {            
            if (PhotonNetwork.LocalPlayer.GetPhotonTeam() == null) // If player is not in a team yet
            {
                _priorTeam = PhotonNetwork.LocalPlayer.GetPhotonTeam(); // Store current team as previous team
                PhotonNetwork.LocalPlayer.JoinTeam(newTeam); // Join new team (From no team)            
            }
            else if (CanSwitchToTeam(newTeam)) // If there is space in the new team
            {
                _priorTeam = PhotonNetwork.LocalPlayer.GetPhotonTeam(); // Store current team as previous team
                PhotonNetwork.LocalPlayer.SwitchTeam(newTeam); // Join new team (Switching from priorTeam)              
            }
        }

        private void AutoAssignPlayerToTeam(Player player, GameMode gameMode) // Automatically assign a player to any available team
        {
            foreach (PhotonTeam team in _roomTeams) // Loop through all teams
            {
                int teamPlayerCount = PhotonTeamsManager.Instance.GetTeamMembersCount(team.Code); // Get the current player count in the team

                if (teamPlayerCount < gameMode.TeamSize) // Check if max players in team is reached
                {
                    Debug.Log($"Auto assigned {player.NickName} to {team.Name}"); // Logging
                    if (player.GetPhotonTeam() == null) // If player is not in a team yet
                    {
                        player.JoinTeam(team.Code); // Join team
                    }
                    else if (player.GetPhotonTeam().Code != team.Code) // If player is already in a team
                    {
                        player.SwitchTeam(team.Code); // Switch team
                    }
                    break;
                }
            }
        } 

        //_____________________________________________________________________________________________________________________
        //(BOOLEAN)
        //--------------------------------------------------------------------------------------------------------------------- 
        private bool CanSwitchToTeam(PhotonTeam newTeam) // Boolean variable function CanSwitchToTeam
        {
            bool canSwitch = false; // Start bool at FALSE

            if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Code != newTeam.Code) // Check if player is NOT already in the new team
            {
                Player[] players = null; // Set emplty list Players
                if (PhotonTeamsManager.Instance.TryGetTeamMembers(newTeam.Code, out players)) // Attempt to retrieve the members of the specified team from the PhotonTeamsManager.
                {
                    if (players.Length < _teamSize) // If there is still space in the team
                    {
                        canSwitch = true; // Set TRUE
                    }
                    else
                    {
                        Debug.Log($"{newTeam.Name} is full"); // Logging
                    }
                }
            }
            else
            {
                Debug.Log($"You are already on the team {newTeam.Name}"); // Log if you are already on the team
            }

            return canSwitch; // Return bool
        }
        

        //_____________________________________________________________________________________________________________________
        //LEAVE & JOIN ROOM
        //--------------------------------------------------------------------------------------------------------------------- 
        private void HandleLeaveRoom() // Handling what happens when the player itself leaves the room
        {
            PhotonNetwork.LocalPlayer.LeaveCurrentTeam(); // Register the leave in Photon
            _roomTeams.Clear(); // Clear team related data
            _teamSize = 0; // Set team size to 0 (Local of player)
            OnClearTeams?.Invoke(); // Invoke the event/action to notify about cleaning teams
        }

        private void HandleOtherPlayerLeftRoom(Player otherPlayer) // Handling what happens when another player leaves ther oom
        {
            OnRemovePlayer?.Invoke(otherPlayer); // Invoke the event/action which notifies the room a player has left
        }
        

//_____________________________________________________________________________________________________________________
//OVERRIDE VOIDS/FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------  

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) // 
        {
            object teamCodeObject;
            if (changedProps.TryGetValue(PhotonTeamsManager.TeamPlayerProp, out teamCodeObject))
            {
                if (teamCodeObject == null) return;

                byte teamCode = (byte)teamCodeObject;
                
                PhotonTeam newTeam;
                if(PhotonTeamsManager.Instance.TryGetTeamByCode(teamCode, out newTeam))
                {
                    Debug.Log($"Switching {targetPlayer.NickName} to new team {newTeam.Name}");
                    OnSwitchTeam?.Invoke(targetPlayer, newTeam);
                }
            }
        }
       
       

    }
}