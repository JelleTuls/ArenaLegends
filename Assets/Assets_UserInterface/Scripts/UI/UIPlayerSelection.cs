/*
UIPlayerSelection.cs

Purpose:
    The UIPlayerSelection script is responsible for managing character selection for each player. 
    It controls the UI that allows players to select their characters, tracks their selections, 
    and handles special actions like kicking players from the lobby.

Key functionalities:
    - Character Selection: Allows players to scroll through a set of character portraits and choose one.
    - Player Management: It can kick players from the room if they are not the host.
    - Photon Properties: It updates each player’s custom properties to reflect their character selection 
    and tracks who has been kicked.
    - UI Setup: It customizes the UI for each player, displaying the selected character and allowing only 
    the player to control their own selection.
    - Master Client UI: The master client has the ability to see and use the "Kick Player" button for other 
    players, but not for themselves.

Differences:
    This script is player-centric, focusing on character selection and player-specific actions (like kicking 
    players). It doesn’t manage room states or game start logic like the PhotonRoomController. Instead, it 
    focuses solely on what players can do in the lobby in terms of character choices and actions.
*/

using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using System;

namespace KnoxGameStudios
{
    public class UIPlayerSelection : MonoBehaviourPunCallbacks
    {
        //_____________________________________________________________________________________________________________________
        // VARIABLES:
        //---------------------------------------------------------------------------------------------------------------------
        #region VARIABLES
        #region PROFILE VARIABLES
            [SerializeField] private TMP_Text _usernameText; // PHOTON Variable to hold player's username
            [SerializeField] private Player _owner; // PHOTON Variable to store the reference to a player
        #endregion PROFILE VARIABLES

        #region CHARACTER SELECTION VARIABLES
            [SerializeField] private Image _portraitImage; // Image file used for selection portraits
            [SerializeField] private GameObject _previousButton; // Previous character selection button
            [SerializeField] private GameObject _nextButton; // Next character selection button
            [SerializeField] private GameObject _kickButton; // Remove player from lobby button

            // Add this array of sprites for character selections
            [SerializeField] private Sprite[] profileSprites; // List of sprite images to select from

            [SerializeField] private int _currentSelection; // Currently selected character
        #endregion CHARACTER SELECTION VARIABLES

        #region CONSTANTS
            private const string CHARACTER_SELECTION_NUMBER = "CSN"; // Personal identification number of player (Constant --> doesn't change)
            private const string KICKED_PLAYER = "KICKED"; // Status value for kicked players
        #endregion CONSTANTS

        #region ACTIONS
            public static Action<Player> OnKickPlayer = delegate { }; // Action to play when player gets kicked
        #endregion ACTIONS
        #endregion VARIABLES

        //_____________________________________________________________________________________________________________________
        // RETURN FUNCTIONS:
        //---------------------------------------------------------------------------------------------------------------------
        #region PLAYER/OWNER OBJECT
        public Player Owner // Refers to the player/the owner of a selection
        {
            get { return _owner; } // Gets the Photon player reference
            private set { _owner = value; } // Register the player
        }
        #endregion PLAYER/OWNER OBJECT

        //_____________________________________________________________________________________________________________________
        // GENERAL VOIDS: (CALL FUNCTIONS)
        //---------------------------------------------------------------------------------------------------------------------
        #region PUBLIC VOIDS
        public void Initialize(Player player)
        {
            Debug.Log($"Initializing player selection for {player.NickName}");

            // Set the owner to the player for whom this UI is being initialized
            Owner = player;
            _currentSelection = GetCharacterSelection();
            
            // Set up the UI with player details and character selection
            SetupPlayerSelection();
            UpdateCharacterModel(_currentSelection);  // Update model on initialization
        }
        #endregion PUBLIC VOIDS

        #region PRIVATE VOIDS
        public void SetupPlayerSelection() // Prepare/activate character selection visuals
        {
            _usernameText.SetText(_owner.NickName); // Set the value of: _usernameText equal to the _owner Photon nickname
            _kickButton.SetActive(false); // Deactivate the kick button
            if (PhotonNetwork.LocalPlayer.Equals(Owner)) // Only for the local player!
            {
                _previousButton.SetActive(true); // Activate your selection previous button
                _nextButton.SetActive(true); // Activate your selection next button
            }
            else
            {
                _previousButton.SetActive(false); // If not owner of the selection, deactivate previous button
                _nextButton.SetActive(false); // If not owner of the selection, deactivate next button
            }

            if (PhotonNetwork.IsMasterClient) // If you are the host
            {
                ShowMasterClientUI(); // Activate kick button for the host
            }                
        }

