// DynamicBattleSceneController.cs
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DynamicBattleSceneController : MonoBehaviour
{
    public Animator priestAnimator;
    public Transform priestTransform;
    
    [Header("Audio Settings")] // Добавляем секцию для аудио
    public AudioSource audioSource; // Ссылка на AudioSource для звуков атак и результата
    public AudioClip priestAttackSound; // Звук атаки жреца
    public AudioClip wolfAttackSound;   // Звук атаки волка
    public AudioClip victorySound;      // Звук победы
    public AudioClip defeatSound;       // Звук проигрыша
    public AudioSource backgroundMusicSource; // Ссылка на AudioSource фоновой музыки

    [Header("Wolf Settings")]
    public List<GameObject> wolfPrefabs; // BlackWolf (Element 0), WhiteWolf (Element 1)
    public Transform wolfSpawnPoint;
    private Animator wolfAnimator;
    private GameObject currentWolf;
    
    [Header("Key Settings")] 
    public GameObject keyPrefab;
    public Transform keysParent;
    public List<KeyCode> possibleKeys = new List<KeyCode> 
    { 
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W,
        KeyCode.F, KeyCode.J, KeyCode.K, KeyCode.L
    };
    
    [Header("UI Settings")]
    public TMP_Text resultText;
    public TMP_Text timerText;
    public Vector3 priestStartPosition;

    [Header("Scene Settings")]
    public string gameSceneName = "GameScene"; 

    // Параметры боя, зависящие от типа волка
    private string activeWolfIdentifier;
    private int keysForThisBattle;
    private float timePerKeyForThisBattle; 
    private int damageFromThisWolfOnLoss;
    
    private List<KeyCode> keysToPress = new List<KeyCode>();
    private List<GameObject> keyObjects = new List<GameObject>();
    private int currentKeyIndex = 0;
    private int correctCount = 0;
    private int wrongCount = 0;
    private bool inputActive = false; 
    private bool isAttacking = false;
    private bool isWolfAttacking = false;
    private Coroutine currentPriestAttack;
    private Coroutine currentWolfAttack;
    private Coroutine battleTimerCoroutine;
    private bool isBattleStarted = false;

    // Константы для индексов префабов согласно твоему скриншоту
    private const int BLACK_WOLF_PREFAB_INDEX = 0; // Черный волк на Element 0
    private const int WHITE_WOLF_PREFAB_INDEX = 1; // Белый волк на Element 1

    void Start()
    {
        activeWolfIdentifier = BattleDataHolder.CurrentWolfIdentifier;
        if (string.IsNullOrEmpty(activeWolfIdentifier))
        {
            Debug.LogWarning("DynamicBattleSceneController: Wolf Identifier не был передан. Используются параметры для Черного Волка (дефолт).");
            activeWolfIdentifier = "BlackWolf"; 
        }

        // Получаем компонент AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("DynamicBattleSceneController: AudioSource не найден на объекте " + gameObject.name + ". Звуки атак не будут работать.");
        }

        SetupBattleParametersForWolf(activeWolfIdentifier);
        
        if (currentWolf == null)
        {
            Debug.LogError("Волк не был заспаунен в SetupBattleParametersForWolf. Бой не может начаться корректно.");
            if(resultText != null) resultText.text = "Ошибка боя: Волк не найден!";
            StartCoroutine(ReturnToGameSceneAfterDelay(2f)); // Возвращаемся, если нет волка
            return; 
        }
        
        StartCoroutine(BattleStartCountdown());
    }

    void SetupBattleParametersForWolf(string wolfId)
    {
        int wolfPrefabIndexToSpawn = -1;

        // Используем твои данные для настроек
        if (wolfId == "WhiteWolf")
        {
            keysForThisBattle = Random.Range(6, 8 + 1);       // 6-8 клавиш
            timePerKeyForThisBattle = 1.5f;                   // 1.5 секунды на клавишу
            damageFromThisWolfOnLoss = 20;                    // 20 HP урона
            wolfPrefabIndexToSpawn = WHITE_WOLF_PREFAB_INDEX; // Используем индекс 1
        }
        else if (wolfId == "BlackWolf")
        {
            keysForThisBattle = Random.Range(3, 5 + 1);       // 3-5 клавиш
            timePerKeyForThisBattle = 1.5f;                   // 2 секунды на клавишу
            damageFromThisWolfOnLoss = 30;                    // 30 HP урона
            wolfPrefabIndexToSpawn = BLACK_WOLF_PREFAB_INDEX; // Используем индекс 0
        }
        else 
        {
            Debug.LogWarning($"Неизвестный или дефолтный wolfIdentifier: {wolfId}. Используются параметры для Черного Волка.");
            keysForThisBattle = Random.Range(3, 5 + 1);       // Параметры как у Черного Волка
            timePerKeyForThisBattle = 2.0f;
            damageFromThisWolfOnLoss = 30;
            wolfPrefabIndexToSpawn = BLACK_WOLF_PREFAB_INDEX; // Спауним Черного Волка по умолчанию (индекс 0)
        }

        if (wolfPrefabIndexToSpawn != -1)
        {
            SpawnSpecificWolf(wolfPrefabIndexToSpawn);
        }
        else
        {
            // Эта ветка не должна сработать при текущей логике, так как всегда есть дефолт
            Debug.LogError($"Не удалось определить префаб для волка. ID: {wolfId}");
            currentWolf = null; // Убедимся, что волк null, чтобы Start() прервался
        }
    }

    void SpawnSpecificWolf(int prefabIndex)
    {
        if (wolfPrefabs == null || wolfPrefabs.Count <= prefabIndex || prefabIndex < 0 || wolfPrefabs[prefabIndex] == null)
        {
            Debug.LogError($"DynamicBattleSceneController: Ошибка спауна волка! Некорректный prefabIndex ({prefabIndex}) или список wolfPrefabs не настроен. Убедитесь, что в списке 'Wolf Prefabs' есть элементы [{BLACK_WOLF_PREFAB_INDEX}] (BlackWolf) и [{WHITE_WOLF_PREFAB_INDEX}] (WhiteWolf) и они не пустые.");
            currentWolf = null; 
            return;
        }

        if (currentWolf != null) Destroy(currentWolf);

        currentWolf = Instantiate(wolfPrefabs[prefabIndex], wolfSpawnPoint.position, wolfSpawnPoint.rotation);
        wolfAnimator = currentWolf.GetComponentInChildren<Animator>();
        if (wolfAnimator == null) Debug.LogError($"Animator не найден на префабе волка {wolfPrefabs[prefabIndex].name} или его дочерних объектах!");
    }
    
    IEnumerator BattleStartCountdown()
    {
        float totalBattleTime; 

        if(timerText == null) 
        {
            Debug.LogError("TimerText не назначен в Inspector! Пропускаем обратный отсчет.");
            InitializeBattle(); 
            isBattleStarted = true; inputActive = true; 
            
            totalBattleTime = keysForThisBattle * timePerKeyForThisBattle; 
            if (totalBattleTime > 0) battleTimerCoroutine = StartCoroutine(BattleTimer(totalBattleTime));
            else Debug.LogWarning("Total battle time is zero or negative, timer not started (timerText was null branch).");
        } 
        else 
        {
            timerText.gameObject.SetActive(true);
            for (int i = 3; i > 0; i--) {
                timerText.text = $"Battle starts in: {i}";
                yield return new WaitForSecondsRealtime(1f);
            }
            timerText.text = "FIGHT!";
            yield return new WaitForSecondsRealtime(0.5f);
            timerText.gameObject.SetActive(false);
            
            InitializeBattle();
            
            isBattleStarted = true; inputActive = true;

            totalBattleTime = keysForThisBattle * timePerKeyForThisBattle; 
            if (totalBattleTime > 0) battleTimerCoroutine = StartCoroutine(BattleTimer(totalBattleTime));
            else Debug.LogWarning("Total battle time is zero or negative, timer not started (normal countdown branch).");
        }
    }

    IEnumerator BattleTimer(float battleDuration)
    {
        if(timerText == null || battleDuration <=0) yield break;

        float battleTime = battleDuration;
        timerText.gameObject.SetActive(true);
        while (battleTime > 0)
        {
            if (!isBattleStarted || !inputActive) 
            {
                timerText.gameObject.SetActive(false);
                yield break;
            }
            battleTime -= Time.deltaTime; // Предполагаем Time.timeScale = 1 в этой сцене
            timerText.text = $"Time left: {Mathf.CeilToInt(battleTime)}";
            yield return null;
        }
        
        if (isBattleStarted && inputActive) 
        {
            timerText.text = "TIME'S UP!";
            yield return new WaitForSecondsRealtime(0.5f); 
            timerText.gameObject.SetActive(false);
            EndBattleByTimer();
        }
    }
    
    void InitializeBattle()
    {
        GenerateKeySequence(); 
        CreateKeyButtons();
        
        currentKeyIndex = 0;
        correctCount = 0;
        wrongCount = 0;
        isAttacking = false;
        isWolfAttacking = false;

        if (currentPriestAttack != null) StopCoroutine(currentPriestAttack);
        if (currentWolfAttack != null) StopCoroutine(currentWolfAttack);
        
        if (priestAnimator != null) 
        {
            priestAnimator.Rebind(); 
            priestAnimator.Update(0f); 
            priestAnimator.Play("idle 1", 0, 0f); 
        }
        if (wolfAnimator != null) 
        {
            wolfAnimator.Rebind();
            wolfAnimator.Update(0f);
            wolfAnimator.Play("walk 1", 0, 0f); // Волк: "walk 1" (боевая стойка/idle)
        }
        
        if (resultText != null) resultText.text = "";
        if (priestTransform != null)
        {
            priestTransform.position = priestStartPosition;
            SpriteRenderer priestSprite = priestTransform.GetComponent<SpriteRenderer>(); 
            if (priestSprite != null) priestSprite.flipX = false; // Предполагаем, жрец смотрит вправо на волка
            else priestTransform.localScale = new Vector3(1, 1, 1); 
        }
        
        HighlightCurrentKey();
    }

    void GenerateKeySequence()
    {
        keysToPress.Clear();
        int keyCount = keysForThisBattle; 
        
        if (keyCount <= 0 || possibleKeys.Count == 0) {
            Debug.LogError("Cannot generate key sequence: keyCount is zero or no possible keys.");
            if(keyCount <= 0) Debug.LogWarning($"keysForThisBattle: {keysForThisBattle} (is zero or negative)");
            return;
        }

        for (int i = 0; i < keyCount; i++)
        {
            keysToPress.Add(possibleKeys[Random.Range(0, possibleKeys.Count)]);
        }
    }

    void CreateKeyButtons()
    {
        foreach (var keyObj in keyObjects) { if(keyObj != null) Destroy(keyObj); }
        keyObjects.Clear();
        
        if (keysToPress.Count == 0 || keyPrefab == null || keysParent == null) 
        {
            if(keysToPress.Count == 0) Debug.LogWarning("CreateKeyButtons: keysToPress is empty.");
            if(keyPrefab == null) Debug.LogError("CreateKeyButtons: keyPrefab is not assigned.");
            if(keysParent == null) Debug.LogError("CreateKeyButtons: keysParent is not assigned.");
            return;
        }

        float spacing = 100f; 
        float startX = -((keysToPress.Count - 1) * spacing) / 2f;
        
        for (int i = 0; i < keysToPress.Count; i++)
        {
            GameObject keyObj = Instantiate(keyPrefab, keysParent);
            keyObj.transform.localPosition = new Vector3(startX + i * spacing, 0, 0);
            
            TMP_Text keyText = keyObj.GetComponentInChildren<TMP_Text>();
            if (keyText != null)
            {
                keyText.text = keysToPress[i].ToString();
            }
            keyObjects.Add(keyObj);
        }
    }

    void Update() 
    {
        if (!inputActive || !isBattleStarted || keysToPress.Count == 0 || currentKeyIndex >= keysToPress.Count)
            return;

        if (isAttacking || isWolfAttacking) 
            return;

        if (Input.anyKeyDown) 
        {
            bool actionTaken = false;
            bool correctKeyPressedThisFrame = false; 
            
            if (Input.GetKeyDown(keysToPress[currentKeyIndex]))
            {
                correctCount++;
                if (!isAttacking && currentPriestAttack == null && priestAnimator != null)
                {
                    currentPriestAttack = StartCoroutine(PlayPriestAttack());
                }
                actionTaken = true;
                correctKeyPressedThisFrame = true;
            }
            
            if (!correctKeyPressedThisFrame) 
            {
                foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (kc >= KeyCode.Mouse0 && kc <= KeyCode.Mouse6) continue; 
                    if (Input.GetKeyDown(kc)) 
                    {
                        if (kc != keysToPress[currentKeyIndex]) 
                        { 
                            wrongCount++;
                            if (!isWolfAttacking && currentWolfAttack == null && wolfAnimator != null)
                            {
                                currentWolfAttack = StartCoroutine(PlayWolfAttack());
                            }
                            actionTaken = true;
                        }
                        break; 
                    }
                }
            }

            if (actionTaken) 
            {
                currentKeyIndex++;
                HighlightCurrentKey();

                if (currentKeyIndex >= keysToPress.Count) 
                {
                    inputActive = false; 
                    ShowResult(); 
                }
            }
        }
    }

    void HighlightCurrentKey() 
    {
        for (int i = 0; i < keyObjects.Count; i++)
        {
            if (keyObjects[i] == null) continue; 
            var image = keyObjects[i].GetComponent<Image>();
            if (image != null)
            {
                image.color = i == currentKeyIndex ? Color.yellow : 
                             (i < currentKeyIndex ? Color.gray : Color.white);
            }
        }
    }

    void ShowResult() 
    {
        inputActive = false; 
        isBattleStarted = false; 

        if (battleTimerCoroutine != null) StopCoroutine(battleTimerCoroutine);
        if (timerText != null && timerText.gameObject.activeSelf) timerText.gameObject.SetActive(false);
        
        HideKeyButtons();

        // Проверяем, есть ли вообще клавиши для нажатия. Если нет (например, из-за ошибки в GenerateKeySequence),
        // то результат может быть не определен корректно.
        bool playerWon = false;
        if (keysToPress.Count > 0) {
            playerWon = correctCount >= Mathf.CeilToInt((float)keysToPress.Count / 2.0f);
        } else {
            Debug.LogWarning("ShowResult: Нет клавиш для проверки результата (keysToPress.Count is 0). Считаем проигрышем.");
            playerWon = false; // Если нет клавиш, считаем проигрышем
        }

        string outcomeMessage = playerWon ? "Победа!" : "Проигрыш!";

        if (resultText != null) 
        {
            resultText.text = outcomeMessage;
            resultText.color = playerWon ? Color.green : Color.red;
        }

        // Останавливаем фоновую музыку, если она проигрывается
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }

        // Воспроизводим звук победы или поражения
        if (audioSource != null)
        {
            // Удаляем предыдущую логику остановки, так как фоновая музыка на другом источнике
            // if (audioSource.isPlaying)
            // {
            //     audioSource.Stop();
            // }

            if (playerWon && victorySound != null)
            {
                audioSource.PlayOneShot(victorySound);
            }
            else if (!playerWon && defeatSound != null)
            {
                audioSource.PlayOneShot(defeatSound);
            }
        }

        // Определяем исход и урон
        string finalOutcome;
        int finalDamage;

        if (playerWon)
        {
            finalOutcome = "Victory";
            finalDamage = 0;
            if (priestAnimator != null) StartCoroutine(PriestVictory());
            if (currentWolf != null && wolfAnimator != null) StartCoroutine(WolfRunAway()); 
        }
        else
        {
            finalOutcome = "Defeat"; 
            if (timerText != null && timerText.text == "TIME'S UP!")
            {
                finalOutcome = "DefeatTimeOut"; 
            }
            finalDamage = damageFromThisWolfOnLoss;
            if (priestAnimator != null) StartCoroutine(PriestHurt());
            if (currentWolf != null && wolfAnimator != null) StartCoroutine(WolfRunAway()); 
        }
        
        BattleDataHolder.SetBattleOutcome(finalOutcome, finalDamage);
        
        StartCoroutine(ReturnToGameSceneAfterDelay(3.0f));
    }
    
    void EndBattleByTimer() 
    {
        if (!isBattleStarted && !inputActive) return; 
        Debug.Log("Время для QTE вышло!");
        ShowResult(); 
    }

    void HideKeyButtons() 
    {
        foreach (var keyObj in keyObjects) { if (keyObj != null) keyObj.gameObject.SetActive(false); }
    }

    IEnumerator PlayPriestAttack() 
    {
        if (isAttacking || priestAnimator == null) yield break;
        isAttacking = true; 
        priestAnimator.Play("attack", 0, 0f); 
        
        // Воспроизводим звук атаки жреца
        if (audioSource != null && priestAttackSound != null)
        {
            audioSource.PlayOneShot(priestAttackSound);
        }

        yield return null; 
        // Ждем фактической длительности анимации
        float animationLength = 0;
        AnimatorStateInfo currentAnimState = priestAnimator.GetCurrentAnimatorStateInfo(0);
        if (currentAnimState.IsName("attack")) animationLength = currentAnimState.length;
        else Debug.LogWarning("PlayPriestAttack: Анимация жреца не 'attack' сразу после Play.");
        yield return new WaitForSeconds(animationLength > 0 ? animationLength : 0.5f); // Запасной вариант, если length = 0

        if (priestAnimator != null) priestAnimator.Play("idle 1", 0, 0f);
        isAttacking = false; 
        currentPriestAttack = null;

        // Останавливаем звук атаки жреца, если он еще играет (PlayOneShot сам остановится, но на всякий случай)
        // if (audioSource != null && audioSource.isPlaying && audioSource.clip == priestAttackSound)
        // {
        //     audioSource.Stop();
        // }
    }

    IEnumerator PlayWolfAttack() 
    {
        if(wolfAnimator == null || isWolfAttacking) yield break;
        isWolfAttacking = true; 
        wolfAnimator.Play("attack", 0, 0f);

        // Воспроизводим звук атаки волка
        if (audioSource != null && wolfAttackSound != null)
        {
            audioSource.PlayOneShot(wolfAttackSound);
        }

        yield return null; 
        AnimatorStateInfo stateInfo = wolfAnimator.GetCurrentAnimatorStateInfo(0);
        float timer = 0f; 
        float maxWaitTime = 5f; // Максимальное время ожидания
        float animationLength = stateInfo.IsName("attack") ? stateInfo.length : 0.5f; // Получаем длину, если это атака

        // Ждем либо пока normalizedTime < 1.0, либо пока не пройдет animationLength
        while (stateInfo.IsName("attack") && stateInfo.normalizedTime < 0.95f && timer < animationLength && timer < maxWaitTime) 
        {
            yield return null; 
            stateInfo = wolfAnimator.GetCurrentAnimatorStateInfo(0); 
            timer += Time.deltaTime;
        }
        if(timer >= maxWaitTime && stateInfo.IsName("attack")) Debug.LogWarning("PlayWolfAttack: Превышено время ожидания анимации атаки волка.");
        else if (timer >= animationLength && stateInfo.IsName("attack")) Debug.Log("PlayWolfAttack: Анимация атаки волка завершилась по длительности.");
        
        if (wolfAnimator != null) wolfAnimator.Play("walk 1", 0, 0f); // Волк: "walk 1" (боевая стойка)
        isWolfAttacking = false; 
        currentWolfAttack = null;

        // Останавливаем звук атаки волка, если он еще играет (PlayOneShot сам остановится, но на всякий случай)
        // if (audioSource != null && audioSource.isPlaying && audioSource.clip == wolfAttackSound)
        // {
        //     audioSource.Stop();
        // }
    }

    IEnumerator WolfRunAway() 
    {
        if(wolfAnimator == null || currentWolf == null) yield break;

        // Вызываем анимацию "walk 1", которая, судя по скриншотам, соответствует бегу вправо
        wolfAnimator.Play("walk 1", 0, 0f); 
        yield return null; // Даем кадр на применение анимации

        // После вызова правильной анимации, установка flipX/scale из скрипта может быть уже не нужна,
        // так как анимация сама должна установить правильный Scale.x.
        // Оставим на всякий случай, если есть другие состояния или для подстраховки.
        SpriteRenderer wolfSprite = currentWolf.GetComponentInChildren<SpriteRenderer>();
        if (wolfSprite != null)
        {
            // Если анимация "walk 1" всегда ставит Scale.x > 0, то flipX должен быть false для взгляда вправо.
            // Но так как Scale.x уже положительный из анимации, flipX может не иметь значения или должен быть false.
            wolfSprite.flipX = false; 
        }
        // Альтернативно, если Scale.x из анимации "walk 1" это то, что нам нужно, 
        // то дополнительная коррекция scale здесь не нужна.
        // else if (currentWolf.transform.localScale.x < 0) 
        // {
        //    currentWolf.transform.localScale = new Vector3(Mathf.Abs(currentWolf.transform.localScale.x), currentWolf.transform.localScale.y, currentWolf.transform.localScale.z);
        // }

        float endX = currentWolf.transform.position.x + 10f;
        float runSpeed = 4f;
        while (currentWolf != null && currentWolf.transform.position.x < endX)
        {
            // Если анимация "walk 1" стабильно держит волка повернутым вправо,
            // повторная установка flipX/scale здесь не нужна.
            currentWolf.transform.position += Vector3.right * runSpeed * Time.deltaTime;
            yield return null;
        }
        if(currentWolf != null) Destroy(currentWolf);
    }

    IEnumerator PriestVictory() { if (priestAnimator != null) priestAnimator.Play("victory", 0, 0f); yield return null; }
    IEnumerator PriestHurt() { if (priestAnimator != null) priestAnimator.Play("hurt", 0, 0f); yield return null; }
    IEnumerator ReturnToGameSceneAfterDelay(float delay) { yield return new WaitForSecondsRealtime(delay); SceneManager.LoadScene(gameSceneName); }
}