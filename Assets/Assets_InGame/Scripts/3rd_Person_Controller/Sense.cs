using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;

namespace CJ
{
    public class Sense : MonoBehaviour
    {
        PhotonView view;
        public GameObject playerObject;

        #region Variables:
        //Range Variables:
        public float checkRadius;
        public LayerMask checkLayers;

        //Selection Variables:
        public GameObject targetSelection;
        public GameObject isTarget;
        private GameObject isPreviousTarget;

        //Target Elements:
        public float targetHealth;
        public float targetMaxHealth;
        public Image targetPortraitImage;
        public Image targetPortraitClass;
        public Sprite targetPortraitNoSelection;
        public Sprite targetPortraitClassNoSelection;

        //Target UI Update:
        //private float lerpTimer;
        //public float chipSpeed = 2f;
        public Image frontHealthBar;
        //public Image backHealthBar;
        public Image frontEnergyBar;
        //public Image backEnergyBar;
        #endregion

        void Start()
        {
            view = playerObject.GetComponent<PhotonView>();

            #region Start with no target:
            //Start with NO TARGET!
            isTarget = GameObject.Find("Empty_Target");
            targetSelection.SetActive(false);
            targetPortraitImage.sprite = targetPortraitNoSelection;
            targetPortraitClass.sprite = targetPortraitClassNoSelection;
            #endregion
        }

        public void Update()
        {   
            if(view.IsMine){
                #region Store Previous Target:
                //Store previous target (For resetting Color):
                isPreviousTarget = isTarget;
                #endregion
            }
        }
        
        //After all calculations in Update():
        private void LateUpdate()
        {
            if(view.IsMine){
                #region Reposition target circle:
                //targetSlectionCircle ==> Follows Target:
                targetSelection.transform.position = new Vector3(isTarget.transform.position.x, isTarget.transform.position.y + 0.1f, isTarget.transform.position.z);
                #endregion

                #region Update Target UI:
                //Target Current Health:
                targetHealth = isTarget.GetComponent<Handler_Stats>().myHealth;

                UpdateHealthUI();
                #endregion
            }
        }

        public void FTarget()
        {
            //Scan for enemies within range (Select most nearby):
            //Create list of objects within range:
            Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, checkLayers);
            //Order based on distance:
            Array.Sort(colliders, new TargetComparer(transform));
            //Set Active Target ==> isTarget (Most nearby target):
            isTarget = GameObject.Find(colliders[0].name);
            //Gather Target Elements:
            targetPortraitImage.sprite = isTarget.GetComponent<Handler_Stats>().myPortrait;
            targetPortraitClass.sprite = isTarget.GetComponent<Handler_Stats>().myClass;
            targetSelection.SetActive(true);

            //Get target MaxHealth:
            targetMaxHealth = isTarget.GetComponent<Handler_Stats>().myMaxHealth;
        }

        public void UpdateHealthUI()
        {
            float fillFront = frontHealthBar.fillAmount;
            //float fillBack = backHealthBar.fillAmount;
            float hFraction = targetHealth/targetMaxHealth;
            // if(fillBack > hFraction){
            frontHealthBar.fillAmount = hFraction;
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

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, checkRadius);
        }
    }
}

