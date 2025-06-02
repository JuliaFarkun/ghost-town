// PlayerStats.cs
using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100;
    private int _currentHealth;

    // Статический флаг, чтобы инициализировать здоровье только один раз
    private static bool initialHealthSet = false;
    // Статическое поле для хранения здоровья между загрузками сцен, ЕСЛИ объект игрока уничтожается
    // Если используешь DontDestroyOnLoad на игроке, это поле не нужно, _currentHealth сохранится сам.
    private static int persistentCurrentHealth = -1; 

    public int CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = Mathf.Clamp(value, 0, maxHealth);
            persistentCurrentHealth = _currentHealth; // Обновляем сохраненное значение
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }
    }

    public static event Action<int, int> OnHealthChanged;

    void Awake()
    {
        // Если это ПЕРВЫЙ раз, когда PlayerStats просыпается в игре ИЛИ если здоровье не было восстановлено
        if (!initialHealthSet || persistentCurrentHealth == -1)
        {
            CurrentHealth = maxHealth;
            initialHealthSet = true;
            Debug.Log($"[PlayerStats] Начальное здоровье установлено: {CurrentHealth}/{maxHealth}");
        }
        else
        {
            // Восстанавливаем здоровье из сохраненного статического значения
            // Это сработает, если GameObject игрока пересоздается при загрузке сцены
            _currentHealth = persistentCurrentHealth; // Устанавливаем _currentHealth напрямую, чтобы не вызвать лишний Invoke
            Debug.Log($"[PlayerStats] Здоровье восстановлено: {_currentHealth}/{maxHealth}");
            // Вызываем событие, чтобы UI обновился с восстановленным значением
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }
    }

    // ... (TakeDamage, Heal, Die, TestDamage остаются без изменений) ...
    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        Debug.Log($"Player took {amount} damage. Current Health: {CurrentHealth}");
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        Debug.Log($"Player healed {amount}. Current Health: {CurrentHealth}");
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // Здесь логика смерти: перезагрузка уровня, экран Game Over и т.д.
    }

    [ContextMenu("Test Take 10 Damage")]
    void TestDamage()
    {
        TakeDamage(10);
    }
}