using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISettings : MonoBehaviour
{
    public RectTransform objectToAdjust; // The object whose height you want to adjust
    public RectTransform referenceObject1; // First reference object
    public RectTransform referenceObject2; // Second reference object
    public RectTransform referenceObject3; // Second reference object
    public RectTransform referenceObject4; // Second reference object

    public RectTransform socialContainerInvites;
    public RectTransform socialContainerFriends;
    public RectTransform invitesScrollView;
    public RectTransform friendsScrollView;

    private void Start()
    {
        AdjustHeight();
        AdjustSocialContainers();
        AdjustScrollViews();
    }

    private void AdjustHeight()
    {
        RectTransform canvasRectTransform = GetComponent<RectTransform>(); // Assuming this script is attached to the canvas

        float canvasHeight = canvasRectTransform.rect.height;
        float totalReferenceObjectsHeight = referenceObject1.rect.height + referenceObject2.rect.height + referenceObject3.rect.height + referenceObject4.rect.height;
        float newHeight = canvasHeight - totalReferenceObjectsHeight;

        // Set the adjusted height for the object
        Vector2 objectSize = objectToAdjust.sizeDelta;
        objectSize.y = newHeight;
        objectToAdjust.sizeDelta = objectSize;
    }

    private void AdjustSocialContainers()
    {
        RectTransform canvasRectTransform = GetComponent<RectTransform>(); // Assuming this script is attached to the canvas

        float canvasHeight = canvasRectTransform.rect.height;
        float totalReferenceObjectsHeight = referenceObject1.rect.height + referenceObject2.rect.height + referenceObject3.rect.height + referenceObject4.rect.height;
        float newHeight = canvasHeight - totalReferenceObjectsHeight;
        
        Vector2 objectSizeInvites = socialContainerInvites.sizeDelta;
        Vector2 objectSizeFriends = socialContainerFriends.sizeDelta;

        objectSizeInvites.y = newHeight;
        objectSizeFriends.y = newHeight;

        socialContainerInvites.sizeDelta = objectSizeInvites;
        socialContainerFriends.sizeDelta = objectSizeFriends;
    }

    private void AdjustScrollViews()
    {
        float socialContainerFriendsHeight = socialContainerFriends.rect.height;
        float socialContainerInvitesHeight = socialContainerInvites.rect.height;

        float newFriendsViewHeight = socialContainerFriendsHeight - 180f; 
        float newInvitesViewHeight = socialContainerInvitesHeight - 90f;
        
        Vector2 objectSizeInvitesView = invitesScrollView.sizeDelta;
        Vector2 objectSizeFriendsView = friendsScrollView.sizeDelta;

        objectSizeInvitesView.y = newInvitesViewHeight;
        objectSizeFriendsView.y = newFriendsViewHeight;

        friendsScrollView.sizeDelta = objectSizeFriendsView;
        invitesScrollView.sizeDelta = objectSizeInvitesView;
    }


}

