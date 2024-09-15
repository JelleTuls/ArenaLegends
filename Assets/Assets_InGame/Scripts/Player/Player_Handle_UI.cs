using TMPro;
using UnityEngine;

namespace CJ
{
    public class Player_Handle_UI : MonoBehaviour
    {
        public TextMeshProUGUI teamText; // Reference to the TextMeshPro UI element

        public void SetTeamText(string teamName)
        {
            Debug.Log("Playing Function: SetTeamText");
            if (teamText != null)
            {
                if (!string.IsNullOrEmpty(teamName))
                {
                    teamText.text = "Your Team: " + teamName;
                }
                else
                {
                    teamText.text = "No Team Found"; // Default message if no team is assigned
                }
            }
            else
            {
                Debug.LogError("Team Text is not assigned in the Player_Handle_UI script.");
            }
        }
    }
}
