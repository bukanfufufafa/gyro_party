using UnityEngine;

public class bomb : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            KnifeCut knife = other.GetComponentInParent<KnifeCut>();

            if (knife != null)
            {
                knife.stunt = 5f;
            }

            Destroy(gameObject);
        }
    }
}