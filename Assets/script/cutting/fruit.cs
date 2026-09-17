using UnityEngine;

public class Fruit : MonoBehaviour
{
    private Rigidbody rb;

    //public GameObject player1;
    //public GameObject player2;

    public float cutDistanceX = 0.2f;
    public int scoreGiven;
    public bool cutted = false;

    bool started;


    private void Start()
    {
        started = true;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.CompareTag("Player"))
            {
                KnifeCut knife = other.GetComponentInParent<KnifeCut>();

                if (knife != null)
                {

                    if (!cutted)
                    {
                        knife.score += scoreGiven;
                    }
                    cutted = true;


                }
                detach();
            }
        }
    }


    private void Update()
    {
        if (!started) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    public void detach()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.localPosition += Vector3.right * cutDistanceX;
    }
}