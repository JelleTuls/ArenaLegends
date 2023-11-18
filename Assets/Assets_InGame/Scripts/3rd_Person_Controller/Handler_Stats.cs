using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//_____________________________________________________________________________________________________________________
//NOTES
//---------------------------------------------------------------------------------------------------------------------
//NOTE: 
//              General Variables are currently not in use!

namespace CJ
{
    public class Handler_Stats : MonoBehaviour
    {


//_____________________________________________________________________________________________________________________
//GENERAL VARIABLES:
//---------------------------------------------------------------------------------------------------------------------
    #region GENERAL VARIABLES
        #region Health Variables
            public float myHealth;
            //private float lerpTimer;
            public float myMaxHealth = 100f;
        #endregion Health Variables

        #region Visual Variabes
            public Sprite myPortrait; // Used for Sense
            public Sprite myClass; // Used for Sense
        #endregion Visual Vairables

        #region Unused Variables
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
                myHealth = myMaxHealth; // Start with full health
            }

            void Update()
            {
                myHealth = Mathf.Clamp(myHealth, 0, myMaxHealth); // returns the difference between the values (current health)
                UpdateHealthUI(); // Function to track current health PER FRAME
            }
    #endregion GENERAL VOIDS


//_____________________________________________________________________________________________________________________
// FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
    #region UpdateHealthUI Variables
        public Image frontHealthBar; // Image object for the UI front healthBar
        public Image floatingHealthBar; // Image object for the "in scene" floating healthBar
    #endregion UpdateHealthUI Variables
            
    public void UpdateHealthUI() // Function to update visual health bar after chaning its value
    {
        float hFraction = myHealth/myMaxHealth; // Calculate percentage
        frontHealthBar.fillAmount = hFraction; // Set new fillAmount for frontHealthBar (ui healthBar)
        floatingHealthBar.fillAmount = hFraction; // Set new fillAmount for floatingHealthBar (in scene healthBar)
    }

    public void TakeDamage(float damage) // Function to decrease health
    {
        myHealth -= damage; // Decrease health based on incoming damage
    }

    public void RestoreHealth(float healAmount) // Function to increase  health
    {
        myHealth += healAmount; // Increase health based on incoming heal
    }
    }
}

