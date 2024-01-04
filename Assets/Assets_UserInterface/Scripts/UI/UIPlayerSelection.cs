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
            [SerializeField] private Player _owner; // PHOTON Variable to store the reference to the a player
        #endregion PROFILE VARIABLES

        #region CHARACTER SELECTION VARIABLES
            [SerializeField] private Image _portraitImage; // Image file used for selection portraits
            [SerializeField] private GameObject _previousButton; // Previous character selection button
            [SerializeField] private GameObject _nextButton; // Next character selection button
            [SerializeField] private GameObject _kickButton; // Remove player from lobby button
            [SerializeField] private Sprite[] _sprites; // List of sprite images to select from
            [SerializeField] private int _currentSelection; // Currently selected character
        #endregion CHARACTER SELECTION VARIABLES

        #region CONSTANTS
            private const string CHARACTER_SELECTION_NUMBER = "CSN"; // Personal identification number of player (Constant --> doesn't change)
            private const string KICKED_PLAYER = "KICKED"; // Status value for kicked players
        #endregion CONSTANTS

        #region ACTIONS
            public static Action<Player> OnKickPlayer = delegate { }; // Action to play when playet gets kicked (STILL FIGURE OUT WHERE THIS ACTION IS LISTED)
        #endregion ACTIONS
    #endregion VARIABLES

//_____________________________________________________________________________________________________________________
// RETURN FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
        #region PLAYER/OWNER OBJECT
            public Player Owner // Refers to the player/the owner of a selection
            {
                get { return _owner; } // Gets the photon value of a player
                private set { _owner = value; } // Register the player (I guess?)
            }
        #endregion PLAYER/OWNER OBJECT

        #region CHARACTER SELECTION INTEGER VALUE
            private int GetCharacterSelection() // Holds the selected integer value refering to the selected character
            {
                int selection = 0; // Default value is 0 (first in the list)
                object playerSelectionObj; // Variable to hold the selected object (Equals to the integer in the list)
                if (Owner.CustomProperties.TryGetValue(CHARACTER_SELECTION_NUMBER, out playerSelectionObj)) // If CHARACTER_SELECTION_NUMBER exist --> Give the playerSelectionObj
                {
                    selection = (int)playerSelectionObj; // Get the selected object equal to the integer from character selection
                }
                return selection; // Function returns the selection value
            }
        #endregion CHARACTER SELECTION INTEGER VALUE

