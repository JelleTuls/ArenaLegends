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
        [SerializeField] private TMP_Text _usernameText; // PHOTON Variable to hold player's username
        [SerializeField] private Player _owner; // PHOTON Variable to store the reference to a player

        [SerializeField] private Image _portraitImage; // Image file used for selection portraits
        [SerializeField] private GameObject _kickButton; // Remove player from lobby button

        // Add this array of sprites for character selections
        [SerializeField] private Sprite[] profileSprites; // List of sprite images to select from

        [SerializeField] private int _currentSelection; // Currently selected character

        private const string CHARACTER_SELECTION_NUMBER = "CSN"; // Personal identification number of player (Constant --> doesn't change)
        private const string KICKED_PLAYER = "KICKED"; // Status value for kicked players

        public static Action<Player> OnKickPlayer = delegate { }; // Action to play when player gets kicked


//_____________________________________________________________________________________________________________________
// RETURN FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
        public Player Owner // Refers to the player/the owner of a selection
        {
            get { return _owner; } // Gets the Photon player reference
            private set { _owner = value; } // Register the player
        }


//_____________________________________________________________________________________________________________________
// START FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
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


//_____________________________________________________________________________________________________________________
// SELECTION FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
        public void SetupPlayerSelection() // Prepare/activate character selection visuals
        {
            _usernameText.SetText(_owner.NickName); // Set the value of: _usernameText equal to the _owner Photon nickname
            _kickButton.SetActive(false); // Deactivate the kick button

            if (PhotonNetwork.IsMasterClient) // If you are the host
            {
                ShowMasterClientUI(); // Activate kick button for the host
            }                
        }


        private int GetCharacterSelection()
        {
            int selection = 0;  // Default selection value
            object playerSelectionObj;
            if (Owner.CustomProperties.TryGetValue(CHARACTER_SELECTION_NUMBER, out playerSelectionObj))
            {
                selection = (int)playerSelectionObj;
            }
            return selection;
        }


        // Set the CHARACTER_SELECTION_NUMBER equal the the incoming int selection, then UpdateCharacterModel
        public void UpdateCharacterSelection(int selection)
        {
            // This log message is showing!
            Debug.Log($"Updating Photon Custom Property {CHARACTER_SELECTION_NUMBER} for {PhotonNetwork.LocalPlayer.NickName} to {selection}");
            
            // Update Photon player properties with the new selection. This can then be collected from the next scene.
            Hashtable playerSelectionProperty = new Hashtable()
            {
                {CHARACTER_SELECTION_NUMBER, selection}
            };
            // Set property of local player: playerSelectionProperty
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerSelectionProperty);
            
            // Removed because not in the original script!
            // // Update the UI elements (profile image and character model)
            // UpdateCharacterModel(selection);  // Updates character image in the player selection
        }


        private void UpdateCharacterModel(int selection) // Update visuals based on selection.
        {
            // Set the Image.sprite object/component equal to the sprite in the profileSpites list based on the int selection
            _portraitImage.sprite = profileSprites[selection];
            Debug.Log($"Character model updated to selection index: {selection}");
        }


//_____________________________________________________________________________________________________________________
// MASTER CLIENT FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
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

            if (!Owner.Equals(targetPlayer)) return;

            object characterSelectedNumberObject;
            if (changedProps.TryGetValue(CHARACTER_SELECTION_NUMBER, out characterSelectedNumberObject))
            {
                _currentSelection = (int)characterSelectedNumberObject;
                UpdateCharacterModel(_currentSelection);
            }

            object kickedPlayerObject;
            if (changedProps.TryGetValue(KICKED_PLAYER, out kickedPlayerObject))
            {
                bool kickedPlayer = (bool)kickedPlayerObject;
                if(kickedPlayer)
                {
                    Hashtable kickedProperty = new Hashtable()
                    {
                        {KICKED_PLAYER, false}
                    };
                    Owner.SetCustomProperties(kickedProperty);

                    OnKickPlayer?.Invoke(Owner);
                }
            }
        }



        
    }
}