using UnityEngine;

public class SimpleKinematicMoveTest : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody2D rb;
    private Collider2D playerCollider; // Для использования в Cast

    // Опционально: для отражения спрайта без изменения scale.x
    // public SpriteRenderer mainSpriteRenderer; // Перетащи сюда главный спрайт игрока в Inspector

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>(); // Получаем коллайдер один раз здесь

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on " + gameObject.name + "!");
            enabled = false; // Выключаем скрипт, если нет Rigidbody
            return;
        }
        if (playerCollider == null)
        {
            Debug.LogError("Collider2D not found on " + gameObject.name + "! Cast will not work correctly.");
            enabled = false; // Выключаем скрипт, если нет Collider2D
            return;
        }
        // Опционально: Проверка SpriteRenderer
        // if (mainSpriteRenderer == null)
        // {
        //     mainSpriteRenderer = GetComponent<SpriteRenderer>(); // Попытка найти, если не назначен
        //     if (mainSpriteRenderer == null)
        //     {
        //         Debug.LogWarning("MainSpriteRenderer not assigned on " + gameObject.name + " for flipping.");
        //     }
        // }

        if (rb.bodyType != RigidbodyType2D.Kinematic)
        {
            Debug.LogWarning("Rigidbody2D is not Kinematic on " + gameObject.name + "!");
        }
        rb.useFullKinematicContacts = true; // Важно для Kinematic тел
        Debug.Log($"Rigidbody2D on {gameObject.name}: Type={rb.bodyType}, useFullKinematicContacts={rb.useFullKinematicContacts}");
    }

    // Ввод лучше обрабатывать в Update, а физику в FixedUpdate.
    // Но для простоты оставим пока так, как было в твоем примере с вводом в FixedUpdate.
    // Если будут проблемы с "дерганым" вводом, перенеси получение Input.GetAxisRaw в Update()
    // и сохрани значения в переменные класса.

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector2 movementInput = new Vector2(moveHorizontal, moveVertical);
        Vector2 moveDirection = movementInput.normalized; // Направление движения

        // Опционально: Отражение спрайта
        // if (mainSpriteRenderer != null)
        // {
        //     if (moveHorizontal > 0.01f) // Движение вправо
        //     {
        //         mainSpriteRenderer.flipX = false;
        //     }
        //     else if (moveHorizontal < -0.01f) // Движение влево
        //     {
        //         mainSpriteRenderer.flipX = true;
        //     }
        // }

        if (moveDirection != Vector2.zero) // Двигаемся только если есть ввод
        {
            float distanceToMove = speed * Time.fixedDeltaTime;

            // Определяем маску слоев, с которыми мы хотим сталкиваться.
            // Убедись, что игрок на слое "Player", а стены на слоях, которые не "Player".
            // Этот LayerMask будет сталкиваться со всем, КРОМЕ слоя "Player".
            int layerMaskForCast = ~LayerMask.GetMask("Player");
            // Если хочешь указать конкретные слои для столкновения (например, "Walls", "Obstacles"):
            // int layerMaskForCast = LayerMask.GetMask("Walls", "Obstacles", "Default");

            // Настраиваем ContactFilter2D
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(layerMaskForCast); // Устанавливаем маску слоев
            contactFilter.useTriggers = false; // Мы не хотим сталкиваться с триггерами этим Cast'ом

            // Массив для хранения результатов Cast. Даже если ожидаем одно столкновение, нужен массив.
            RaycastHit2D[] hits = new RaycastHit2D[1];

            // Выполняем Cast от имени коллайдера игрока
            // playerCollider.Cast возвращает количество обнаруженных столкновений
            int numHits = playerCollider.Cast(moveDirection, contactFilter, hits, distanceToMove);

            Vector2 finalMovementThisFrame;

            if (numHits > 0) // Если Cast что-то задел
            {
                // hits[0] содержит информацию о ближайшем столкновении
                RaycastHit2D hit = hits[0];
                // Мы столкнулись! Перемещаемся только до точки столкновения
                // hit.distance - это расстояние до препятствия.
                // Уменьшаем его на небольшое значение (skin width), чтобы не "прилипать" и не проходить сквозь тонкие стены.
                float skinWidth = 0.01f; // Можешь подобрать это значение
                finalMovementThisFrame = moveDirection * Mathf.Max(0, hit.distance - skinWidth);
                Debug.Log($"Cast hit: {hit.collider.name} at distance {hit.distance}. Moving by {finalMovementThisFrame.magnitude}");
            }
            else
            {
                // Препятствий нет, двигаемся на полную рассчитанную дистанцию
                finalMovementThisFrame = moveDirection * distanceToMove;
                // Debug.Log($"No cast hit. Moving by {finalMovementThisFrame.magnitude}"); // Можно раскомментировать для отладки
            }

            // Теперь двигаем Rigidbody на рассчитанное безопасное расстояние
            rb.MovePosition(rb.position + finalMovementThisFrame);
            // Debug.Log($"Current Pos: {rb.position}, Moved by: {finalMovementThisFrame}"); // Можно раскомментировать для отладки
        }
    }

    // Методы для отладки столкновений остаются полезными
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"COLLISION ENTER with: {collision.gameObject.name} on layer {LayerMask.LayerToName(collision.gameObject.layer)}");
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log($"COLLISION STAY with: {collision.gameObject.name}");
    }
}