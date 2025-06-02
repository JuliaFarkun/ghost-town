// PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    public static bool isGamePaused = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody2D не найден на этом объекте!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (isGamePaused)
        {
            movement = Vector2.zero;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // PauseMenuManager pauseMenu = FindObjectOfType<PauseMenuManager>();
                // pauseMenu?.TogglePauseMenu();
            }
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // PauseMenuManager pauseMenu = FindObjectOfType<PauseMenuManager>();
            // pauseMenu?.TogglePauseMenu();
        }
    }

    void FixedUpdate()
    {
        if (isGamePaused || rb == null)
        {
            if (rb != null && isGamePaused) rb.linearVelocity = Vector2.zero;
            return;
        }
        
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
        else 
        {
            rb.linearVelocity = movement.normalized * moveSpeed;
        }
    }
}