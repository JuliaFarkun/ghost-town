// FinalBattleTrigger.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Для SceneManager

public class FinalBattleTrigger : MonoBehaviour
{
    [Header("Настройки")]
    public string finalBattleSceneName = "FinalBattleScene"; // ТОЧНОЕ имя файла твоей сцены финального боя
    public string playerTag = "Player"; // Тег объекта игрока

    private bool battleTriggered = false; // Чтобы сцена не загружалась многократно

    // Убедись, что у этого GameObject'а есть Collider2D с включенным "Is Trigger",
    // а у игрока есть Rigidbody2D и Collider2D (не триггер).

    void OnTriggerEnter2D(Collider2D other)
    {
        if (battleTriggered) return; // Если уже сработало, ничего не делаем

        if (other.CompareTag(playerTag))
        {
            Debug.Log($"Player entered the Final Battle Trigger! Loading scene: {finalBattleSceneName}");
            battleTriggered = true; // Помечаем, что сработало

            // Перед загрузкой сцены финального боя, ты можешь захотеть:
            // 1. Сохранить текущее состояние игры (если у тебя есть система сохранений).
            // 2. Приостановить игрока (хотя при загрузке новой сцены он и так "остановится").
            // PlayerController.isGamePaused = true; // Если нужно

            // 3. Возможно, передать какие-то данные в сцену финального боя через BattleDataHolder или другой механизм,
            //    например, текущее здоровье игрока, если оно не сохраняется персистентно через PlayerStats.
            //    Если PlayerStats с его статическим persistentCurrentHealth работает, то это может быть не нужно.

            // Загружаем сцену финального боя
            SceneManager.LoadScene(finalBattleSceneName);
        }
    }

    // Опционально: для визуализации триггера в редакторе
    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = new Color(0.8f, 0, 0.8f, 0.4f); // Полупрозрачный фиолетовый
            if (col is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius * Mathf.Max(transform.localScale.x, transform.localScale.y));
            }
            else if (col is BoxCollider2D box)
            {
                Gizmos.DrawCube(transform.position + (Vector3)box.offset, Vector3.Scale(box.size, transform.localScale));
            }
        }
    }
}