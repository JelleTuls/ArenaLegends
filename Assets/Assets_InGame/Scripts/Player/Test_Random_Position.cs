using UnityEngine;

public class Test_Random_Position : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Generate a random position on the X-axis between -10 and 10
        float randomX = Random.Range(-5f, 5f);

        // Set the new position of the cube while keeping Y and Z at their original values
        transform.position = new Vector3(randomX, transform.position.y, transform.position.z);

        Debug.Log($"Cube's new position: {transform.position}");
    }
}