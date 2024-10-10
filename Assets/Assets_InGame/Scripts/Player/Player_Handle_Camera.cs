using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//_____________________________________________________________________________________________________________________
//NOTES
//---------------------------------------------------------------------------------------------------------------------
//NOTE: 

namespace CJ
{
    public class Player_Handle_Camera : MonoBehaviour
    {
//_____________________________________________________________________________________________________________________
//GENERAL VARIABLES:
//---------------------------------------------------------------------------------------------------------------------
        PhotonView view;

        public Transform target; // Camera's target
        public Transform playerObject; // Reference to the player object
        public Transform horizontalOrientation; // Horizontal rotation (To allow seperate rotations along axes)
        public Transform verticalOrientation; // Vertical rotation (To allow seperate rotations along axes)

        public GameObject cameraTouchPad; // Field of touch directly translating x/y movement to horizontal/vertical camera rotation
        public GameObject m_Camera; // My camera

        public Vector3 offset; // Vector3 position to setup the standard distance / height of the camera (Starting position)
        public float cameraDistance; // Current camera distance
        public bool offsetStartingPosition; // To lock and reset camera distance
        public float cameraMinimumHeight = 2f; // To prevent camera going through the floor
        public float maxViewAngle = 45f; // To prevent camera reaching top position above player
        public float minViewAngle = 315f; // To prevent camera reaching lowest position below player
        public float smoothSpeed = 0.125f; // Rotation speed:

        private float cameraDistanceStart; // To store starting distance
        private GameObject hitCollision; // To hold object of collision with raycast
        private GameObject previousCollision; // To hold previous object of collision
        private bool hitFlag; // Collision detection signal

//_____________________________________________________________________________________________________________________
//GENERAL UPDATE LOOPS:
//---------------------------------------------------------------------------------------------------------------------
        void Start(){
            view = playerObject.GetComponent<PhotonView>(); // Get player's personal photon view

            // Cursor settings (OFF FOR MOBILE):
            // Remove cursor & centralize it:
                // Cursor.lockState = CursorLockMode.Locked;
            // Hide cursor from screen:
                // Cursor.visible = false; 
            
                horizontalOrientation.transform.parent = null; // Set free orientation from its parent:
            
            // Offset Camera Position (AT START):
            offsetStartingPosition = true; // When True ==> return camera to starting postion
            if(offsetStartingPosition){ // Set camera at starting position
                offset = target.position - m_Camera.transform.position; // Save camera starting position
            }

            // Store Values (AT START):
            cameraDistanceStart = cameraDistance; // Set camera distance AT START
            previousCollision = null; // Start previous collision = Null
        }

        void Update(){
            if(view.IsMine){
                cameraDistance = cameraDistanceStart; // Set Camera Back to Original Position (Later collision position will be calculated)
                Debug.DrawRay(target.transform.position, new Vector3(m_Camera.transform.position.x 
                    - target.position.x, m_Camera.transform.position.y - target.position.y, 
                    m_Camera.transform.position.z - target.position.z)); // Draw RayCast From: Target ==> Camera (Debug Drawing)
                    
                Ray rayCastCameraCollision = new Ray(target.transform.position, 
                    new Vector3(m_Camera.transform.position.x - target.position.x, 
                    m_Camera.transform.position.y - target.position.y, m_Camera.transform.position.z 
                    - target.position.z)); // Calculate RayCast From: Target ==> Camera

                RaycastHit hitCameraCollision; // Set RaycastHit

                if(Physics.Raycast(rayCastCameraCollision, out hitCameraCollision, 
                    cameraDistanceStart * 8.5f)){ // Check if RayCast hit something at range: rayCastDistance * 8.5f
                    if (hitCameraCollision.collider != null && hitCameraCollision.collider.tag == "isObstacle") 
                    {
                        // Check if object's tag being hit == isObstacle
                        hitFlag = true; // Signal hit
                        hitCollision = hitCameraCollision.transform.gameObject; // Store hitted object
                                            
                        hitCollision.GetComponent<MeshRenderer>().enabled = false; // Deactivate MeshRenderer of collided object
                    }
                    else
                    {
                        hitFlag = false; // If there's no hit, or hit but not with isObstacle tag ==> No hit signal
                    }
                }
                // To ensure previous hitFlag get visible again, even if new hitFlag collides with previous hitFlag
                else if (hitFlag == true)  // If no hit with isObstacle object, But signal is still TRUE
                    {
                        previousCollision.GetComponent<MeshRenderer>().enabled = true; // PreviousCollision become visible again
                    }
                if(hitCameraCollision.collider != null && hitCameraCollision.collider.tag == "isWall"){ // If the hitted object.tag == Wall
                        cameraDistance = (hitCameraCollision.distance/8.5f); // Move camera closer to the player
                    }
                previousCollision = hitCollision; // After all calculations: Set previousCollision to current collision
            }
        }

