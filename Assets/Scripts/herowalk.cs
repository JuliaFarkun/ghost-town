using UnityEngine;

public class HeroController : MonoBehaviour
{
    public float speed = 3f;
    public AudioSource audioSource; // Добавьте это поле для ссылки на компонент AudioSource
    public AudioClip walkSound;     // Добавьте это поле для аудиоклипа шагов
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
        audioSource = GetComponent<AudioSource>(); // Получаем ссылку на AudioSource

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
        if (audioSource == null)
        {
            Debug.LogWarning("HeroController: AudioSource не найден на объекте " + gameObject.name + ". Звуки не будут работать.");
        }

        if (rb.bodyType != RigidbodyType2D.Kinematic)
        {
            Debug.LogWarning("HeroController: Rigidbody2D не установлен в Kinematic. " +
                             "Для корректной работы с MovePosition и Cast рекомендуется Kinematic тип.");
        }
        rb.useFullKinematicContacts = true; // Для Kinematic тел, чтобы регистрировать столкновения

        if (animator != null) animator.Play(IDLE_ANIMATION);
        // lastNonZeroMovement = new Vector2(0, -1); // Начальное направление для idle (если нужно)

        // Логика управления звуком шагов
        if (audioSource != null && walkSound != null)
        {
            if (movementInput != Vector2.zero)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = walkSound;
                    audioSource.loop = true; // Зацикливаем звук шагов пока идем
                    audioSource.Play();
                }
            }
            else
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
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
            // ... (ваш существующий код для анимации и отражения) ...

            // Логика управления звуком шагов при движении
            if (audioSource != null && walkSound != null)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = walkSound;
                    audioSource.loop = true; // Зацикливаем звук шагов пока идем
                    audioSource.Play();
                }
            }
        }
        else // movementInput == Vector2.zero
        {
            // Нет движения, играем Idle
            animator.Play(IDLE_ANIMATION);

            // Логика управления звуком шагов при остановке
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
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