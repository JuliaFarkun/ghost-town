using UnityEngine;

public class HeroController : MonoBehaviour
{
    public float speed = 3f;
    private Animator animator;
    private Vector2 movementInput; // Вектор для хранения ввода
    private Rigidbody2D rb;        // Ссылка на Rigidbody2D
    private Collider2D heroCollider; // Ссылка на Collider2D для Cast
    private SpriteRenderer spriteRenderer;

    // Имена состояний анимации (убедись, что они точно совпадают с именами в твоем Animator Controller)
    private const string IDLE_ANIMATION = "idle";
    private const string WALK_FRONT_ANIMATION = "walkright"; // Ходьба вниз (лицом к камере)
    private const string WALK_BACK_ANIMATION = "walkback";  // Ходьба вверх (спиной к камере)
    private const string WALK_SIDE_ANIMATION = "walk";      // Ходьба вбок (влево/вправо)

    // private Vector2 lastNonZeroMovement; // Пока не используется, но может пригодиться для idle-направления

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        heroCollider = GetComponent<Collider2D>(); // Получаем Collider2D

        if (rb == null)
        {
            Debug.LogError("HeroController: Rigidbody2D не найден на объекте " + gameObject.name + "!");
            enabled = false;
            return;
        }
        if (heroCollider == null)
        {
            Debug.LogError("HeroController: Collider2D не найден на объекте " + gameObject.name + "! Cast для столкновений не будет работать.");
            enabled = false;
            return;
        }
        if (animator == null)
        {
            Debug.LogWarning("HeroController: Animator не найден на объекте " + gameObject.name + ". Анимации не будут работать.");
        }
        if (spriteRenderer == null)
        {
            Debug.LogWarning("HeroController: SpriteRenderer не найден на объекте " + gameObject.name + ". Отражение спрайта может не работать.");
        }


        if (rb.bodyType != RigidbodyType2D.Kinematic)
        {
            Debug.LogWarning("HeroController: Rigidbody2D не установлен в Kinematic. " +
                             "Для корректной работы с MovePosition и Cast рекомендуется Kinematic тип.");
        }
        rb.useFullKinematicContacts = true; // Для Kinematic тел, чтобы регистрировать столкновения

        if (animator != null) animator.Play(IDLE_ANIMATION);
        // lastNonZeroMovement = new Vector2(0, -1); // Начальное направление для idle (если нужно)
    }

    void Update()
    {
        // Сюда можно добавить проверку на паузу, если она у тебя глобальная
        // if (GameManager.Instance.IsPaused) { movementInput = Vector2.zero; return; }

        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(moveHorizontal, moveVertical).normalized;

        UpdateAnimationAndSprite();
    }

    void FixedUpdate()
    {
        // if (GameManager.Instance.IsPaused) { rb.velocity = Vector2.zero; return; } // Убедимся, что остановлен при паузе

        if (movementInput != Vector2.zero) // Двигаемся только если есть ввод
        {
            float distanceToMove = speed * Time.fixedDeltaTime;

            // Определяем маску слоев, с которыми мы хотим сталкиваться.
            // Игрок должен быть на слое "Player" (или другом, которого здесь нет).
            // Стены/препятствия на слоях, которые указаны здесь.
            int layerMaskForCast = LayerMask.GetMask("Default", "Wall"); // Примеры слоев, измени на свои
            // Альтернативно: сталкиваться со всем, КРОМЕ слоя "Player"
            // int layerMaskForCast = ~LayerMask.GetMask("Player");

            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(layerMaskForCast);
            contactFilter.useTriggers = false; // Не сталкиваемся с триггерами

            RaycastHit2D[] hits = new RaycastHit2D[1]; // Массив для результатов Cast

            // Выполняем Cast от имени коллайдера героя
            int numHits = heroCollider.Cast(movementInput, contactFilter, hits, distanceToMove);

            Vector2 finalMovementThisFrame;

            if (numHits > 0) // Если Cast что-то задел
            {
                RaycastHit2D hit = hits[0];
                float skinWidth = 0.01f; // Небольшой отступ от стены
                finalMovementThisFrame = movementInput * Mathf.Max(0, hit.distance - skinWidth);
                // Debug.Log($"Collision with {hit.collider.name}. Moving by {finalMovementThisFrame.magnitude}");
            }
            else
            {
                // Препятствий нет, двигаемся на полную рассчитанную дистанцию
                finalMovementThisFrame = movementInput * distanceToMove;
            }

            // Двигаем Rigidbody на рассчитанную (возможно, скорректированную) позицию
            rb.MovePosition(rb.position + finalMovementThisFrame);
        }
        // Если нет ввода (movementInput == Vector2.zero), то rb.MovePosition не вызывается,
        // и персонаж остается на месте, что корректно.
    }

    void UpdateAnimationAndSprite()
    {
        if (animator == null || spriteRenderer == null) return; // Если нет аниматора или спрайта, ничего не делаем

        if (movementInput != Vector2.zero)
        {
            // lastNonZeroMovement = movementInput; // Если нужно запоминать последнее направление для idle

            // Отражение спрайта по X
            if (movementInput.x > 0.01f)
            {
                spriteRenderer.flipX = false;
            }
            else if (movementInput.x < -0.01f)
            {
                spriteRenderer.flipX = true;
            }
            // Если movementInput.x близок к нулю, flipX не меняется (персонаж сохраняет направление)

            // Определение основной оси движения для выбора анимации
            if (Mathf.Abs(movementInput.y) > Mathf.Abs(movementInput.x))
            {
                // Вертикальное движение доминирует
                if (movementInput.y > 0)
                {
                    animator.Play(WALK_BACK_ANIMATION);
                }
                else
                {
                    animator.Play(WALK_FRONT_ANIMATION);
                }
            }
            else
            {
                // Горизонтальное движение доминирует или равно вертикальному
                // (или движение только по горизонтали)
                if (Mathf.Abs(movementInput.x) > 0.01f) // Убедимся, что есть горизонтальное движение
                {
                     animator.Play(WALK_SIDE_ANIMATION);
                }
                else // Если вдруг сюда попали с чисто вертикальным (хотя предыдущий блок должен был это обработать)
                {
                    if (movementInput.y > 0) animator.Play(WALK_BACK_ANIMATION);
                    else if (movementInput.y < 0) animator.Play(WALK_FRONT_ANIMATION);
                    else animator.Play(IDLE_ANIMATION); // Совсем нет движения
                }
            }
        }
        else
        {
            // Нет движения, играем Idle
            // Здесь можно было бы использовать lastNonZeroMovement для Idle анимации в определенном направлении,
            // но пока просто общая Idle.
            animator.Play(IDLE_ANIMATION);
        }
    }
    
    // Опционально: если хочешь, чтобы OnCollision... тоже работали,
    // они должны срабатывать, так как useFullKinematicContacts = true,
    // и Cast теперь предотвращает прохождение, давая шанс этим событиям сработать корректно.
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"HeroController: OnCollisionEnter2D with {collision.gameObject.name}");
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log($"HeroController: OnCollisionStay2D with {collision.gameObject.name}");
    }
}