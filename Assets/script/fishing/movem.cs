using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Player")]
    public bool player1 = true;

    void Update()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (player1)
        {
            // =========================
            // PLAYER 1 - WASD
            // =========================

            if (Input.GetKey(KeyCode.A))
                horizontal = 1f;

            if (Input.GetKey(KeyCode.D))
                horizontal = -1f;

            if (Input.GetKey(KeyCode.W))
                vertical = -1f;

            if (Input.GetKey(KeyCode.S))
                vertical = 1f;
        }
        else
        {
            // =========================
            // PLAYER 2 - ARROW
            // =========================

            if (Input.GetKey(KeyCode.LeftArrow))
                horizontal = 1f;

            if (Input.GetKey(KeyCode.RightArrow))
                horizontal = -1f;

            if (Input.GetKey(KeyCode.UpArrow))
                vertical = -1f;

            if (Input.GetKey(KeyCode.DownArrow))
                vertical = 1f;
        }

        Vector3 movement = new Vector3(
            horizontal,
            0f,
            vertical
        );

        transform.Translate(
            movement * moveSpeed * Time.deltaTime,
            Space.World
        );
    }
}