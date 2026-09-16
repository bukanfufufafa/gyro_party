using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Obstacle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Pastikan Pesawat (Child) Anda memiliki Tag "Player"
        if (other.CompareTag("Player"))
        {
            // Panggil fungsi GameOver di LevelManager
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.TriggerGameOver();
            }
        }
    }
}