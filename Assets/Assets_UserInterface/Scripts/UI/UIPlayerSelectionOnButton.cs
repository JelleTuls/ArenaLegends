using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace KnoxGameStudios
{
    public class CharacterSelectButton : MonoBehaviour
    {
        [SerializeField] private int selectionIndex; // The index for this button
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            // Find the UIPlayerSelection script on the local player
            UIPlayerSelection playerSelection = FindLocalPlayerProfile();

            // Check if playerSelection photonView is actual connected to the local player
            if (playerSelection != null && playerSelection.photonView.IsMine)
            {
                // Call the SelectCharacter method on the player's UIPlayerSelection script
                playerSelection.UpdateCharacterSelection(selectionIndex);

                // Log the updated index and the player's name who clicked it
                Debug.Log($"Player {PhotonNetwork.LocalPlayer.NickName} selected character index {selectionIndex}.");
            }
            else
            {
                Debug.LogError("No local UIPlayerSelection script found or PhotonView is not mine.");
            }
        }

        // Helper method to find the local player's UIPlayerSelection component based on their Photon username
        private UIPlayerSelection FindLocalPlayerProfile()
        {
            string expectedProfileName = $"Player_Profile_{PhotonNetwork.LocalPlayer.NickName}";

            GameObject[] playerProfiles = FindObjectsOfType<GameObject>();

            foreach (GameObject profile in playerProfiles)
            {
                if (profile.name == expectedProfileName)
                {
                    UIPlayerSelection selection = profile.GetComponent<UIPlayerSelection>();
                    if (selection != null && selection.photonView.IsMine)
                    {
                        return selection;
                    }
                }
            }
            return null;
        }
    }
}
