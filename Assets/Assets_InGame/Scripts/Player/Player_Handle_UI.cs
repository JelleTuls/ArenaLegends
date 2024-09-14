using UnityEngine;
using TMPro;

public class Player_UI_Display : MonoBehaviour
{
    public TextMeshProUGUI FpsText;
    public TextMeshProUGUI teamText; // Reference to the player's TextMeshPro element for team info

    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        frameCount++;

        if(time >+ pollingTime){
            int frameRate = Mathf.RoundToInt(frameCount / time);
            FpsText.text = frameRate.ToString() + " FPS";

            time -= pollingTime; 
            frameCount = 0;
        }
    }


    // Method to update the team information on the player's UI
    public void SetTeamText(string teamName)
    {
        if (teamText != null)
        {
            teamText.text = "Your Team: " + teamName;
        }
    }
}
