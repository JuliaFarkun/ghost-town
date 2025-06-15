// FinalBattleController.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class FinalBattleController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;    // Показывает "Правильно: X / Всего Ожидаемых: Y"
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonsParent; // Родительский объект для кнопок на Canvas

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 30f; 
    [SerializeField] private float spawnInterval = 2f;  
    // [SerializeField] private int uiFontSize = 24; // Размер шрифта настраивается в редакторе
    [SerializeField] private float uiSafeZonePadding = 50f; // Отступы для позиционирования кнопок
    [SerializeField] private float buttonLifetime = 3.0f; 

    [Header("Scene Transitions")]
    [SerializeField] private string gameOverSceneName = "GameOverScene"; 
    [SerializeField] private float delayBeforeEndSceneTransition = 5f; 

    private float currentTime;
    private bool isGameRunning;
    private List<ActiveButtonInfo> activeButtons = new List<ActiveButtonInfo>();
    private Coroutine spawnCoroutine;
    private RectTransform canvasRect; 
    
    private int totalButtonsExpected = 0; 
    private int correctPressedButtons = 0;

    private struct ActiveButtonInfo
    {
        public GameObject buttonObject;
        public KeyCode keyCode; 
        public bool isProcessed; 
    }

    void Start()
    {
        Time.timeScale = 1f;
        // PlayerController.isGamePaused = false; // Если нужно управлять глобальным флагом паузы

        Canvas canvas = GameObject.FindAnyObjectByType<Canvas>();
        if (canvas != null) canvasRect = canvas.GetComponent<RectTransform>();
        else {
            Debug.LogError("FinalBattleController: Canvas не найден на сцене!");
            enabled = false; return;
        }

        if (buttonPrefab == null) Debug.LogError("FinalBattleController: Button prefab не назначен в Inspector!");
        if (buttonsParent == null) {
            Debug.LogWarning("FinalBattleController: Buttons Parent не назначен. Кнопки будут создаваться на Canvas.");
            buttonsParent = canvasRect.transform;
        }
        
        if (countdownText == null) Debug.LogError("FinalBattleController: CountdownText не назначен!");
        if (timerText == null) Debug.LogError("FinalBattleController: TimerText не назначен!");
        if (scoreText == null) Debug.LogError("FinalBattleController: ScoreText не назначен!");

        SetupTextProperties();
        InitializeGame();
    }

    void SetupTextProperties() 
    {
        // Размеры, положение и шрифт текстовых полей настраиваются в редакторе Unity.
        // Скрипт только управляет видимостью и содержимым текста.
        if (countdownText != null) {
            countdownText.text = ""; 
            countdownText.gameObject.SetActive(false); 
        }
        if (timerText != null) {
            timerText.gameObject.SetActive(false);
        }
        if (scoreText != null) {
            scoreText.gameObject.SetActive(false); 
        }
    }
    
    void InitializeGame()
    {
        currentTime = gameDuration;
        isGameRunning = false;
        correctPressedButtons = 0;

        if (spawnInterval > 0.001f) {
            totalButtonsExpected = Mathf.FloorToInt(gameDuration / spawnInterval);
        } else {
            totalButtonsExpected = (gameDuration > 0) ? 1 : 0;
            Debug.LogWarning("FinalBattleController: SpawnInterval очень мал или равен нулю.");
        }
        Debug.Log($"[FinalBattle] Ожидаемое количество кнопок для этого боя: {totalButtonsExpected}");

        foreach (var buttonInfo in activeButtons) { if (buttonInfo.buttonObject != null) Destroy(buttonInfo.buttonObject); }
        activeButtons.Clear();

        if (countdownText != null) countdownText.gameObject.SetActive(true); 
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (scoreText != null) scoreText.gameObject.SetActive(true);

        UpdateScoreDisplay(); 
        UpdateTimerDisplay(); 

        StartCoroutine(GameRoutine());
    }

    IEnumerator GameRoutine()
    {
        yield return StartCoroutine(CountdownPhase());
        if (!isActiveAndEnabled) yield break; 

        yield return StartCoroutine(MainGamePhase());
        if (!isActiveAndEnabled) yield break;

        yield return StartCoroutine(EndGamePhase());
    }

    IEnumerator CountdownPhase() 
    {
        if (countdownText == null) {
             Debug.LogWarning("CountdownText не назначен, пропускаем фазу отсчета.");
             yield break;
        }
        countdownText.gameObject.SetActive(true);
        countdownText.color = Color.white; 
        for (int i = 3; i > 0; i--) {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f); // Используем обычный WaitForSeconds, т.к. Time.timeScale = 1
            if (!isActiveAndEnabled) yield break;
        }
        countdownText.text = "START!";
        yield return new WaitForSeconds(1f);
        if (!isActiveAndEnabled) yield break;
        countdownText.gameObject.SetActive(false);
    }

    IEnumerator MainGamePhase() 
    {
        isGameRunning = true;
        if (timerText != null) timerText.color = Color.white;
        if (isActiveAndEnabled && totalButtonsExpected > 0) { 
            spawnCoroutine = StartCoroutine(SpawnButtonsRoutine());
        } else if (totalButtonsExpected <= 0) {
            Debug.LogWarning("MainGamePhase: Не ожидается спауна кнопок (totalButtonsExpected <= 0).");
        }
        
        while (currentTime > 0 && isGameRunning) {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
            yield return null;
        }
        isGameRunning = false; 
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
    }

    IEnumerator SpawnButtonsRoutine() 
    {
        int spawnedThisRound = 0;
        while (isGameRunning && spawnedThisRound < totalButtonsExpected) { 
            SpawnButton();
            spawnedThisRound++;
            yield return new WaitForSeconds(spawnInterval);
        }
        Debug.Log($"[FinalBattle] Завершение спауна. Заспаунено: {spawnedThisRound} из {totalButtonsExpected} ожидаемых.");
    }

    void SpawnButton() 
    {
        if (buttonPrefab == null || buttonsParent == null || canvasRect == null) return;
        GameObject newButtonGO = Instantiate(buttonPrefab, buttonsParent);
        UpdateScoreDisplay(); 
        RectTransform rt = newButtonGO.GetComponent<RectTransform>();
        rt.anchoredPosition = GetRandomPositionInRect(canvasRect, rt.sizeDelta); 
        Image img = newButtonGO.GetComponent<Image>();
        if (img != null) img.color = Color.white;
        TMP_Text buttonText = newButtonGO.GetComponentInChildren<TMP_Text>();
        KeyCode randomKeyCode = GetRandomKeyCode(); 
        if (buttonText != null) buttonText.text = KeyCodeToDisplayString(randomKeyCode);
        activeButtons.Add(new ActiveButtonInfo { buttonObject = newButtonGO, keyCode = randomKeyCode, isProcessed = false });
        StartCoroutine(DestroyButtonAfterDelay(newButtonGO, buttonLifetime)); 
    }
    
    IEnumerator DestroyButtonAfterDelay(GameObject buttonToDestroy, float delay) 
    {
        yield return new WaitForSeconds(delay);
        if (buttonToDestroy != null) {
            int buttonIndex = activeButtons.FindIndex(b => b.buttonObject == buttonToDestroy && !b.isProcessed);
            if (buttonIndex != -1) {
                ActiveButtonInfo info = activeButtons[buttonIndex];
                info.isProcessed = true; 
                activeButtons[buttonIndex] = info;
            }
            Destroy(buttonToDestroy);
        }
    }

    Vector2 GetRandomPositionInRect(RectTransform parentRectForBounds, Vector2 itemSize)
    {
        float xMin = parentRectForBounds.rect.xMin + itemSize.x / 2f + uiSafeZonePadding;
        float xMax = parentRectForBounds.rect.xMax - itemSize.x / 2f - uiSafeZonePadding;
        float yMin = parentRectForBounds.rect.yMin + itemSize.y / 2f + uiSafeZonePadding;
        float yMax = parentRectForBounds.rect.yMax - itemSize.y / 2f - uiSafeZonePadding;

        if (xMin >= xMax || yMin >= yMax) {
            Debug.LogWarning("Родительский RectTransform (или Canvas) слишком мал для безопасного спауна кнопок с учетом отступов. Кнопка будет в центре.");
            return parentRectForBounds.rect.center; 
        }
        return new Vector2(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
    }

    KeyCode GetRandomKeyCode() {
        List<KeyCode> keys = new List<KeyCode> 
        {
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
            KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
            KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,
            // Добавлены новые KeyCodes по запросу пользователя
            KeyCode.Space, KeyCode.Plus, KeyCode.Minus, KeyCode.Equals, KeyCode.Slash,
            KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
            KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
        };
        return keys[Random.Range(0, keys.Count)];
    }

    string KeyCodeToDisplayString(KeyCode kc) {
        if (kc == KeyCode.Space) return "SPACE";
        if (kc == KeyCode.Plus) return "+";
        if (kc == KeyCode.Minus) return "-";
        if (kc == KeyCode.Equals) return "=";
        if (kc == KeyCode.Slash) return "/";
        // Для цифр можно оставить существующий метод ToString(), так как Alpha0-9 дадут "Alpha0" и т.д.
        // Или добавить явное преобразование, если нужны только цифры "0"-"9"
        if (kc >= KeyCode.Alpha0 && kc <= KeyCode.Alpha9) {
            return (kc - KeyCode.Alpha0).ToString();
        }
        return kc.ToString().ToUpper();
    }

    void UpdateTimerDisplay() {
        if (timerText == null) return;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(currentTime));
        timerText.text = $"Time: {seconds}";
        if (seconds <= 5 && seconds > 0) timerText.color = Color.yellow;
        else if (seconds == 0) timerText.color = Color.red;
        else timerText.color = Color.white;
    }

    void UpdateScoreDisplay() {
        if (scoreText == null) return;
        scoreText.text = $"Правильно: {correctPressedButtons} / Всего: {totalButtonsExpected}";
    }

    IEnumerator EndGamePhase()
    {
        isGameRunning = false; 
        
        foreach (var buttonInfo in activeButtons) {
            if (buttonInfo.buttonObject != null) Destroy(buttonInfo.buttonObject);
        }
        activeButtons.Clear();

        if(countdownText == null) {
            Debug.LogError("CountdownText не назначен, не могу показать результат! Переход на Game Over...");
            yield return new WaitForSecondsRealtime(delayBeforeEndSceneTransition); // Используем Realtime, т.к. Time.timeScale может быть 0
            GoToGameOverScene();
            yield break;
        }
        countdownText.gameObject.SetActive(true);
        
        bool playerWon = false;
        if (totalButtonsExpected > 0) { 
            playerWon = correctPressedButtons > totalButtonsExpected / 2.0f;
            Debug.Log($"[FinalBattle] Результат: Правильно {correctPressedButtons} из {totalButtonsExpected} ожидаемых. Победа: {playerWon}");
        } else {
            Debug.LogWarning("[FinalBattle] totalButtonsExpected равен 0. Считаем проигрышем по умолчанию.");
            playerWon = false; 
        }

        if (playerWon) {
            countdownText.text = $"ПОБЕДА!\nПравильно: {correctPressedButtons} из {totalButtonsExpected}";
            countdownText.color = Color.green;
        } else {
            countdownText.text = $"ПРОИГРЫШ!\nПравильно: {correctPressedButtons} из {totalButtonsExpected}";
            countdownText.color = Color.red;
        }

        // Используем WaitForSecondsRealtime, так как Time.timeScale может быть изменен перед переходом
        yield return new WaitForSecondsRealtime(delayBeforeEndSceneTransition); 
        GoToGameOverScene();
    }

    void GoToGameOverScene() 
    {
        if (!string.IsNullOrEmpty(gameOverSceneName)) {
            Debug.Log($"[FinalBattle] Завершение финального боя. Загрузка сцены: {gameOverSceneName}. Удаление файла сохранения.");
            
            SaveLoadManager.DeleteSaveFile(); // УДАЛЯЕМ файл сохранения
            BattleDataHolder.ResetSessionData();  // Сбрасываем временные данные боя

            // PlayerStats.PrepareForNewGameSession(); // Этот вызов больше не нужен, так как PlayerStats не хранит статику сессии

            SceneManager.LoadScene(gameOverSceneName);
        } else {
            Debug.LogError("Имя сцены Game Over (gameOverSceneName) не указано в Inspector для FinalBattleController!");
        }
    }

    void Update() 
    {
        if (!isGameRunning || activeButtons.Count == 0) return;

        for (int i = activeButtons.Count - 1; i >= 0; i--) {
            ActiveButtonInfo buttonInfo = activeButtons[i];
            if (buttonInfo.buttonObject == null || buttonInfo.isProcessed) {
                if(buttonInfo.buttonObject == null && i < activeButtons.Count) activeButtons.RemoveAt(i); 
                continue;
            }

            if (Input.GetKeyDown(buttonInfo.keyCode)) {
                correctPressedButtons++;
                UpdateScoreDisplay();
                Image img = buttonInfo.buttonObject.GetComponent<Image>();
                if(img != null) img.color = Color.green;
                
                buttonInfo.isProcessed = true;
                activeButtons[i] = buttonInfo; 
                Destroy(buttonInfo.buttonObject, 0.2f); 
            }
        }
        activeButtons.RemoveAll(b => b.buttonObject == null);
    }
}