using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Obstacle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Cari script PlayerCollision di objek yang menabrak
        PlayerCollision player = other.GetComponent<PlayerCollision>();
        
        if (player != null)
        {
            player.HitObstacle(); // Beritahu pesawat bahwa dia menabrak
        }
    }
}