        void LateUpdate(){
            if(view.IsMine){
                m_Camera.transform.LookAt(target); // Make camera look at Target
                horizontalOrientation.transform.position = playerObject.transform.position; // Set orientation position on player.transform.position

                // Calculate Mouse X & Mouse Y Position:
                float horizontal = cameraTouchPad.GetComponent<UltimateTouchpad>().GetHorizontalAxis() * 0.25f; //Calculate X position of the mouse * 0.25f for reduce Sensitivity!
                horizontalOrientation.Rotate(0, horizontal, 0); // Based on horizontal input ==> Rotate horizontalOrientation object
                float vertical = cameraTouchPad.GetComponent<UltimateTouchpad>().GetVerticalAxis() * 0.15f; //Calculate Y position of the mouse * 0.1f for reduce Sensitivity!
                verticalOrientation.Rotate(-vertical, 0, 0); // Based on vertical input ==> Rotate verticalOrientation object

                // Limit Camera Min/Max Height:
                if(verticalOrientation.rotation.eulerAngles.x > 45f // If X angle is higher than 45f (Slighly above ground)
                    && verticalOrientation.rotation.eulerAngles.x < 180f){ // & If X angle is lower than 180f (Straight line of sphere-of-reach)
                        verticalOrientation.rotation = Quaternion.Euler(maxViewAngle, 
                        verticalOrientation.eulerAngles.y, 0); // Then: Set camera on its position (Prevents camera from going lower than 45f)
                }
                if(verticalOrientation.rotation.eulerAngles.x > 180f // If X angle is higher than 180f (Straight line of sphere-of-reach)
                    && verticalOrientation.rotation.eulerAngles.x < 315f){ // & If X angle is lower than 180f (Max reach above payer)
                        verticalOrientation.rotation = Quaternion.Euler(minViewAngle, 
                        horizontalOrientation.eulerAngles.y, 0); // Then: Set camera on its position (Prevents camera from going higher than 315f)
                }
        
                float targetPositionOffset = target.position.y - (cameraMinimumHeight); // Set offset
                if(m_Camera.transform.position.y < targetPositionOffset){
                    m_Camera.transform.position = new Vector3(m_Camera.transform.position.x, 
                    targetPositionOffset, m_Camera.transform.position.z); // Prevent the camera POSITION to move below the player (Not rotation)
                }

                // Set the FINAL angle & circle of camera towards the player:
                float desiredYAngle = horizontalOrientation.eulerAngles.y; // Create the desired angle Y
                float desiredXAngle = verticalOrientation.eulerAngles.x; // Create the desired angle Y
                Quaternion rotation = Quaternion.Euler(desiredXAngle, desiredYAngle, 0); // Convert desired angels into rotation values:

                // Relocate the Camera:
                m_Camera.transform.position = Vector3.Slerp(m_Camera.transform.position, 
                playerObject.position - (rotation * offset * cameraDistance), 
                smoothSpeed); // Smoothly move the camera towards the calculated position (Coming from the previous frame's position)  
            }
        }
    }
}

