using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    // Для паузы
    public static bool isGamePaused = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isGamePaused) // Если игра на паузе, не обрабатываем ввод для движения
        {
            movement = Vector2.zero; // Останавливаем движение
            return;
        }

        // Ввод для движения
        movement.x = Input.GetAxisRaw("Horizontal"); // A, D, LeftArrow, RightArrow
        movement.y = Input.GetAxisRaw("Vertical");   // W, S, UpArrow, DownArrow

        // Проверка на Escape для меню паузы (рассмотрим ниже)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Логика открытия/закрытия Escape-меню
            // Например, FindObjectOfType<PauseMenuManager>().TogglePauseMenu();
        }
    }

    void FixedUpdate()
    {
        // Применяем движение в FixedUpdate для физики
        // Если используешь Rigidbody2D в режиме Dynamic
        // rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);

        // Если используешь Rigidbody2D в режиме Kinematic или двигаешь через Transform
        if (rb != null && rb.bodyType == RigidbodyType2D.Kinematic)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
        else // Если нет Rigidbody или он не Kinematic, двигаем через Transform
        {
            transform.Translate(movement.normalized * moveSpeed * Time.deltaTime);
        }
    }
}