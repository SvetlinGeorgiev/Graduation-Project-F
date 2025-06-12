using Cinemachine.Utility;
using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 2, -10);

    private float previousX;

    private void LateUpdate()
    {
        if (player == null) return;
        Vector3 targetPosition = new Vector3(player.position.x + offset.x, player.position.y + offset.y, offset.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Detect movement direction
        float moveDir = player.position.x - previousX;

        if (Mathf.Abs(moveDir) > 0.01f)
        {
            offset.x = moveDir > 0 ? 4.5f : -3f;
            
        }
        targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        previousX = player.position.x;

    }
}
