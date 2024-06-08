// Import necessary namespaces
using System;  // Provides basic functionalities like the Action delegate
using UnityEngine;  // Provides functionalities for Unity game development

// Define a namespace for organizational purposes
namespace KnoxGameStudios
{
    // Define a class UIGameMode that inherits from MonoBehaviour
    // MonoBehaviour is the base class from which every Unity script derives
    public class UIGameMode : MonoBehaviour
    {
        // A serialized private field to hold a reference to a GameMode object
        // This will allow you to set this field in the Unity Editor
        [SerializeField] private GameMode _gameMode;

        // A static Action delegate that takes a GameMode parameter
        // This will be used to notify listeners when a game mode is selected
        public static Action<GameMode> OnGameModeSelected = delegate { };

        // A method to handle the selection of a game mode
        public void SelectGameMode()
        {
            // Check if _gameMode is null to avoid null reference exceptions
            if (_gameMode == null) return;

            // Invoke the OnGameModeSelected event and pass the selected _gameMode
            OnGameModeSelected?.Invoke(_gameMode);
        }
    }
}
