using UnityEngine;

public class KnifeCut : MonoBehaviour
{
    [Header("Pengaturan")]
    public float moveDistance = 0.5f;
    public float moveSpeed = 5f;
    public KeyCode cutKey = KeyCode.Space;

    [Header("Stun")]
    public float stunt = 0f;

    private Vector3 startPosition;
    private bool cutting = false;

    public int score;

    [SerializeField] private GameplayManagerProxy gameplayManagerProxy;
    bool started;


    void Start()
    {
        gameplayManagerProxy.OnStartGame += (_, _) =>
        {
            started = true;
            startPosition = transform.position;
        };
        // Controller controller = ControllerService.Instance.GetController();
    }

    private void OnTriggerEnter(Collider other)
    {
        Fruit fruit = other.GetComponentInParent<Fruit>();

        if (fruit != null)
        {
            // fruit.CutPart(other);
        }
    }

    void Update()
    {
        if (!started) return;

        // Kalau sedang stun, jangan lakukan apa-apa
        if (stunt > 0)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                moveSpeed * Time.deltaTime
            );

            stunt -= Time.deltaTime;
            return;
        }

        if (Input.GetKeyDown(cutKey))
        {
            cutting = true;
        }

        if (cutting)
        {
            Vector3 targetPosition =
                startPosition + Vector3.down * moveDistance;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                cutting = false;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                moveSpeed * Time.deltaTime
            );
        }
    }
}