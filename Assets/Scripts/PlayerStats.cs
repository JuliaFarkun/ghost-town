// PlayerStats.cs
using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100;
    private int _currentHealth;

    // Статические поля для сохранения состояния между игровыми сессиями (пока игра запущена)
    private static bool gameWasStartedOrContinued = false; // Флаг, что была нажата "Новая игра" или "Продолжить"
    private static int persistentPlayerHealth; 

    public int CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = Mathf.Clamp(value, 0, maxHealth);
            // Обновляем "сохраненное" значение, когда здоровье активно меняется в игре
            if (gameWasStartedOrContinued) // Только если игра действительно идет
            {
                persistentPlayerHealth = _currentHealth;
            }
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }
    }

    public static event Action<int, int> OnHealthChanged;

    // Вызывается из MainMenuController при нажатии "Новая игра"
    public static void InitializeForNewGame()
    {
        persistentPlayerHealth = 0; // Будет установлено в maxHealth в Awake
        gameWasStartedOrContinued = true; // Помечаем, что можно "продолжить" эту сессию
                                           // но Awake установит здоровье на максимум.
        PlayerController.isGamePaused = false;
        Time.timeScale = 1f;
        Debug.Log("[PlayerStats] Статистика подготовлена для НОВОЙ ИГРЫ.");
    }

    // Вызывается из MainMenuController при нажатии "Продолжить"
    public static void PrepareToContinueGame()
    {
        gameWasStartedOrContinued = true; // Помечаем, что можно "продолжить"
        PlayerController.isGamePaused = false;
        Time.timeScale = 1f;
        Debug.Log("[PlayerStats] Подготовка к ПРОДОЛЖЕНИЮ ИГРЫ.");
    }
    
    // Для MainMenuController, чтобы знать, активна ли кнопка "Продолжить"
    // "Продолжить" можно, если игра была хоть раз начата и здоровье не на нуле (если Game Over не сбрасывает persistentPlayerHealth)
    public static bool CanContinue()
    {
        // Можно продолжить, если игра была начата и сохраненное здоровье больше 0.
        // Если мы хотим, чтобы после Game Over "Продолжить" было неактивно,
        // то при Game Over нужно будет gameWasStartedOrContinued = false; или persistentPlayerHealth = 0;
        // Пока оставим так: если игра была начата, можно пробовать продолжить.
        return gameWasStartedOrContinued; 
    }

    void Awake()
    {
        Debug.Log($"[PlayerStats Awake] gameWasStartedOrContinued: {gameWasStartedOrContinued}, persistentPlayerHealth до Awake: {persistentPlayerHealth}");
        if (gameWasStartedOrContinued && persistentPlayerHealth > 0) // Если есть данные для "Продолжить" и здоровье было > 0
        {
            _currentHealth = Mathf.Clamp(persistentPlayerHealth, 0, maxHealth); 
            OnHealthChanged?.Invoke(_currentHealth, maxHealth); 
            Debug.Log($"[PlayerStats Awake] Здоровье ВОССТАНОВЛЕНО (Продолжить): {_currentHealth}/{maxHealth}");
        }
        else // Если это "Новая игра" (gameWasStartedOrContinued=false после сброса или перед первым запуском) или persistentPlayerHealth <=0
        {
            CurrentHealth = maxHealth; // Это вызовет сеттер, который установит persistentPlayerHealth и gameWasStartedOrContinued
            Debug.Log($"[PlayerStats Awake] Установлено НАЧАЛЬНОЕ/НОВОЕ здоровье: {CurrentHealth}/{maxHealth}");
        }
         // Если это первый Awake вообще, то gameWasStartedOrContinued будет false, CurrentHealth = maxHealth, 
         // и сеттер CurrentHealth установит gameWasStartedOrContinued = true и persistentPlayerHealth = maxHealth.
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        Debug.Log($"[PlayerStats] Игрок получил {amount} урона. Текущее здоровье: {CurrentHealth}");
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        Debug.Log($"[PlayerStats] Игрок вылечился на {amount}. Текущее здоровье: {CurrentHealth}");
    }

    void Die()
    {
        Debug.Log("[PlayerStats] Игрок УМЕР!");
        // persistentPlayerHealth уже будет 0 из-за сеттера CurrentHealth.
        // gameWasStartedOrContinued останется true.
        // GameSceneManager загрузит GameOver.
        // Если из GameOver выйти в меню и нажать "Продолжить", игра начнется с 0 HP, что приведет снова к GameOver.
        // Чтобы этого избежать, при загрузке GameOverScene или в MainMenuController.StartNewGame() можно сбрасывать gameWasStartedOrContinued = false.
        // Но сейчас ResetStatsForNewGame() уже делает это.
    }

    [ContextMenu("Test Take 10 Damage")]
    void TestDamage() { TakeDamage(10); }

    [ContextMenu("Test Full Heal")]
    void TestHeal() { Heal(maxHealth); }
}