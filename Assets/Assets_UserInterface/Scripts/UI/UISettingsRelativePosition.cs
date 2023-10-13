using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISettingsRelativePosition : MonoBehaviour
{
    public Transform firstObject; // The child object you want to position
    public Transform secondObject;
    public Transform thirdObject;
    public Transform fourthObject;
    public Transform fifthObject;

    public float percentagePosition1;
    public float percentagePosition2;
    public float percentagePosition3;
    public float percentagePosition4;
    public float percentagePosition5;

    private void Start()
    {
        PositionRelativeTo(percentagePosition1, percentagePosition2, percentagePosition3, percentagePosition4, percentagePosition5);
    }

    private void PositionRelativeTo(float percentage1, float percentage2, float percentage3, float percentage4, float percentage5)
    {
        RectTransform parentRectTransform = GetComponent<RectTransform>();

        float parentWidth = parentRectTransform.rect.width;

        if (firstObject != null)
        {
            float xOffset1 = (parentWidth / 2) * percentage1;
            Vector3 newPosition1 = firstObject.localPosition;
            newPosition1.x = xOffset1;
            firstObject.localPosition = newPosition1;
        }

        if (secondObject != null)
        {
            float xOffset2 = (parentWidth / 2) * percentage2;
            Vector3 newPosition2 = secondObject.localPosition;
            newPosition2.x = xOffset2;
            secondObject.localPosition = newPosition2;
        }

        if (thirdObject != null)
        {
            float xOffset3 = (parentWidth / 2) * percentage3;
            Vector3 newPosition3 = thirdObject.localPosition;
            newPosition3.x = xOffset3;
            thirdObject.localPosition = newPosition3;
        }

        if (fourthObject != null)
        {
            float xOffset4 = (parentWidth / 2) * percentage4;
            Vector3 newPosition4 = fourthObject.localPosition;
            newPosition4.x = xOffset4;
            fourthObject.localPosition = newPosition4;
        }

        if (fifthObject != null)
        {
            float xOffset5 = (parentWidth / 2) * percentage5;
            Vector3 newPosition5 = fifthObject.localPosition;
            newPosition5.x = xOffset5;
            fifthObject.localPosition = newPosition5;
        }
    }
}
