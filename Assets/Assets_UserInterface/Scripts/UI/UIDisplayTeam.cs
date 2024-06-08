using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnoxGameStudios
{
    public class UIDisplayTeam : MonoBehaviour
    {
        [SerializeField] private UITeam _uiTeamPrefab;
        [SerializeField] private List<UITeam> _uiTeams;
        [SerializeField] private Transform _teamContainer;

        public static Action<Player, PhotonTeam> OnAddPlayerToTeam = delegate { };
        public static Action<Player> OnRemovePlayerFromTeam = delegate { };

        private void Awake()
        {
            PhotonTeamController.OnCreateTeams += HandleCreateTeams;
            PhotonTeamController.OnSwitchTeam += HandleSwitchTeam;
            PhotonTeamController.OnRemovePlayer += HandleRemovePlayer;
            PhotonTeamController.OnClearTeams += HandleClearTeams;
            _uiTeams = new List<UITeam>();
        }

        private void OnDestroy()
        {
            PhotonTeamController.OnCreateTeams -= HandleCreateTeams;
            PhotonTeamController.OnSwitchTeam += HandleSwitchTeam;
            PhotonTeamController.OnRemovePlayer += HandleRemovePlayer;
            PhotonTeamController.OnClearTeams -= HandleClearTeams;
        }

        private void HandleCreateTeams(List<PhotonTeam> teams, GameMode gameMode)
        {
            // Find the placeholders at the start
            Transform placeholder1 = _teamContainer.Find("Placeholder_Team1");
            Transform placeholder2 = _teamContainer.Find("Placeholder_Team2");

            // Counter to keep track of how many UITeams have been instantiated
            int instantiatedTeamsCount = 0;

            foreach (PhotonTeam team in teams)
            {
                // Ensure no more than 2 UITeams are instantiated
                if (instantiatedTeamsCount >= 2)
                {
                    Debug.Log("Maximum number of UITeams instantiated.");
                    break;
                }

                // Instantiate the UITeam
                UITeam uiTeam = Instantiate(_uiTeamPrefab, _teamContainer);                
                uiTeam.Initialize(team, gameMode.TeamSize);
                _uiTeams.Add(uiTeam);

                // Replace placeholders
                if (placeholder1 != null)
                {
                    // Set the parent of the UITeam to the same parent as the placeholder
                    uiTeam.transform.SetParent(placeholder1.parent, false);
                    // Set the position and other properties to match the placeholder
                    uiTeam.transform.SetSiblingIndex(placeholder1.GetSiblingIndex());
                    uiTeam.transform.position = placeholder1.position;
                    uiTeam.transform.rotation = placeholder1.rotation;
                    uiTeam.transform.localScale = placeholder1.localScale;
                    // Destroy the placeholder
                    Destroy(placeholder1.gameObject);
                    // Set placeholder1 to null to indicate it's been used
                    placeholder1 = null;
                    Debug.Log("Replaced Placeholder_Team1");
                }
                else if (placeholder2 != null)
                {
                    // Set the parent of the UITeam to the same parent as the placeholder
                    uiTeam.transform.SetParent(placeholder2.parent, false);
                    // Set the position and other properties to match the placeholder
                    uiTeam.transform.SetSiblingIndex(placeholder2.GetSiblingIndex());
                    uiTeam.transform.position = placeholder2.position;
                    uiTeam.transform.rotation = placeholder2.rotation;
                    uiTeam.transform.localScale = placeholder2.localScale;
                    // Destroy the placeholder
                    Destroy(placeholder2.gameObject);
                    // Set placeholder2 to null to indicate it's been used
                    placeholder2 = null;
                    Debug.Log("Replaced Placeholder_Team2");
                }

                // Increment the counter
                instantiatedTeamsCount++;
            }
        }


        private void HandleSwitchTeam(Player player, PhotonTeam newTeam)
        {
            Debug.Log($"Updating UI to move {player.NickName} to {newTeam.Name}");
            
            OnRemovePlayerFromTeam?.Invoke(player);
            
            OnAddPlayerToTeam?.Invoke(player, newTeam);            
        }

        private void HandleRemovePlayer(Player otherPlayer)
        {
            OnRemovePlayerFromTeam?.Invoke(otherPlayer);
        }

        private void HandleClearTeams()
        {
            foreach (UITeam uiTeam in _uiTeams)
            {
                Destroy(uiTeam.gameObject);
            }
            _uiTeams.Clear();
        }
    }
}