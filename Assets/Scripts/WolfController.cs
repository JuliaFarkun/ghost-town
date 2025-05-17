using UnityEngine;

public class WolfController : MonoBehaviour
{
    public float detectionRadius = 3f; // Радиус обнаружения игрока
    public Transform player; // Ссылка на игрока

    // Имя сцены боя
    public string battleSceneName = "BattleScene"; // Убедись, что такая сцена будет создана и добавлена в Build Settings

    // Флаг, чтобы бой не запускался многократно
    private bool battleTriggered = false;


    void Start()
    {
        // Пытаемся найти игрока по тегу "Player"
        // Не забудь присвоить тег "Player" твоему GameObject'у игрока в Inspector!
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player GameObject has the 'Player' tag.");
        }
    }

    void Update()
    {
        if (player == null || battleTriggered || PlayerController.isGamePaused) // Не работаем, если игрок не найден, бой уже начат или игра на паузе
        {
            return;
        }

        // Проверяем дистанцию до игрока
        if (Vector2.Distance(transform.position, player.position) < detectionRadius)
        {
            Debug.Log("Player detected! Triggering battle (placeholder).");
            battleTriggered = true; // Ставим флаг, чтобы не вызывать постоянно
            // Здесь будет логика перехода на сцену боя
            // Например, можно сохранить состояние текущей игры, а затем загрузить сцену боя
            // Time.timeScale = 0f; // Приостановить основной геймплей
            // SceneManager.LoadScene(battleSceneName, LoadSceneMode.Additive); // или обычный LoadScene
            // Пока что просто выведем сообщение. Полноценный переход обсудим позже.
            // Для демонстрации можно просто загрузить сцену, но это прервет текущую.
            // SceneManager.LoadScene(battleSceneName);
        }
    }

    // Отрисовка радиуса обнаружения в редакторе для удобства
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}