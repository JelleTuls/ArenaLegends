using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;
using CJ;
using TMPro;

//_____________________________________________________________________________________________________________________
// NOTES
//---------------------------------------------------------------------------------------------------------------------
// NOTE: 

namespace CJ
{

public class Player_Handle_Target : MonoBehaviour
{

//_____________________________________________________________________________________________________________________
// GENERAL VARIABLES:
//---------------------------------------------------------------------------------------------------------------------
    PhotonView view;

    public GameObject playerObject; //  Refering to self player GameObject
    private GameObject isPreviousTarget; // Storing GameObject of previous target (To prevent targetting from switching up and down between targets)


//_____________________________________________________________________________________________________________________
// GENERAL VOIDS:
//---------------------------------------------------------------------------------------------------------------------
    void Start()
    {
        view = playerObject.GetComponent<PhotonView>(); // Set Photon view to the player's photon view

        isTarget = GameObject.Find("Empty_Target"); // Start with NO TARGET (The empty target GameObject)
        targetSelection.SetActive(false); // De-Activate Target Visual (No Target Setting)
        targetPortraitImage.sprite = targetPortraitNoSelection; // Start with no/default target portrait (incl. its visuals)
        targetPortraitClass.sprite = targetPortraitClassNoSelection; // Start with no/default target portrait (incl. its visuals)
    }
    

    public void Update()
    {   
        if(view.IsMine){ // Only if the view isMine: Play the upddate void
            isPreviousTarget = isTarget; // Store previous target PER FRAME
        }
    }
    

    private void LateUpdate()
    {
        if(view.IsMine){
            targetSelection.transform.position = new Vector3(isTarget.transform.position.x, 
            isTarget.transform.position.y + 0.1f, isTarget.transform.position.z); // targetSlectionCircle ==> Follows Target

            targetHealth = isTarget.GetComponent<Player_Handle_Stats>().myHealth; // Get Target's Current Health
            UpdateHealthUI(); // Play function UpdateHealthUI()
        }
    }


//_____________________________________________________________________________________________________________________
// FUNCTIONS:
//---------------------------------------------------------------------------------------------------------------------
    //_____________________________________________________________________________________________________________________
    // TARGETTING
    //---------------------------------------------------------------------------------------------------------------------
    public GameObject isTarget; // Current target GameObject reference (Switches when changing target)
    public GameObject targetSelection; // UI Visuals for current target

    public float checkRadius; // Circular distance of searching/scanning for targets
    public LayerMask checkLayers; // Variable for the layer in which target's are layered

    public Image targetPortraitImage; // Image object of target portrait
    public Image targetPortraitClass; // Image object of target class
    public Sprite targetPortraitNoSelection; // Sprite .png object of No Selection portait
    public Sprite targetPortraitClassNoSelection; // Sprite .png object of No Selecction class

    public void FTarget() // Scan for enemies within range (Select most nearby):
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, checkLayers); // Create list of objects within range
        Array.Sort(colliders, new TargetComparer(transform)); // Order based on distance:
        isTarget = GameObject.Find(colliders[0].name); // Set Active Target ==> isTarget (Most nearby target/1st from list)

        targetPortraitImage.sprite = isTarget.GetComponent<Player_Handle_Stats>().myPortrait; // Set target visual sprite == Portrait sprite from target's Handler_Stats
        targetPortraitClass.sprite = isTarget.GetComponent<Player_Handle_Stats>().myClass; // Set target visual sprite == Class sprite from target's Handler_Stats
        targetSelection.SetActive(true); // Activate taret visual object

        targetMaxHealth = isTarget.GetComponent<Player_Handle_Stats>().myMaxHealth; // Get target's MaxHealth
    }

    //_____________________________________________________________________________________________________________________
    // DAMAGE POPUP
    //---------------------------------------------------------------------------------------------------------------------
    public GameObject damageNumberPrefab; // Reference to the object prefab
    public Camera mainCamera; // Reference to the camera for the Billboard script (assign in Inspector)

    public void SpawnDamageNumber(GameObject target, int damageNumber)
    {
        // Get the target's position (without Y-offset)
        Vector3 spawnPosition = target.transform.position;

        // Instantiate the object at the target's position
        GameObject spawnedObject = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);

        // Assign the camera to the Billboard script
        Object_Adjustments_Billboard Object_Adjustments_Billboard = spawnedObject.GetComponent<Object_Adjustments_Billboard>();
        Object_Adjustments_Billboard.cam = mainCamera.transform; // Assign the camera's Transform


        // Pass the target's transform to the Shield_Break_Destroy script, which handles position updating
        Object_Adjustments_FollowAndDestroy shieldDestroyScript = spawnedObject.GetComponent<Object_Adjustments_FollowAndDestroy>();
        shieldDestroyScript.follow = target.transform; // Assign the target's transform

        TextMeshPro textMeshPro = spawnedObject.GetComponentInChildren<TextMeshPro>();
        textMeshPro.text = damageNumber.ToString(); // Set the damage number as text

        // Destroy the object after 1 second (already handled in Shield_Break_Destroy but added for safety)
        Destroy(spawnedObject, 0.8f);
    }

    //_____________________________________________________________________________________________________________________
    // Update Target UI
    //---------------------------------------------------------------------------------------------------------------------
    public Image frontHealthBar; // Image object for target health bar
    public Image frontEnergyBar; // Image object for target energy bar

    public float targetHealth; // Float variable to keep track of target's current health
    public float targetMaxHealth; // Float variable to store target's maximum health

    public void UpdateHealthUI()
    {
        float fillFront = frontHealthBar.fillAmount; // Function to determine the health bar's fill size based on percentage of total
        float hFraction = targetHealth/targetMaxHealth; // Calculate the current percentage value
        frontHealthBar.fillAmount = hFraction; // Fill health bar (Percentage to fill)
    }

    private void OnDrawGizmos() // Function to use for testing/visualizing the range of sense
    {
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
    }
}