        private void ShowMasterClientUI() // Host can see and use the kick button
        {
            if (!PhotonNetwork.IsMasterClient) return; // Not the host --> Ignore this function!

            if (PhotonNetwork.LocalPlayer.Equals(Owner)) // Don't activate kick button on your own selection visuals
            {
                _kickButton.SetActive(false); // Deactivate kick button on your own profile
            }
            else
            {
                _kickButton.SetActive(true); // On other players' profiles --> Activate kick button
            }                
        }

        private int GetCharacterSelection()
        {
            int selection = 0;  // Default selection value
            object playerSelection;
            if (Owner.CustomProperties.TryGetValue(CHARACTER_SELECTION_NUMBER, out playerSelection))
            {
                selection = (int)playerSelection;
            }
            return selection;
        }

        // Set the CHARACTER_SELECTION_NUMBER equal the the incoming int selection, then UpdateCharacterModel
        public void UpdateCharacterSelection(int selection)
        {
            // Update Photon player properties with the new selection. This can then be collected from the next scene.
            Hashtable playerSelectionProperty = new Hashtable()
            {
                {CHARACTER_SELECTION_NUMBER, selection}
            };

            // This log message is showing!
            Debug.Log($"Updating Photon Custom Property {CHARACTER_SELECTION_NUMBER} for {PhotonNetwork.LocalPlayer.NickName} to {selection}");
            // Set property of local player: playerSelectionProperty
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProperty);

            // Update the UI elements (profile image and character model)
            UpdateCharacterModel(selection);  // Updates character image in the player selection
        }

        private void UpdateCharacterModel(int selection) // Update visuals based on selection.
        {
            if (selection >= 0 && selection < profileSprites.Length)
            {
                // Set the Image.sprite object/component equal to the sprite in the profileSpites list based on the int selection
                _portraitImage.sprite = profileSprites[selection];
                Debug.Log($"Character model updated to selection index: {selection}");
            }
            else
            {
                Debug.LogError("Invalid selection index: " + selection);
            }
        }

        public void KickPlayer() // Function plays after clicking the kick player button.
        {
            Debug.Log($"Updating Photon Custom Property {CHARACTER_SELECTION_NUMBER} for {Owner} to {true}"); // Logging

            Hashtable kickedProperty = new Hashtable() // Create new dictionary to keep track of this particular kicked player
            {
                {KICKED_PLAYER, true} // Add the kicked player to a new dictionary
            };
            Owner.SetCustomProperties(kickedProperty); // Photon function to store the dictionary
        }

        //_____________________________________________________________________________________________________________________
        // OVERRIDE VOIDS (OVERRIDE FUNCTIONS):
        //---------------------------------------------------------------------------------------------------------------------
        #region PHOTON CALLBACK METHODS
        public override void OnMasterClientSwitched(Player newMasterClient) // Provide the "Host UI" to the new Client Master
        {
            if (Owner.Equals(newMasterClient)) // If the player who changes properties equals the Photon Master Clients... So this will only happen for the Client Master
            {
                ShowMasterClientUI(); // Activate/Show the UI specific for the Client Master
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            Debug.Log("Function Called: OnPlayerPropertiesUpdate");
            Debug.Log($"Owner :: {Owner}");
            Debug.Log($"targetPlayer :: {targetPlayer}");
            Debug.Log($"chargedProps :: {changedProps}");

            if (Owner == null)
            {
                Debug.LogError("Owner is null. Cannot proceed with OnPlayerPropertiesUpdate.");
                return;
            }

            if (Owner.Equals(targetPlayer))  // Ensure the update is for the right player
            {
                Debug.Log("Owner equals targetPlayer");
                if (changedProps.ContainsKey(CHARACTER_SELECTION_NUMBER))
                {
                    object selectionObject = changedProps[CHARACTER_SELECTION_NUMBER];
                    if (selectionObject is int selectionIndex)
                    {
                        _currentSelection = selectionIndex;
                        UpdateCharacterModel(_currentSelection);  // Update the player's character model
                    }
                    else
                    {
                        Debug.LogError("CHARACTER_SELECTION_NUMBER property is not an int.");
                    }
                }

                if (changedProps.ContainsKey(KICKED_PLAYER) && (bool)changedProps[KICKED_PLAYER])
                {
                    OnKickPlayer?.Invoke(Owner);
                }
            }
        }
        #endregion PHOTON CALLBACK METHODS
    }
}
#endregion