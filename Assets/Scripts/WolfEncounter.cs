// WolfEncounter.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class WolfEncounter : MonoBehaviour
{
    [Header("Настройки этого волка для боя")]
    public string wolfIdentifierForBattle = "DefaultWolf";

    [Header("Настройки взаимодействия")]
    public string playerTag = "Player";
    public string battleSceneName = "BattleScene"; 
    public string ghostGirlTag = "GhostGirl";

    private bool battleInitiated = false; // Этот флаг теперь менее важен, так как волк будет уничтожаться

    void OnTriggerEnter2D(Collider2D other)
    {
        // Если бой уже был инициирован И этот объект волка все еще существует (на всякий случай)
        if (battleInitiated && gameObject != null) return; 

        if (other.CompareTag(playerTag))
        {
            Debug.Log($"Player entered Wolf '{gameObject.name}' (ID: {wolfIdentifierForBattle}) trigger. Initiating battle!");
            
            BattleDataHolder.CurrentWolfIdentifier = wolfIdentifierForBattle;
            BattleDataHolder.ActiveWolfGameObjectName = gameObject.name;

            BattleDataHolder.PlayerPositionBeforeBattle = other.transform.position;
            BattleDataHolder.PlayerPositionSaved = true;

            GameObject ghostGirlObject = GameObject.FindGameObjectWithTag(ghostGirlTag);
            if (ghostGirlObject != null)
            {
                BattleDataHolder.GhostGirlPositionBeforeBattle = ghostGirlObject.transform.position;
                BattleDataHolder.GhostGirlPositionSaved = true;

                GirlMovement girlMovement = ghostGirlObject.GetComponent<GirlMovement>();
                if (girlMovement != null)
                {
                    BattleDataHolder.GhostGirlCurrentWaypointIndex = girlMovement.GetCurrentWaypointIndex();
                    BattleDataHolder.GhostGirlWaypointSaved = true;
                    Debug.Log($"[WolfEncounter] Сохранен индекс точки девочки: {BattleDataHolder.GhostGirlCurrentWaypointIndex}");
                } else {
                    BattleDataHolder.GhostGirlWaypointSaved = false;
                    Debug.LogWarning($"[WolfEncounter] Скрипт GirlMovement не найден на объекте девочки с тегом '{ghostGirlTag}'. Индекс точки не сохранен.");
                }
            } else {
                BattleDataHolder.GhostGirlPositionSaved = false;
                BattleDataHolder.GhostGirlWaypointSaved = false;
            }

            // <--- ДОБАВИТЬ СОХРАНЕНИЕ ИГРЫ ЗДЕСЬ
            if (GameSceneManager.Instance != null)
            {
                GameSceneManager.Instance.SaveCurrentGameState();
                Debug.Log($"[WolfEncounter] Текущее состояние игры сохранено перед боем с {wolfIdentifierForBattle}.");
            }
            else
            {
                Debug.LogError($"[WolfEncounter] GameSceneManager.Instance не найден! Не могу сохранить игру перед боем с {wolfIdentifierForBattle}.");
            }
            // --->

            PlayerController.isGamePaused = true; 
            Debug.Log("PlayerController.isGamePaused установлен в true из WolfEncounter");
            
            battleInitiated = true; 
            SceneManager.LoadScene(battleSceneName);
        }
    }

    // Этот метод будет вызван из GameSceneManager при любом исходе боя, приводящем к исчезновению волка
    public void ResolveEncounter()
    {
        Debug.Log($"Волк {gameObject.name} (ID: {wolfIdentifierForBattle}) завершил взаимодействие. Уничтожается.");
        Destroy(gameObject); 
    }

    // Методы HandleBattleWon и HandleBattleLostOrOther теперь можно объединить в один или вызывать ResolveEncounter
    // Оставим их для возможной разной логики в будущем, но оба будут уничтожать волка
    public void HandleBattleWon()
    {
        ResolveEncounter();
    }

    public void HandleBattleLostOrOther()
    {
        ResolveEncounter(); // Волк исчезает и при проигрыше
    }

    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
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