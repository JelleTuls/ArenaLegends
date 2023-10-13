using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISettingsRelativePositionHeight : MonoBehaviour
{
    public Transform firstObject; // The child object you want to position
    public Transform secondObject;
    public Transform thirdObject;

    public float percentagePosition1;
    public float percentagePosition2;
    public float percentagePosition3;

    private void Start()
    {
        PositionRelativeTo(percentagePosition1, percentagePosition2, percentagePosition3);
    }

    private void PositionRelativeTo(float percentage1, float percentage2, float percentage3)
    {
        RectTransform parentRectTransform = GetComponent<RectTransform>();

        float parentHeight = parentRectTransform.rect.height;

        if (firstObject != null)
        {
            float yOffset1 = (parentHeight / 2) * percentage1;
            Vector3 newPosition1 = firstObject.localPosition;
            newPosition1.y = yOffset1;
            firstObject.localPosition = newPosition1;
        }

        if (secondObject != null)
        {
            float yOffset2 = (parentHeight / 2) * percentage2;
            Vector3 newPosition2 = secondObject.localPosition;
            newPosition2.y = yOffset2;
            secondObject.localPosition = newPosition2;
        }

        if (thirdObject != null)
        {
            float yOffset3 = (parentHeight / 2) * percentage3;
            Vector3 newPosition3 = thirdObject.localPosition;
            newPosition3.y = yOffset3;
            thirdObject.localPosition = newPosition3;
        }
    }
}