using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace CJ{

    public class Handler_Camera_Rotation : MonoBehaviour{

        PhotonView view;

        #region Basic Camera setup:
        public Transform target;
        public Transform playerObject;
        public Transform horizontalOrientation;
        public Transform verticalOrientation;

        public GameObject cameraTouchPad;
        public GameObject m_Camera;
        #endregion

        #region Rotate Player:
        //Vector3 position to setup the standard distance / height of the camera:
        public Vector3 offset;
        public float cameraDistance;
        //Option to lock the Vector3 offset position.
        public bool offsetStartingPosition;
        //Value to set minimum camera height:
        public float cameraMinimumHeight = 2f;
        public float maxViewAngle = 45f;
        public float minViewAngle = 315f;
        //Rotation speed:
        public float smoothSpeed = 0.125f;
        #endregion

        #region Camera Collision:
        private float cameraDistanceStart;
        private GameObject hitCollision;
        private GameObject previousCollision;
        private bool hitFlag;
        #endregion


        void Start(){
            view = playerObject.GetComponent<PhotonView>();

            #region Cursor & Parenting:
            //Remove cursor & centralize it:
                // Cursor.lockState = CursorLockMode.Locked;
            //Hide cursor from screen:
                // Cursor.visible = false; 
            //Set free orientation from its parent:
            horizontalOrientation.transform.parent = null;
            #endregion
            
            #region Offset Position:
            //If True ==> return camera to starting postion:
            offsetStartingPosition = true;
            //Set camera at starting position:
            if(offsetStartingPosition){
                //Save camera starting position:
                offset = target.position - m_Camera.transform.position;
            }
            #endregion

            #region Store Values:
                cameraDistanceStart = cameraDistance;
                previousCollision = null;
            #endregion
        }

        void Update(){
            if(view.IsMine){
                #region Camera_Collision:
                //Set Camera Back to Original Position: 
                cameraDistance = cameraDistanceStart;
                //RayCast From: Target ==> Camera:
                //Direction = new Vector3(transform.position-target.position):
                Debug.DrawRay(target.transform.position, new Vector3(m_Camera.transform.position.x - target.position.x, m_Camera.transform.position.y - target.position.y, m_Camera.transform.position.z - target.position.z));
                Ray rayCastCameraCollision = new Ray(target.transform.position, new Vector3(m_Camera.transform.position.x - target.position.x, m_Camera.transform.position.y - target.position.y, m_Camera.transform.position.z - target.position.z));
                //Calculate RaycastHit & Set range: rayCastDistance * 8.5f:
                RaycastHit hitCameraCollision; 
                if(Physics.Raycast(rayCastCameraCollision, out hitCameraCollision, cameraDistanceStart * 8.5f)){
                    if(hitCameraCollision.collider.tag == "isObstacle"){
                        // Store collided object:
                        hitFlag = true;
                        hitCollision = hitCameraCollision.transform.gameObject;
                        // cameraDistance = (hitCameraCollision.distance/8.5f);

                        // Deactivate MeshRenderer of collided object.
                        hitCollision.GetComponent<MeshRenderer>().enabled = false;
                    }
                    else
                    {
                        hitFlag = false;
                    }
                }
                else if (hitFlag == true)
                    {
                        previousCollision.GetComponent<MeshRenderer>().enabled = true;
                    }
                if(hitCameraCollision.collider.tag == "isWall"){
                        cameraDistance = (hitCameraCollision.distance/8.5f);
                    }
                previousCollision = hitCollision;
                #endregion
            }
        }

        void LateUpdate(){
            if(view.IsMine){
                #region Target & Orientation Position:
                //Make camera look at Target:
                m_Camera.transform.LookAt(target);
                //Set orientation position = player:
                horizontalOrientation.transform.position = playerObject.transform.position;
                #endregion 

                #region Calculate Mouse X & Mouse Y Position:
                //Calculate X position of the mouse ==> Rotate the camera / Player:
                float horizontal = cameraTouchPad.GetComponent<UltimateTouchpad>().GetHorizontalAxis() * 0.25f; //0.1f for reduce Sensitivity!
                horizontalOrientation.Rotate(0, horizontal, 0);
                //Calculate Y position of the mouse ==> Rotate the camera / orientation:
                float vertical = cameraTouchPad.GetComponent<UltimateTouchpad>().GetVerticalAxis() * 0.15f; //0.1f for reduce Sensitivity!
                verticalOrientation.Rotate(-vertical, 0, 0);
                #endregion

                #region Limit Camera Min/Max Height:
                if(verticalOrientation.rotation.eulerAngles.x > 45f && verticalOrientation.rotation.eulerAngles.x < 180f){
                    verticalOrientation.rotation = Quaternion.Euler(maxViewAngle, verticalOrientation.eulerAngles.y, 0);
                }
                if(verticalOrientation.rotation.eulerAngles.x > 180f && verticalOrientation.rotation.eulerAngles.x < 315f){
                    verticalOrientation.rotation = Quaternion.Euler(minViewAngle, horizontalOrientation.eulerAngles.y, 0);
                }

                //Prevent the camera to move below the player. 
                float targetPositionOffset = target.position.y - (cameraMinimumHeight);
                if(m_Camera.transform.position.y < targetPositionOffset){
                    m_Camera.transform.position = new Vector3(m_Camera.transform.position.x, targetPositionOffset, m_Camera.transform.position.z);
                }
                #endregion

                #region Determine angle & circle of camera towards the player:
                //Create the desired angles:
                float desiredYAngle = horizontalOrientation.eulerAngles.y;
                float desiredXAngle = verticalOrientation.eulerAngles.x;
                //Convert desired angels into rotation values:
                Quaternion rotation = Quaternion.Euler(desiredXAngle, desiredYAngle, 0);
                //Transform the position & circle of camera:
                #endregion

                #region Relocate the Camera:
            m_Camera.transform.position = Vector3.Slerp(m_Camera.transform.position, playerObject.position - (rotation * offset * cameraDistance), smoothSpeed);
            #endregion   
            }
        }
    }
}

