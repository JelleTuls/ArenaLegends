// Necessary using directives for Photon and Unity functionalities
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KnoxGameStudios
{
    // UITeam class to manage the UI representation of a team in a multiplayer game
    public class UITeam : MonoBehaviour
    {
        // Serialized fields to be assigned in the Unity Editor
        [SerializeField] private int _teamSize;  // Current size of the team
        [SerializeField] private int _maxTeamSize;  // Maximum size of the team
        [SerializeField] private PhotonTeam _team;  // Reference to the PhotonTeam
        [SerializeField] private TMP_Text _teamNameText;  // Text component to display the team name and size
        [SerializeField] private Transform _playerSelectionContainer;  // Container for player selection UI elements
        [SerializeField] private UIPlayerSelection _playerSelectionPrefab;  // Prefab for player selection UI elements
        [SerializeField] private Dictionary<Player, UIPlayerSelection> _playerSelections;  // Dictionary to map players to their UI elements

        // Static Action event to notify when switching to a team
        public static Action<PhotonTeam> OnSwitchToTeam = delegate { };

        // Method called when the script instance is being loaded
        private void Awake()
        {
            // Subscribe to various events related to team and room management
            UIDisplayTeam.OnAddPlayerToTeam += HandleAddPlayerToTeam;
            UIDisplayTeam.OnRemovePlayerFromTeam += HandleRemovePlayerFromTeam;
            PhotonRoomController.OnRoomLeft += HandleLeaveRoom;
        }

        // Method called when the script instance is being destroyed
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks and unexpected behavior
            UIDisplayTeam.OnAddPlayerToTeam -= HandleAddPlayerToTeam;
            UIDisplayTeam.OnRemovePlayerFromTeam -= HandleRemovePlayerFromTeam;
            PhotonRoomController.OnRoomLeft -= HandleLeaveRoom;
        }

        // Method to initialize the team UI with the given team and team size
        public void Initialize(PhotonTeam team, int teamSize)
        {
            _team = team;  // Set the team reference
            _maxTeamSize = teamSize;  // Set the maximum team size
            Debug.Log($"{_team.Name} is added with the size {_maxTeamSize}");
            _playerSelections = new Dictionary<Player, UIPlayerSelection>();  // Initialize the dictionary for player selections
            UpdateTeamUI();  // Update the team UI

            // Get the members of the team and add them to the UI
            Player[] teamMembers;
            if (PhotonTeamsManager.Instance.TryGetTeamMembers(_team.Code, out teamMembers))
            {
                foreach (Player player in teamMembers)
                {
                    AddPlayerToTeam(player);
                }
            }
        }

        // Handler for adding a player to the team
        public void HandleAddPlayerToTeam(Player player, PhotonTeam team)
        {
            if (_team.Code == team.Code)
            {
                Debug.Log($"Updating {_team.Name} UI to add {player.NickName}");
                AddPlayerToTeam(player);
            }
        }

        // Handler for removing a player from the team
        public void HandleRemovePlayerFromTeam(Player player)
        {
            RemovePlayerFromTeam(player);
        }

        // Handler for when the room is left
        private void HandleLeaveRoom()
        {
            Destroy(gameObject);  // Destroy this game object
        }

        // Method to update the team UI with the current team name and size
        private void UpdateTeamUI()
        {
            _teamNameText.SetText($"{_team.Name} \n {_playerSelections.Count} / {_maxTeamSize}");
        }

        // Method to add a player to the team and update the UI
        private void AddPlayerToTeam(Player player)
        {
            // Instantiate a new UIPlayerSelection for the player
            UIPlayerSelection uiPlayerSelection = Instantiate(_playerSelectionPrefab, _playerSelectionContainer);
            uiPlayerSelection.Initialize(player);  // Initialize the UIPlayerSelection with the player
            _playerSelections.Add(player, uiPlayerSelection);  // Add the player and their UI element to the dictionary
            UpdateTeamUI();  // Update the team UI
        }

        // Method to remove a player from the team and update the UI
        private void RemovePlayerFromTeam(Player player)
        {
            if (_playerSelections.ContainsKey(player))
            {
                Debug.Log($"Updating {_team.Name} UI to remove {player.NickName}");
                Destroy(_playerSelections[player].gameObject);  // Destroy the player's UI element
                _playerSelections.Remove(player);  // Remove the player from the dictionary
                UpdateTeamUI();  // Update the team UI
            }
        }

        // Method to handle switching to this team
        public void SwitchToTeam()
        {
            Debug.Log($"Trying to switch to team {_team.Name}");
            if (_teamSize >= _maxTeamSize) return;  // If the team is already full, do nothing

            Debug.Log($"Switching to team {_team.Name}");
            OnSwitchToTeam?.Invoke(_team);  // Invoke the OnSwitchToTeam event
        }
    }
}
