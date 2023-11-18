using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;

//_____________________________________________________________________________________________________________________
// NOTES
//---------------------------------------------------------------------------------------------------------------------
// NOTE: 

namespace CJ
{

public class Sense : MonoBehaviour
{

//_____________________________________________________________________________________________________________________
// GENERAL VARIABLES:
//---------------------------------------------------------------------------------------------------------------------
    PhotonView view;

    #region General Variables:
            public GameObject playerObject; //  Refering to self player GameObject
            private GameObject isPreviousTarget; // Storing GameObject of previous target (To prevent targetting from switching up and down between targets)
    #endregion


//_____________________________________________________________________________________________________________________
// GENERAL VOIDS:
//---------------------------------------------------------------------------------------------------------------------
    void Start()
    {
        view = playerObject.GetComponent<PhotonView>(); // Set Photon view to the player's photon view

        #region No default target AT START
            isTarget = GameObject.Find("Empty_Target"); // Start with NO TARGET (The empty target GameObject)
            targetSelection.SetActive(false); // De-Activate Target Visual (No Target Setting)
            targetPortraitImage.sprite = targetPortraitNoSelection; // Start with no/default target portrait (incl. its visuals)
            targetPortraitClass.sprite = targetPortraitClassNoSelection; // Start with no/default target portrait (incl. its visuals)
        #endregion
    }
    

    public void Update()
    {   
        if(view.IsMine){ // Only if the view isMine: Play the upddate void
            #region Store Previous Target:
                isPreviousTarget = isTarget; // Store previous target PER FRAME
            #endregion
        }
    }
    

    private void LateUpdate()
    {
        if(view.IsMine){
            #region Reposition target circle:
                targetSelection.transform.position = new Vector3(isTarget.transform.position.x, 
                isTarget.transform.position.y + 0.1f, isTarget.transform.position.z); // targetSlectionCircle ==> Follows Target
            #endregion

            #region Update Target UI:
                targetHealth = isTarget.GetComponent<Handler_Stats>().myHealth; // Get Target's Current Health
                UpdateHealthUI(); // Play function UpdateHealthUI()
            #endregion
        }
    }


//_____________________________________________________________________________________________________________________
// FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
#region FUNCTIONS:
    //_____________________________________________________________________________________________________________________
    // TARGETTING
    //---------------------------------------------------------------------------------------------------------------------
    #region TARGETTING 
        #region Variables
            #region Game Objects
                public GameObject isTarget; // Current target GameObject reference (Switches when changing target)
                public GameObject targetSelection; // UI Visuals for current target
            #endregion

            #region Sense Variables
                public float checkRadius; // Circular distance of searching/scanning for targets
                public LayerMask checkLayers; // Variable for the layer in which target's are layered
            #endregion

            #region Target's Visual Properties Variables
                public Image targetPortraitImage; // Image object of target portrait
                public Image targetPortraitClass; // Image object of target class
                public Sprite targetPortraitNoSelection; // Sprite .png object of No Selection portait
                public Sprite targetPortraitClassNoSelection; // Sprite .png object of No Selecction class
            #endregion
        #endregion

        public void FTarget() // Scan for enemies within range (Select most nearby):
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, checkLayers); // Create list of objects within range
            Array.Sort(colliders, new TargetComparer(transform)); // Order based on distance:
            isTarget = GameObject.Find(colliders[0].name); // Set Active Target ==> isTarget (Most nearby target/1st from list)

            targetPortraitImage.sprite = isTarget.GetComponent<Handler_Stats>().myPortrait; // Set target visual sprite == Portrait sprite from target's Handler_Stats
            targetPortraitClass.sprite = isTarget.GetComponent<Handler_Stats>().myClass; // Set target visual sprite == Class sprite from target's Handler_Stats
            targetSelection.SetActive(true); // Activate taret visual object

            targetMaxHealth = isTarget.GetComponent<Handler_Stats>().myMaxHealth; // Get target's MaxHealth
        }
    #endregion TARGETTING


//_____________________________________________________________________________________________________________________
// Update Target UI
//---------------------------------------------------------------------------------------------------------------------
    #region Update Target UI:
        #region Variables:
            #region Update UI Variables
                public Image frontHealthBar; // Image object for target health bar
                public Image frontEnergyBar; // Image object for target energy bar
            #endregion

            #region Target Stats Variables
                public float targetHealth; // Float variable to keep track of target's current health
                public float targetMaxHealth; // Float variable to store target's maximum health
            #endregion
        #endregion

        public void UpdateHealthUI()
        {
            float fillFront = frontHealthBar.fillAmount; // Function to determine the health bar's fill size based on percentage of total
            float hFraction = targetHealth/targetMaxHealth; // Calculate the current percentage value
            frontHealthBar.fillAmount = hFraction; // Fill health bar (Percentage to fill)
            //_____________________________________________________________________________________________________________________
            // SLIDING ANIMATION WHEN CHANGING HEALTH:
            //---------------------------------------------------------------------------------------------------------------------
                // if(fillBack > hFraction){
                // frontHealthBar.fillAmount = hFraction; // Fill health bar (Percentage to fill)
                //     backHealthBar.color = Color.red;
                //     lerpTimer += Time.deltaTime;
                //     float percentageComplete = lerpTimer / chipSpeed;
                //     percentageComplete = percentageComplete * percentageComplete;
                //     backHealthBar.fillAmount = Mathf.Lerp(fillBack, hFraction, percentageComplete);
                // }
                // if(fillFront < hFraction){
                //     backHealthBar.color = Color.green;
                //     backHealthBar.fillAmount = hFraction;
                //     lerpTimer += Time.deltaTime;
                //     float percentageComplete = lerpTimer / chipSpeed;   
                //     percentageComplete = percentageComplete * percentageComplete;
                //     frontHealthBar.fillAmount = Mathf.Lerp(fillFront, backHealthBar.fillAmount, percentageComplete);       
                //}
        }
    #endregion Update Target UI

#endregion FUNCTIONS

    private void OnDrawGizmos() // Function to use for testing/visualizing the range of sense
    {
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
    }
}

