using UnityEngine;
using Photon.Pun;
using System.Linq; // For LINQ methods like FirstOrDefault

public class Player_Photon_View_Transfer : MonoBehaviour
{
    private PhotonView photonView;

    private void Awake()
    {
        // Ensure that we are referring to the PhotonView component on this GameObject
        photonView = GetComponent<PhotonView>();
        TransferOwnershipBasedOnHierarchy();
    }

    public void TransferOwnershipBasedOnHierarchy()
    {
        // Get the highest parent object in the hierarchy, or use the current object if no parent exists
        Transform highestParent = GetHighestParent(photonView.transform);

        // Extract the player name part from the format "Player_Object_<PlayerName>"
        string[] nameParts = highestParent.name.Split('_');
        if (nameParts.Length >= 3)
        {
            // The player name should be the third part of the string (after "Player_Object_")
            string playerName = nameParts[2];

            // Find the player by the extracted player name
            Photon.Realtime.Player player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.NickName == playerName);

            if (player != null)
            {
                // Transfer the ownership of the photonView to the found player
                photonView.TransferOwnership(player);
                Debug.Log($"Ownership of {photonView.gameObject.name} transferred to {player.NickName}.");
            }
            else
            {
                Debug.LogError($"Player with name {playerName} not found.");
            }
        }
        else
        {
            Debug.LogError("Object name does not follow the format 'Player_Object_<PlayerName>'.");
        }
    }

    // Helper method to get the highest parent in the hierarchy or return itself if there is no parent
    private Transform GetHighestParent(Transform current)
    {
        // Traverse up the hierarchy to find the highest parent
        while (current.parent != null)
        {
            current = current.parent;
        }

        return current;
    }
}
