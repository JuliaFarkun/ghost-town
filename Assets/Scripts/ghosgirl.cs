// GirlMovement.cs
using UnityEngine;

public class GirlMovement : MonoBehaviour
{
    [Header("Настройки Маршрута")]
    public Transform[] waypoints;
    public float pointReachedThreshold = 0.1f;

    [Header("Настройки Движения")]
    public float moveSpeed = 2f;

    [Header("Взаимодействие с Героем")]
    public Transform playerTransform;
    public float followDistance = 10f;
    public bool stopWhenPlayerOutOfRange = true; // Новый флаг: останавливаться ли, если игрок далеко

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private int currentWaypointIndex = 0;
    private bool isMovingThisFrame = false;
    private bool playerInRange = false;
    private bool pathCompleted = false; // Флаг, что весь маршрут пройден

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null) Debug.LogError("GirlMovement: SpriteRenderer не найден!");
        if (rb == null) Debug.LogWarning("GirlMovement: Rigidbody2D не найден. Движение будет через transform.position.");
        else if (rb.bodyType != RigidbodyType2D.Kinematic) Debug.LogWarning("GirlMovement: Rigidbody2D не Kinematic. Рекомендуется Kinematic.");

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("GirlMovement: Точки маршрута (waypoints) не заданы!");
            enabled = false;
            return;
        }

        // Поиск игрока, если не назначен
        if (playerTransform == null)
        {
            Debug.LogWarning("GirlMovement: Transform игрока (playerTransform) не задан. Пытаемся найти по тегу 'Player'.");
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player"); // Убедись, что у игрока есть тег "Player"
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
                Debug.Log("GirlMovement: Игрок найден по тегу 'Player'.");
            }
            else Debug.LogError("GirlMovement: Игрок не найден по тегу 'Player' и не задан вручную!");
        }
    }

    void Update()
    {
        // --- ДОБАВЛЕНО: Проверка глобальной паузы ---
        if (PlayerController.isGamePaused)
        {
            isMovingThisFrame = false; // Останавливаем движение в этот кадр
            // Опционально: остановить анимацию девочки, если она есть
            // Animator girlAnimator = GetComponent<Animator>();
            // if (girlAnimator != null) girlAnimator.speed = 0; 
            return; // Ничего не делаем, если игра на глобальной паузе
        }
        // --- Конец добавленной проверки ---
        // else 
        // {
        //    // Если была анимация, возобновить ее
        //    // Animator girlAnimator = GetComponent<Animator>();
        //    // if (girlAnimator != null) girlAnimator.speed = 1;
        // }


        isMovingThisFrame = false; // Сбрасываем флаг каждый кадр
        CheckPlayerDistance();

        // Двигаемся, только если путь не завершен И (игрок в радиусе ИЛИ мы не должны останавливаться из-за игрока)
        if (!pathCompleted && (playerInRange || !stopWhenPlayerOutOfRange) && waypoints.Length > 0 && currentWaypointIndex < waypoints.Length)
        {
            MoveTowardsWaypoint();
        }
        
        if(isMovingThisFrame) 
        {
            UpdateSpriteDirection();
        }
    }

    void CheckPlayerDistance()
    {
        if (playerTransform == null)
        {
            // Если игрок не назначен, решаем, как должна вести себя девочка.
            // Например, всегда считать игрока "в радиусе" или всегда "вне радиуса".
            // Если stopWhenPlayerOutOfRange = true, и игрока нет, то playerInRange = false остановит ее.
            playerInRange = !stopWhenPlayerOutOfRange; // Если нужно останавливаться без игрока, то false. Если двигаться - true.
            return;
        }
        playerInRange = Vector2.Distance(transform.position, playerTransform.position) <= followDistance;
    }

    void MoveTowardsWaypoint()
    {
        if (pathCompleted) return; // Если путь уже завершен, не двигаемся

        if (currentWaypointIndex >= waypoints.Length || waypoints[currentWaypointIndex] == null)
        {
            Debug.Log("GirlMovement: Достигнута невалидная точка или конец массива waypoints. Проверяем, был ли это конец маршрута.");
            // Эта ситуация не должна возникать, если pathCompleted устанавливается правильно
            pathCompleted = true; // Считаем путь завершенным, если вышли за пределы
            isMovingThisFrame = false;
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector2 currentPosition = (rb != null && rb.bodyType == RigidbodyType2D.Kinematic) ? rb.position : (Vector2)transform.position;
        float distanceToTarget = Vector2.Distance(currentPosition, targetWaypoint.position);

        if (distanceToTarget < pointReachedThreshold)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                Debug.Log("GirlMovement: Маршрут завершен (достигнута последняя точка).");
                pathCompleted = true; // Устанавливаем флаг, что путь пройден
                isMovingThisFrame = false;
                // НЕ УСТАНАВЛИВАЕМ currentWaypointIndex = 0; здесь, если не хотим зацикливания
                // Девочка просто остановится.
                return;
            }
            // Пересчитываем цель на новую точку
            if (waypoints[currentWaypointIndex] == null) {
                Debug.LogError($"GirlMovement: Следующая точка waypoints[{currentWaypointIndex}] не назначена (null)!");
                pathCompleted = true; // Ошибка в маршруте, останавливаемся
                isMovingThisFrame = false;
                return;
            }
            targetWaypoint = waypoints[currentWaypointIndex];
            // Пересчитываем distanceToTarget, так как цель изменилась
            distanceToTarget = Vector2.Distance(currentPosition, targetWaypoint.position); 
        }
        
        // Двигаемся, если мы все еще не достигли порога НОВОЙ (или текущей, если не переключились) точки
        // И если путь еще не помечен как завершенный
        if (!pathCompleted && distanceToTarget >= pointReachedThreshold) 
        {
            Vector2 directionToWaypoint = ((Vector2)targetWaypoint.position - currentPosition).normalized;
            float step = moveSpeed * Time.deltaTime;
            isMovingThisFrame = true;

            if (rb != null && rb.bodyType == RigidbodyType2D.Kinematic)
            {
                Vector2 moveAmount = directionToWaypoint * Mathf.Min(step, distanceToTarget);
                rb.MovePosition(currentPosition + moveAmount);
            }
            else
            {
                transform.position = Vector2.MoveTowards(currentPosition, targetWaypoint.position, step);
            }
        } else if (!pathCompleted) {
            // Мы близко к точке, но еще не переключились или это последняя точка в пределах threshold
            // В этом случае isMovingThisFrame может остаться false, если предыдущее условие не выполнилось
             isMovingThisFrame = false; // Явно указываем, что активного движения в этот кадр к точке нет
        }
    }

    void UpdateSpriteDirection()
    {
        // Обновляем направление, только если есть цель и спрайт
        if (spriteRenderer == null || pathCompleted || waypoints.Length == 0 || currentWaypointIndex >= waypoints.Length || waypoints[currentWaypointIndex] == null)
        {
            return;
        }

        Vector2 currentPos = (rb != null && rb.bodyType == RigidbodyType2D.Kinematic) ? rb.position : (Vector2)transform.position;
        Vector2 directionToTarget = ((Vector2)waypoints[currentWaypointIndex].position - currentPos);

        if (Mathf.Abs(directionToTarget.x) > 0.01f) // Небольшой порог, чтобы не менять направление при очень малом смещении
        {
            spriteRenderer.flipX = directionToTarget.x < 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.3f);
                    if (i > 0 && waypoints[i-1] != null) Gizmos.DrawLine(waypoints[i-1].position, waypoints[i].position);
                }
            }
        }
        if (playerTransform != null)
        {
            Gizmos.color = playerInRange ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, followDistance);
        }
    }
    
    public int GetCurrentWaypointIndex()
    {
        return currentWaypointIndex;
    }
    
    public void SetCurrentWaypointIndex(int index)
    {
        if (waypoints != null && index >= 0 && index < waypoints.Length)
        {
            currentWaypointIndex = index;
            pathCompleted = false; // Сбрасываем флаг завершения пути, так как мы установили новую точку
            isMovingThisFrame = true; // Предполагаем, что после установки индекса она должна начать двигаться (или хотя бы обновить направление)
            Debug.Log($"[GirlMovement] Waypoint index установлен на: {currentWaypointIndex}");
        }
        else
        {
            Debug.LogWarning($"[GirlMovement] Попытка установить некорректный waypoint index: {index}. Индекс не изменен.");
            // Если индекс некорректен, НЕ сбрасываем currentWaypointIndex на 0, чтобы она не прыгала к началу,
            // а оставалась на своей текущей валидной точке (если она была).
            // Если нужно поведение сброса на 0 при ошибке, можно раскомментировать:
            // currentWaypointIndex = 0; 
            // pathCompleted = false;
        }
    }
}