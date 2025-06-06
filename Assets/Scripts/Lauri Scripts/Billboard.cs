using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // Make the canvas face the camera and stay upright
            transform.LookAt(Camera.main.transform.position, Vector3.up);
        }
    }
}
