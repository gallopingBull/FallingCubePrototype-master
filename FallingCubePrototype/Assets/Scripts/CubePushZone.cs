

using UnityEngine;

public class CubePushZone : MonoBehaviour
{
    public Transform cubeCenter;
    public float minPushDistance = 0.6f;       // Start pushing if within this range
    public float maxPushDistance = 1.5f;       // Max range of the push zone
    public float maxPushSpeed = 5f;            // Max speed at the closest point
    public AnimationCurve pushForceCurve;      // Optional falloff curve (0–1)

    private Transform player;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (player == null)
                player = other.transform;

            Vector3 toPlayer = player.position - cubeCenter.position;
            float distance = toPlayer.magnitude;

            if (distance < maxPushDistance)
            {
                // Closer = stronger push (inverted ratio)
                float t = Mathf.InverseLerp(maxPushDistance, minPushDistance, distance);
                float forceMultiplier = pushForceCurve != null && pushForceCurve.length > 0
                    ? pushForceCurve.Evaluate(t)
                    : t; // Linear fallback

                Vector3 pushDir = toPlayer.normalized;
                pushDir.y = 0;

                float pushAmount = maxPushSpeed * forceMultiplier;
                Vector3 push = pushDir * pushAmount * Time.deltaTime;

                player.Translate(push, Space.World);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.transform == player)
        {
            player = null;
        }
    }
}
