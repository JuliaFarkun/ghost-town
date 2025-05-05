using UnityEngine;

public class GirlMovement : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 5f;
    
    [Header("Эффект парения")]
    public float floatAmplitude = 0.2f;
    public float floatFrequency = 1.5f;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer не найден на объекте!");
        }

        originalScale = transform.localScale;
    }

    void Update()
    {
        // Получаем ввод
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Создаем вектор движения
        Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);
        
        // Нормализуем направление и создаем движение
        if (direction.magnitude > 0)
        {
            direction.Normalize();
            Vector3 movement = direction * (moveSpeed * Time.deltaTime);
            transform.position += movement;
        }

        // Эффект парения
        float floatingOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        Vector3 currentPos = transform.position;
        currentPos.y += floatingOffset;
        transform.position = currentPos;

        // Поворот спрайта
        if (horizontalInput != 0)
        {
            spriteRenderer.flipX = horizontalInput < 0;
        }
    }
}