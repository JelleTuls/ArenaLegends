using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//_____________________________________________________________________________________________________________________
// NOTES
//---------------------------------------------------------------------------------------------------------------------
// NOTE: 
//              General Variables are currently not in use!

namespace CJ
{
    public class Player_Handle_Stats : MonoBehaviour
    {
        //_____________________________________________________________________________________________________________________
        // GENERAL VARIABLES:
        //---------------------------------------------------------------------------------------------------------------------
        #region GENERAL VARIABLES
        #region Health Variables
            public float myHealth; // The current health of the player
            public float myMaxHealth = 100f; // The maximum health of the player
        #endregion Health Variables

        #region Visual Variables
            public Sprite myPortrait; // Used for displaying player portrait in UI
            public Sprite myClass; // Used for displaying player class icon in UI
        #endregion Visual Variables

        #region UpdateHealthUI Variables
            public Image frontHealthBar; // UI Image object for the UI health bar (shown in HUD)
            public Image floatingHealthBar; // UI Image object for the floating health bar (above player in scene)
        #endregion UpdateHealthUI Variables

        #region Unused Variables
            // Placeholder for future use or unimplemented features
            // public GameObject playerObject;
            // public float shieldReduction = 2.0f;
            // public Image frontEnergyBar;
            // public float chipSpeed = 2f;
        #endregion Unused Variables
        #endregion GENERAL VARIABLES

        //_____________________________________________________________________________________________________________________
        // GENERAL VOIDS:
        //---------------------------------------------------------------------------------------------------------------------
        #region GENERAL VOIDS
        void Start()
        {
            myHealth = myMaxHealth; // Initialize with full health
        }

        void Update()
        {
            myHealth = Mathf.Clamp(myHealth, 0, myMaxHealth); // Ensure health is clamped between 0 and max health
            UpdateHealthUI(); // Update health bar visuals based on current health
        }
        #endregion GENERAL VOIDS

        //_____________________________________________________________________________________________________________________
        // FUNCTIONS:
        //---------------------------------------------------------------------------------------------------------------------
        #region UpdateHealthUI Variables
        public void UpdateHealthUI() // Function to update visual health bars when health changes
        {
            float hFraction = myHealth / myMaxHealth; // Calculate health percentage

            // Update the front health bar UI if assigned
            if (frontHealthBar != null)
            {
                frontHealthBar.fillAmount = hFraction; // Set new fill amount for frontHealthBar (HUD)
            }
            else
            {
                Debug.LogWarning("Front Health Bar UI is not assigned."); // Warn if the front health bar is missing
            }

            // Update the floating health bar UI if assigned
            if (floatingHealthBar != null)
            {
                floatingHealthBar.fillAmount = hFraction; // Set new fill amount for floatingHealthBar (above player)
            }
            else
            {
                Debug.LogWarning("Floating Health Bar UI is not assigned."); // Warn if the floating health bar is missing
            }
        }

        public void TakeDamage(float damage) // Function to decrease health based on incoming damage
        {
            myHealth -= damage; // Reduce health by damage value
        }

        public void RestoreHealth(float healAmount) // Function to increase health based on incoming healing
        {
            myHealth += healAmount; // Increase health by heal amount
        }
        #endregion UpdateHealthUI Variables
    }
}
