using UnityEngine;
using UnityEngine.UI;

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
            UIPlayerSelection playerSelection = FindObjectOfType<UIPlayerSelection>();

            // Ensure we found the script
            if (playerSelection != null)
            {
                // Call the SelectCharacter method on the player's UIPlayerSelection script
                playerSelection.SelectCharacter(selectionIndex);

                // Log the updated index and the player's name who clicked it
                Debug.Log($"Player {Photon.Pun.PhotonNetwork.LocalPlayer.NickName} selected character index {selectionIndex}");
            }
            else
            {
                Debug.LogError("No UIPlayerSelection script found on the local player.");
            }
        }
    }
}
