// Import necessary namespaces
using System;  // Provides basic functionalities and attributes like [Serializable]
using UnityEngine;  // Provides functionalities for Unity game development, including ScriptableObject

// Define a namespace for organizational purposes
namespace KnoxGameStudios
{
    // Apply the [Serializable] attribute to allow this class to be serialized
    // Apply the [CreateAssetMenu] attribute to enable creating instances of this class from the Unity Editor's "Create" menu
    [Serializable]
    [CreateAssetMenu(menuName = "Arena Legends/Photon/Game Mode", fileName = "gameMode")]
    public class GameMode : ScriptableObject
    {
        // Private serialized fields to hold game mode data
        // These fields can be set in the Unity Editor
        [SerializeField] private string _name;  // Name of the game mode
        [SerializeField] private byte _maxPlayers;  // Maximum number of players allowed in this game mode
        [SerializeField] private bool _hasTeams;  // Indicates if the game mode supports teams
        [SerializeField] private int _teamSize;  // Size of each team, if teams are supported

        // Public property for the name of the game mode
        // Allows reading the name but only allows setting it within this class
        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        // Public property for the maximum number of players
        // Allows reading the max players but only allows setting it within this class
        public byte MaxPlayers
        {
            get { return _maxPlayers; }
            private set { _maxPlayers = value; }
        }

        // Public property to indicate if the game mode supports teams
        // Allows reading the teams support status but only allows setting it within this class
        public bool HasTeams
        {
            get { return _hasTeams; }
            private set { _hasTeams = value; }
        }

        // Public property for the team size
        // Allows reading the team size but only allows setting it within this class
        public int TeamSize
        {
            get { return _teamSize; }
            private set { _teamSize = value; }
        }
    }
}
