// FinalBattleTrigger.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalBattleTrigger : MonoBehaviour
{
    [Header("Настройки")]
    public string finalBattleSceneName = "FinalBattleScene"; 
    public string playerTag = "Player"; 

    private bool battleTriggered = false; 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (battleTriggered) return; 

        if (other.CompareTag(playerTag))
        {
            Debug.Log($"Player entered the Final Battle Trigger! Loading scene: {finalBattleSceneName}");
            battleTriggered = true; 
            
            // СОХРАНЯЕМ ТЕКУЩЕЕ СОСТОЯНИЕ ИГРЫ ПЕРЕД ФИНАЛЬНЫМ БОЕМ
            if (GameSceneManager.Instance != null)
            {
                GameSceneManager.Instance.SaveCurrentGameState();
                Debug.Log("[FinalBattleTrigger] Текущее состояние игры сохранено через GameSceneManager.");
            }
            else
            {
                Debug.LogError("[FinalBattleTrigger] GameSceneManager.Instance не найден! Не могу сохранить игру.");
            }

            // Данные для BattleDataHolder (если финальному бою нужны какие-то особые идентификаторы)
            // BattleDataHolder.CurrentWolfIdentifier = "FINAL_BOSS"; // Пример
            // BattleDataHolder.ActiveWolfGameObjectName = "FinalBossObjectOnMap"; // Если он есть на карте
            // Но чаще всего финальный бой - это уникальная сцена, и BattleDataHolder может не использоваться
            // для передачи типа врага, а только для возврата результата.
            // Или можно не использовать BattleDataHolder для финального боя вообще, если он не возвращает урон/исход.
            // Для простоты, пока не будем ничего передавать через BattleDataHolder для финального боя,
            // если только его скрипт не ожидает этого.

            PlayerController.isGamePaused = true; 
            SceneManager.LoadScene(finalBattleSceneName);
        }
    }
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