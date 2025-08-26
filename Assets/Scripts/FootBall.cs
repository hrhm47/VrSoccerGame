
using UnityEngine;

public class FootBall : MonoBehaviour
{
    public float kickForce;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Debug.Log("Player hit the ball!");
        }
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            // Kick in the direction the player is moving
            Vector3 kickDirection = collision.contacts[0].point - collision.transform.position;
            kickDirection = -kickDirection.normalized;

            rb.AddForce(kickDirection * kickForce, ForceMode.Impulse);
        }
    }
}
