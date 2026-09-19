using UnityEngine;

public class RobotHover : MonoBehaviour
{
    [Header("Vertical Hover")]
    public float verticalRange = 0.5f;  
    public float verticalSpeed = 1.5f;   

    [Header("Horizontal Wobble")]
    public float horizontalRange = 0.2f;
    public float horizontalSpeed = 1.0f; 

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * verticalSpeed) * verticalRange;

        float newX = Mathf.Sin((Time.time + 1.0f) * horizontalSpeed) * horizontalRange;

        transform.localPosition = startPosition + new Vector3(newX, newY, 0);
        float tilt = Mathf.Sin(Time.time * horizontalSpeed) * 5.0f; 
        transform.localRotation = Quaternion.Euler(0, 0, tilt);
    }
}