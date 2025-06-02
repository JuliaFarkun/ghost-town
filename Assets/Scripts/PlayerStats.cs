// PlayerStats.cs
using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100; // Это значение будет инициализировано из PlayerData
    private int _currentHealthInternal;

    public int CurrentHealth
    {
        get { return _currentHealthInternal; }
        private set 
        {
            _currentHealthInternal = Mathf.Clamp(value, 0, maxHealth);
            OnHealthChanged?.Invoke(_currentHealthInternal, maxHealth);
            // Debug.Log($"[PlayerStats] CurrentHealth установлен в: {_currentHealthInternal}");
        }
    }

    public static event Action<int, int> OnHealthChanged;

    // Вызывается из GameSceneManager для применения загруженных/новых данных
    public void ApplySaveData(PlayerData data, Vector3 defaultPlayerSpawnPos)
    {
        Debug.Log($"[PlayerStats.ApplySaveData] Вызван. Has PlayerData: {data != null}, HasSavedPlayerState: {data?.hasSavedPlayerState}");
        if (data != null && data.hasSavedPlayerState)
        {
            this.maxHealth = data.maxPlayerHealth;
            this.CurrentHealth = data.currentPlayerHealth; // Используем сеттер для обновления UI
            Vector3 newPos = data.GetPlayerPosition();
            transform.position = newPos; // Восстанавливаем позицию
            Debug.Log($"[PlayerStats] Статы игрока ПРИМЕНЕНЫ из PlayerData: HP {CurrentHealth}/{maxHealth}, Pos: {transform.position} (пришло: {newPos})");
        }
        else
        {
            Debug.Log($"[PlayerStats.ApplySaveData] Используются дефолтные статы. Default spawn: {defaultPlayerSpawnPos}");
            // Это новая игра или файл сохранения поврежден/отсутствует
            this.maxHealth = 100; // Дефолтное значение, если не загружено
            this.CurrentHealth = this.maxHealth;
            transform.position = defaultPlayerSpawnPos; // Устанавливаем на дефолтную позицию
            Debug.Log($"[PlayerStats] Применены ДЕФОЛТНЫЕ статы (новая игра): HP {CurrentHealth}/{maxHealth}, Pos: {transform.position}");
        }
    }
    
    // Вызывается из GameSceneManager для сбора данных перед сохранением
    public void PopulateSaveData(PlayerData data)
    {
        if (data == null) return;

        data.currentPlayerHealth = this.CurrentHealth;
        data.maxPlayerHealth = this.maxHealth;
        data.SetPlayerPosition(transform.position);
        data.hasSavedPlayerState = true;
    }

    // Awake теперь может быть пустым, так как GameSceneManager управляет инициализацией
    void Awake()
    {
        Debug.Log("[PlayerStats] Awake. Инициализация здоровья и позиции будет из GameSceneManager.");
    }

    public void TakeDamage(int damage)
    {
        int previousHealth = CurrentHealth;
        CurrentHealth -= damage;
        Debug.Log($"[PlayerStats.TakeDamage] Получен урон: {damage}. Здоровье изменено с {previousHealth} на {CurrentHealth}.");
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Debug.Log("[PlayerStats.TakeDamage] Здоровье игрока достигло 0 или ниже.");
            // Логика смерти или Game Over теперь обрабатывается в GameSceneManager.CheckForGameOver()
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount; 
        Debug.Log($"[PlayerStats] Игрок вылечился на {amount}. Текущее здоровье: {CurrentHealth}");
    }

    void Die()
    {
        Debug.Log("[PlayerStats] Игрок УМЕР! (CurrentHealth <= 0)");
        // GameSceneManager обнаружит это и загрузит GameOver.
    }
}