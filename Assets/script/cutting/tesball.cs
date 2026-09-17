using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public float force = 10f;

    private Rigidbody rb;

    [SerializeField] private GameplayManagerProxy gameplayManagerProxy;
    bool started;


    void Start()
    {
        gameplayManagerProxy.OnStartGame += (_, _) =>
      {
          started = true;
          rb = GetComponent<Rigidbody>();
      };
    }

    void FixedUpdate()
    {
        if (!started) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical);

        rb.AddForce(direction * force);
    }
}