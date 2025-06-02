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
    public Transform playerTransform; // Можно оставить null, если игрок ищется по тегу
    public float followDistance = 10f;
    public bool stopWhenPlayerOutOfRange = true;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private int currentWaypointIndex = 0;
    private bool isMovingThisFrame = false; // Используется для решения, нужно ли обновлять направление спрайта
    private bool playerInRange = false;
    private bool pathCompleted = false;

    // --- Методы для сохранения и загрузки ---
    /// <summary>
    /// Применяет загруженные данные к состоянию девочки.
    /// </summary>
    public void ApplySaveData(PlayerData data, Vector3 defaultGirlSpawnPos, int defaultGirlWaypoint)
    {
        Debug.Log($"[GirlMovement.ApplySaveData] Вызван. Has PlayerData: {data != null}, HasSavedGhostGirlState: {data?.hasSavedGhostGirlState}");
        if (data != null && data.hasSavedGhostGirlState)
        {
            Vector3 newPos = data.GetGhostGirlPosition();
            transform.position = newPos;
            SetCurrentWaypointIndexInternal(data.ghostGirlCurrentWaypointIndex); 
            Debug.Log($"[GirlMovement] Состояние девочки ПРИМЕНЕНО из PlayerData: Pos {transform.position} (пришло: {newPos}), Waypoint: {currentWaypointIndex}, PathCompleted: {pathCompleted}");
        }
        else
        {
            Debug.Log($"[GirlMovement.ApplySaveData] Используются дефолтные параметры. Default spawn: {defaultGirlSpawnPos}, Default waypoint: {defaultGirlWaypoint}");
            transform.position = defaultGirlSpawnPos;
            SetCurrentWaypointIndexInternal(defaultGirlWaypoint); 
            Debug.Log($"[GirlMovement] Применены ДЕФОЛТНЫЕ параметры для девочки: Pos {transform.position}, Waypoint: {currentWaypointIndex}, PathCompleted: {pathCompleted}");
        }
    }

    /// <summary>
    /// Заполняет объект PlayerData текущим состоянием девочки для сохранения.
    /// </summary>
    public void PopulateSaveData(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogError("[GirlMovement] PopulateSaveData вызван с null PlayerData!");
            return;
        }

        data.SetGhostGirlPosition(transform.position);
        data.ghostGirlCurrentWaypointIndex = GetCurrentWaypointIndex();
        data.hasSavedGhostGirlState = true; // Помечаем, что есть валидные данные для сохранения
        Debug.Log($"[GirlMovement] Данные девочки ЗАПОЛНЕНЫ для сохранения: Pos {transform.position}, Waypoint: {currentWaypointIndex}");
    }
    // -----------------------------------------

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null) Debug.LogError("GirlMovement: SpriteRenderer не найден!");
        if (rb == null) Debug.LogWarning("GirlMovement: Rigidbody2D не найден. Движение будет через transform.position.");
        else if (rb.bodyType != RigidbodyType2D.Kinematic) Debug.LogWarning("GirlMovement: Rigidbody2D не Kinematic. Рекомендуется Kinematic.");

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("GirlMovement: Точки маршрута (waypoints) не заданы! Девочка не будет двигаться.");
            pathCompleted = true; // Считаем путь завершенным, если нет точек
            enabled = false; // Можно отключить скрипт, если нет маршрута
            return;
        }

        if (playerTransform == null)
        {
            // Debug.LogWarning("GirlMovement: Transform игрока (playerTransform) не задан. Пытаемся найти по тегу 'Player'.");
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player"); 
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
                // Debug.Log("GirlMovement: Игрок найден по тегу 'Player'.");
            }
            // else Debug.LogError("GirlMovement: Игрок не найден по тегу 'Player' и не задан вручную! Логика followDistance может не работать.");
        }
        CheckIfPathCompleted(); // Проверка на случай, если currentWaypointIndex уже на конце пути
    }

    void Update()
    {
        if (PlayerController.isGamePaused)
        {
            isMovingThisFrame = false; 
            return; 
        }

        isMovingThisFrame = false; 
        CheckPlayerDistance();

        if (!pathCompleted && (playerInRange || !stopWhenPlayerOutOfRange) && CanMoveToWaypoint())
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
            playerInRange = !stopWhenPlayerOutOfRange; 
            return;
        }
        playerInRange = Vector2.Distance(transform.position, playerTransform.position) <= followDistance;
    }

    bool CanMoveToWaypoint() // Проверка, есть ли валидная текущая точка
    {
        return waypoints != null && waypoints.Length > 0 && currentWaypointIndex < waypoints.Length && waypoints[currentWaypointIndex] != null;
    }

    void MoveTowardsWaypoint()
    {
        if (pathCompleted || !CanMoveToWaypoint()) 
        {
            isMovingThisFrame = false;
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector2 currentPosition = (rb != null && rb.bodyType == RigidbodyType2D.Kinematic) ? rb.position : (Vector2)transform.position;
        float distanceToTarget = Vector2.Distance(currentPosition, targetWaypoint.position);

        if (distanceToTarget < pointReachedThreshold)
        {
            currentWaypointIndex++;
            CheckIfPathCompleted(); // Проверяем, не завершился ли путь

            if (pathCompleted) {
                 Debug.Log("GirlMovement: Маршрут завершен (достигнута последняя точка).");
                 isMovingThisFrame = false;
                 return;
            }
            // Если путь не завершен, то currentWaypointIndex указывает на следующую валидную точку (проверено в CheckIfPathCompleted)
            // Обновляем цель для возможного движения в том же кадре, если новая точка далеко
            if (!CanMoveToWaypoint()) // Дополнительная проверка после инкремента
            {
                Debug.LogError($"GirlMovement: Ошибка в логике маршрута, следующая точка waypoints[{currentWaypointIndex}] невалидна после инкремента.");
                pathCompleted = true;
                isMovingThisFrame = false;
                return;
            }
            targetWaypoint = waypoints[currentWaypointIndex];
            distanceToTarget = Vector2.Distance(currentPosition, targetWaypoint.position); 
        }
        
        // Двигаемся, если мы все еще не достигли порога НОВОЙ (или текущей) точки
        // и если путь еще не помечен как завершенный
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
        } else {
             isMovingThisFrame = false; 
        }
    }

    void UpdateSpriteDirection()
    {
        if (spriteRenderer == null || pathCompleted || !CanMoveToWaypoint())
        {
            return;
        }

        Vector2 currentPos = (rb != null && rb.bodyType == RigidbodyType2D.Kinematic) ? rb.position : (Vector2)transform.position;
        Vector2 directionToTarget = ((Vector2)waypoints[currentWaypointIndex].position - currentPos);

        if (Mathf.Abs(directionToTarget.x) > 0.01f) 
        {
            spriteRenderer.flipX = directionToTarget.x < 0;
        }
    }

    void CheckIfPathCompleted() // Вспомогательный метод
    {
        if (waypoints == null || waypoints.Length == 0) {
            pathCompleted = true;
            return;
        }
        pathCompleted = currentWaypointIndex >= waypoints.Length;
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints != null && waypoints.Length > 0) {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++) {
                if (waypoints[i] != null) {
                    Gizmos.DrawSphere(waypoints[i].position, 0.3f);
                    if (i > 0 && waypoints[i-1] != null) Gizmos.DrawLine(waypoints[i-1].position, waypoints[i].position);
                }
            }
        }
        if (playerTransform != null) {
            Gizmos.color = playerInRange ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, followDistance);
        }
    }
    
    public int GetCurrentWaypointIndex()
    {
        return currentWaypointIndex;
    }
    
    // Изменил SetCurrentWaypointIndex на приватный SetCurrentWaypointIndexInternal, чтобы он не вызывался случайно извне
    // и чтобы гарантировать, что pathCompleted обновляется.
    // GameSceneManager будет вызывать ApplySaveData.
    private void SetCurrentWaypointIndexInternal(int index)
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            // Позволяем currentWaypointIndex быть waypoints.Length, чтобы обозначить завершенный путь.
            // Mathf.Clamp гарантирует, что индекс находится в пределах [0, waypoints.Length].
            currentWaypointIndex = Mathf.Clamp(index, 0, waypoints.Length);
        }
        else // Нет точек маршрута
        {
            currentWaypointIndex = 0;
        }
        // Обновляем состояние pathCompleted сразу после установки currentWaypointIndex
        CheckIfPathCompleted();
        // Дополнительный Debug.Log для ясности состояния после установки индекса
        // Debug.Log($"[GirlMovement.SetCurrentWaypointIndexInternal] Установлен индекс: {currentWaypointIndex}, PathCompleted: {pathCompleted}");
    }
}