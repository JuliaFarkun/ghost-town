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
        if (animator == null || spriteRenderer == null) return;

        // Используем имена анимаций, как в herowalk_right.cs (предполагается, что они определены как константы в этом классе herowalk.cs)
        // const string IDLE_ANIMATION = "idle";
        // const string WALK_FRONT_ANIMATION = "walkright";
        // const string WALK_BACK_ANIMATION = "walkback";
        // const string WALK_SIDE_ANIMATION = "walk";

        if (movementInput != Vector2.zero) // Если есть движение
        {
            // --- Начало логики анимации и отражения (основано на herowalk_right.cs) ---
            // Приоритет вертикальному движению
            if (Mathf.Abs(movementInput.y) > Mathf.Abs(movementInput.x))
            {
                if (movementInput.y > 0)
                {
                    animator.Play(WALK_BACK_ANIMATION);
                }
                else
                {
                    animator.Play(WALK_FRONT_ANIMATION); // "walkright" для движения вниз
                }
                // Примечание: flipX для чисто вертикального движения здесь не меняется,
                // сохраняя последнее горизонтальное направление или дефолтное.
            }
            // Иначе, если доминирует (или равно) горизонтальное движение
            else if (Mathf.Abs(movementInput.x) > 0.01f) 
            {
                if (movementInput.x > 0) // Движение вправо
                {
                    animator.Play(WALK_FRONT_ANIMATION); // "walkright" для движения вправо
                    spriteRenderer.flipX = false; // Предполагаем, что "walkright" смотрит вправо по умолчанию
                }
                else // Движение влево (movementInput.x < 0)
                {
                    animator.Play(WALK_SIDE_ANIMATION); // "walk" для движения влево
                    spriteRenderer.flipX = true; // Предполагаем, что "walk" (базовая) смотрит вправо и ее надо отразить
                }
            }
            // Если движение очень мало и не подпадает под условия выше,
            // будет сохранена последняя активная анимация ходьбы.
            // --- Конец логики анимации и отражения ---

            // --- Сохраненная логика звука шагов (из оригинального herowalk.cs) ---
            if (audioSource != null && walkSound != null)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = walkSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            // --- Конец логики звука шагов ---
        }
        else // Нет движения
        {
            animator.Play(IDLE_ANIMATION);

            // --- Сохраненная логика остановки звука шагов (из оригинального herowalk.cs) ---
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            // --- Конец логики остановки звука шагов ---
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