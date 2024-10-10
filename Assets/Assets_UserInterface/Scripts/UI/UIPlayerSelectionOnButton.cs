using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace KnoxGameStudios
{
    public class CharacterSelectButton : MonoBehaviour
    {
//_____________________________________________________________________________________________________________________
// VARIABLES
//---------------------------------------------------------------------------------------------------------------------
        // INTs
        [SerializeField] private int selectionIndex; // The index for this button

        // BUTTONS
        private Button button;


//_____________________________________________________________________________________________________________________
// START FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------
        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }


//_____________________________________________________________________________________________________________________
// BUTTON FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------
        private void OnButtonClick()
        {
            // Find the UIPlayerSelection script associated with the local player
            UIPlayerSelection playerSelection = FindLocalPlayerProfile();

            // Check if we found the correct UIPlayerSelection for the local player
            if (playerSelection != null)
            {
                // Call the UpdateCharacterSelection method on the player's UIPlayerSelection script
                playerSelection.UpdateCharacterSelection(selectionIndex);

                // Log the updated index and the player's name who clicked it
                Debug.Log($"Player {PhotonNetwork.LocalPlayer.NickName} selected character index {selectionIndex}.");
            }
            else
            {
                Debug.LogError("No local UIPlayerSelection script found for the current player.");
            }
        }


//_____________________________________________________________________________________________________________________
// SUPPORTING FUNCTIONS
//---------------------------------------------------------------------------------------------------------------------
        // Helper method to find the local player's UIPlayerSelection component based on their Photon username
        private UIPlayerSelection FindLocalPlayerProfile()
        {
            string expectedProfileName = $"Player_Profile_{PhotonNetwork.LocalPlayer.NickName}";

            // Find all objects in the scene with UIPlayerSelection component
            UIPlayerSelection[] playerSelections = FindObjectsOfType<UIPlayerSelection>();

            foreach (UIPlayerSelection selection in playerSelections)
            {
                // Check if this UIPlayerSelection object matches the local player's nickname
                if (selection.Owner.NickName == PhotonNetwork.LocalPlayer.NickName)
                {
                    return selection; // Found the correct UI for the local player
                }
            }

            return null; // Return null if no matching profile is found
        }
    }
}
