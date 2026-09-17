using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerCollision : MonoBehaviour
{
    [Tooltip("Tarik LevelManager milik player ini ke sini")]
    public LevelManager myLevelManager;

    // Fungsi ini akan dipanggil oleh rintangan
    public void HitObstacle()
    {
        if (myLevelManager != null)
        {
            myLevelManager.TriggerGameOver();
        }
    }
}