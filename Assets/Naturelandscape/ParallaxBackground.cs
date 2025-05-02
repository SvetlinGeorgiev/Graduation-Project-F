using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform[] backgrounds; // Array of all the backgrounds to be parallaxed
    public float[] parallaxScales; // Proportion of the camera's movement to move the backgrounds by
    public float smoothing = 1f; // How smooth the parallax effect should be

    public Transform cam; // Public reference to the main camera's transform
    private Vector3 previousCamPos; // The position of the camera in the previous frame

    private void Start()
    {
        // If the camera is not assigned, find the main camera
        if (cam == null)
        {
            cam = Camera.main.transform;
        }

        // Store the previous frame's camera position
        previousCamPos = cam.position;

        // Assign corresponding parallax scales
        parallaxScales = new float[backgrounds.Length];
        for (int i = 0; i < backgrounds.Length; i++)
        {
            // Example parallax scales based on layer depth
            if (i == 0) // Sky layer (farthest)
            {
                parallaxScales[i] = 0.1f;
            }
            else if (i == 1) // Distant mountains layer
            {
                parallaxScales[i] = 0.3f;
            }
            else if (i == 2) // Mid-ground trees layer
            {
                parallaxScales[i] = 0.6f;
            }
            else if (i == 3) // Foreground elements layer (closest)
            {
                parallaxScales[i] = 0.9f;
            }
        }
    }

    private void Update()
    {
        // For each background
        for (int i = 0; i < backgrounds.Length; i++)
        {
            // The parallax is the opposite of the camera movement because the previous frame multiplied by the scale
            float parallax = (previousCamPos.x - cam.position.x) * parallaxScales[i];

            // Set a target x position which is the current position plus the parallax
            float backgroundTargetPosX = backgrounds[i].position.x + parallax;

            // Create a target position which is the background's current position with its target x position
            Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, backgrounds[i].position.y, backgrounds[i].position.z);

            // Fade between current position and the target position using lerp
            backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundTargetPos, smoothing * Time.deltaTime);
        }

        // Set the previousCamPos to the camera's position at the end of the frame
        previousCamPos = cam.position;
    }
}