//_____________________________________________________________________________________________________________________
// GENERAL VOIDS: (CALL FUNCTIONS)
//---------------------------------------------------------------------------------------------------------------------
    #region PUBLIC VOIDS
        public void Initialize(Player player) // Function activated when player first time joins selection lobby. Initialize!
        {
            Debug.Log($"Player Selection Init {player.NickName}"); // Logging
            Owner = player; // Set owner value to: player (Photon reference)
            _currentSelection = GetCharacterSelection(); // Set _currentSelection value to: return from function: GetCharacterSelection() ==> 
            SetupPlayerSelection(); // Prepare/activate character selection visuals
            UpdateCharacterModel(_currentSelection); // Update character image/model based on selection integer
        }

        public void PreviousSelection() // After click previous button --> Get previous item in list
        {
            _currentSelection--; // Get the current value of _currentSelection --> Then substract 1
            if (_currentSelection < 0) // If -1 ends up lower than 0
            {
                _currentSelection = _sprites.Length - 1; // Jump to the the very last item of the list
            }
            UpdateCharacterSelection(_currentSelection); // Update the character selection based on the new selection value
        }

        public void NextSelection() // After click next button --> Get previous item in list
        {
            _currentSelection++; // Get the current value of _currentSelection --> Then add 1
            if (_currentSelection > _sprites.Length - 1) // If +1 ends up higher than the last item in the list
            {
                _currentSelection = 0; // Jump to the beginning of the list
            }
            UpdateCharacterSelection(_currentSelection); // Update the character selection based on the new selection value
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
    #endregion PUBLIC VOIDS

    #region PRIVATE VOIDS
        private void SetupPlayerSelection() // Prepare/activate character selection visuals
        {
            _usernameText.SetText(_owner.NickName); // Set the value of: _usernameText equal to the _owner photon nickname
            _kickButton.SetActive(false); // De-activate the kick button
            if (PhotonNetwork.LocalPlayer.Equals(Owner)) // Only for the local player!
            {
                _previousButton.SetActive(true); // Activate your selection previous button
                _nextButton.SetActive(true); // Activate your selection next button
            }
            else
            {
                _previousButton.SetActive(false); // If not owner of the selection, de-activate previous button
                _nextButton.SetActive(false); // If not owner of the selection, de-activate next button
            }

            if(PhotonNetwork.IsMasterClient) // If you are the host
            {
                ShowMasterClientUI(); // Activate kick button for the host
            }                
        }

        private void ShowMasterClientUI() // Host can see and use the kick button
        {
            if (!PhotonNetwork.IsMasterClient) return; // Not the host --> Ignore this function!

            if (PhotonNetwork.LocalPlayer.Equals(Owner)) // Don't activate kick button on your own selection visuals
            {
                _kickButton.SetActive(false); // De-activate kick button on your own profile
            }
            else
            {
                _kickButton.SetActive(true); // On other players' profiles --> Activate kick button
            }                
        }

        private void UpdateCharacterSelection(int selection) // Function to instantly update the character selections. 
        {
            Debug.Log($"Updating Photon Custom Property {CHARACTER_SELECTION_NUMBER} for {PhotonNetwork.LocalPlayer.NickName} to {selection}"); // Logging

            Hashtable playerSelectionProperty = new Hashtable() // Create new dictionary for the player's selection
            {
                {CHARACTER_SELECTION_NUMBER, selection} // # of player & player's selection (# of player means Player1, Player2, Player3 etc.)
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProperty); // Store the updated selection to the player's local profile
        }

        private void UpdateCharacterModel(int selection) // Update visuals based on selection.
        {
            _portraitImage.sprite = _sprites[selection]; // Set _portraitImage to the correct sprite based on the selection integer
        }
    #endregion PRIVATE VOIDS

//_____________________________________________________________________________________________________________________
// OVERRIDE VOIDS (OVERRIDE FUNCTIONS):
//---------------------------------------------------------------------------------------------------------------------
// Override voids/methods/functions are voids which activate when a certain function is activated externally. 
// For example: 
//      Whenever MasterClientSwitched is activated from --> 
//      It overrides the current situation with the outcome of the override function.
    #region PHOTON CALLBACK METHODS
        public override void OnMasterClientSwitched(Player newMasterClient) // Provide the "Host UI" to the new Client Master
        {
            if(Owner.Equals(newMasterClient)) // If the player who changes properties equals the Photon Master Clients... So this will only happens for the Client Master
            {
                ShowMasterClientUI(); // Activate/Show the UI specific for the Client Master
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) // Whenever someone updates it's visuals/properties/selection
        {
            if (!Owner.Equals(targetPlayer)) return; // Make sure changes only occur for the incomming player who changes properties

            object characterSelectedNumberObject; // Create local object variable to store the selected character
            if (changedProps.TryGetValue(CHARACTER_SELECTION_NUMBER, out characterSelectedNumberObject)) // changedProps is an incomming dictionary! --> From this: try to get key: CHARACTER_SELECTION_NUMBER IF TRUE/EXISTS --> Pass the value characterSelectionObject
            {
                _currentSelection = (int)characterSelectedNumberObject; // Set variable _currentSelection equal to the #th item in characterSelectedNumberObject
                UpdateCharacterModel(_currentSelection); // Update the player's visual character to the new selection.
            }

            // NOTE: Whether a player is kicked or not is locally stored in the player's Photon props!!
            // I believe the following check with every change whether the player is a kicked player. 
            //      First it changes it to false
            //      If it has been kicked already --> Return value to TRUE == Kicked!

            object kickedPlayerObject; // Create local object variable to store whether the player is kicked
            if (changedProps.TryGetValue(KICKED_PLAYER, out kickedPlayerObject)) // changedProps is an incomming dictionary! --> From this: try to get key: KICKED_PLAYER IF TRUE/EXISTS --> Pass the value kickedPlayerObject
            {
                bool kickedPlayer = (bool)kickedPlayerObject; // Set bool kickedPlayer equal to the incomming bool (TRUE OR FALSE)
                if(kickedPlayer) // IF TRUE --> If the player is kicked
                {
                    Hashtable kickedProperty = new Hashtable() // Create new hashtable (dictionary)
                    {
                        {KICKED_PLAYER, false} // Key = KICKED_PLAYER, Value = false
                    };
                    Owner.SetCustomProperties(kickedProperty); // change the properties(kickedProperty) of the player who changed properties

                    OnKickPlayer?.Invoke(Owner); // If the player should be kicked --> Set kicked back to TRUE
                }
            }
        }
    #endregion PHOTON CALLBACK METHODS
    }
}