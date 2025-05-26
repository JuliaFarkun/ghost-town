using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DynamicBattleSceneController : MonoBehaviour
{
    public Animator priestAnimator;
    public Transform priestTransform;
    
    [Header("Wolf Settings")]
    public List<GameObject> wolfPrefabs;
    public Transform wolfSpawnPoint;
    private Animator wolfAnimator;
    private GameObject currentWolf;
    
    [Header("Key Settings")] 
    public GameObject keyPrefab;
    public Transform keysParent;
    public List<KeyCode> possibleKeys = new List<KeyCode> 
    { 
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.W 
    };
    public int minKeys = 3;
    public int maxKeys = 8;
    
    [Header("UI Settings")]
    public TMP_Text resultText;
    public TMP_Text timerText; // Добавлено для отображения таймера
    public Vector3 priestStartPosition;
    
    private List<KeyCode> keysToPress = new List<KeyCode>();
    private List<GameObject> keyObjects = new List<GameObject>();
    private int currentKeyIndex = 0;
    private int correctCount = 0;
    private int wrongCount = 0;
    private bool inputActive = true;
    private bool isAttacking = false;
    private bool isWolfAttacking = false;
    private Coroutine currentPriestAttack;
    private Coroutine currentWolfAttack;
    private Coroutine battleTimerCoroutine; // Добавлено для управления таймером
    private bool isBattleStarted = false; // Добавлено для отслеживания состояния боя

    void Start()
    {
        StartCoroutine(BattleStartCountdown());
    }

    IEnumerator BattleStartCountdown()
    {
        // 3 секунды до начала боя
        timerText.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            timerText.text = $"Battle starts in: {i}";
            yield return new WaitForSeconds(1f);
        }
        
        timerText.text = "FIGHT!";
        yield return new WaitForSeconds(0.5f);
        timerText.gameObject.SetActive(false);
        
        // Инициализируем бой
        InitializeBattle();
        
        // Запускаем таймер боя (5 секунд)
        isBattleStarted = true;
        battleTimerCoroutine = StartCoroutine(BattleTimer());
    }

    IEnumerator BattleTimer()
    {
        float battleTime = 5f;
        while (battleTime > 0)
        {
            battleTime -= Time.deltaTime;
            timerText.gameObject.SetActive(true);
            timerText.text = $"Time left: {Mathf.CeilToInt(battleTime)}";
            yield return null;
        }
        
        timerText.text = "TIME'S UP!";
        yield return new WaitForSeconds(0.5f);
        timerText.gameObject.SetActive(false);
        
        // Завершаем бой по таймеру
        EndBattleByTimer();
    }

    void EndBattleByTimer()
    {
        inputActive = false;
        isBattleStarted = false;
        HideKeyButtons();

        // Определяем результат по текущему прогрессу
        if (currentKeyIndex >= keysToPress.Count)
        {
            // Если все клавиши нажаты, показываем результат
            ShowResult();
        }
        else
        {
            // Если время вышло, считаем это поражением
            resultText.text = "Проигрыш (время)";
            StartCoroutine(PriestHurt());
            StartCoroutine(WolfRunAwayLeft());
        }
    }

    void InitializeBattle()
    {
        SpawnRandomWolf();
        GenerateKeySequence();
        CreateKeyButtons();
        
        // Сбрасываем все состояния
        isAttacking = false;
        isWolfAttacking = false;
        if (currentPriestAttack != null) StopCoroutine(currentPriestAttack);
        if (currentWolfAttack != null) StopCoroutine(currentWolfAttack);
        
        // Устанавливаем начальные анимации
        priestAnimator.Rebind();
        priestAnimator.Update(0f);
        wolfAnimator.Rebind();
        wolfAnimator.Update(0f);
        
        priestAnimator.Play("idle", 0, 0f);
        wolfAnimator.Play("walk1", 0, 0f);
        
        resultText.text = "";
        priestTransform.localScale = new Vector3(-1, 1, 1);
        priestTransform.position = priestStartPosition;
        
        HighlightCurrentKey();
    }

    void SpawnRandomWolf()
    {
        if (wolfPrefabs.Count == 0)
        {
            Debug.LogError("No wolf prefabs assigned!");
            return;
        }
        
        if (currentWolf != null)
        {
            Destroy(currentWolf);
        }
        
        int wolfIndex = Random.Range(0, wolfPrefabs.Count);
        currentWolf = Instantiate(wolfPrefabs[wolfIndex], wolfSpawnPoint.position, Quaternion.identity);
        wolfAnimator = currentWolf.GetComponentInChildren<Animator>();
        currentWolf.transform.localScale = new Vector3(1, 1, 1);
    }

    void GenerateKeySequence()
    {
        keysToPress.Clear();
        int keyCount = Random.Range(minKeys, maxKeys + 1);
        
        for (int i = 0; i < keyCount; i++)
        {
            keysToPress.Add(possibleKeys[Random.Range(0, possibleKeys.Count)]);
        }
    }

    void CreateKeyButtons()
    {
        foreach (var key in keyObjects)
        {
            Destroy(key);
        }
        keyObjects.Clear();
        
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
        if (!inputActive || currentKeyIndex >= keysToPress.Count || !isBattleStarted)
            return;

        if (isAttacking || isWolfAttacking)
            return;

        if (Input.GetKeyDown(keysToPress[currentKeyIndex]))
        {
            correctCount++;
            currentKeyIndex++;
            HighlightCurrentKey();
            
            if (!isAttacking && currentPriestAttack == null)
            {
                currentPriestAttack = StartCoroutine(PlayPriestAttack());
            }
        }
        else if (Input.anyKeyDown)
        {
            bool wrongKey = true;
            foreach (KeyCode key in keysToPress)
            {
                if (Input.GetKeyDown(key) && key == keysToPress[currentKeyIndex])
                {
                    wrongKey = false;
                    break;
                }
            }
            if (wrongKey)
            {
                wrongCount++;
                currentKeyIndex++;
                HighlightCurrentKey();
                
                if (!isWolfAttacking && currentWolfAttack == null)
                {
                    currentWolfAttack = StartCoroutine(PlayWolfAttack());
                }
            }
        }

        if (currentKeyIndex >= keysToPress.Count)
        {
            inputActive = false;
            if (battleTimerCoroutine != null)
            {
                StopCoroutine(battleTimerCoroutine);
                timerText.gameObject.SetActive(false);
            }
            Invoke("ShowResult", 1f);
        }
    }

    void HighlightCurrentKey()
    {
        for (int i = 0; i < keyObjects.Count; i++)
        {
            var image = keyObjects[i].GetComponent<Image>();
            if (image != null)
            {
                image.color = i == currentKeyIndex ? Color.yellow : 
                             i < currentKeyIndex ? Color.gray : Color.white;
            }
        }
    }

    void ShowResult()
    {
        if (correctCount > wrongCount)
        {
            resultText.text = "Победа";
            StartCoroutine(PriestVictory());
            StartCoroutine(WolfRunAway());
        }
        else
        {
            resultText.text = "Проигрыш";
            StartCoroutine(PriestHurt());
            StartCoroutine(WolfRunAwayLeft());
        }
    }

    void HideKeyButtons()
    {
        foreach (var keyObj in keyObjects)
        {
            if (keyObj != null) // Проверка на null, чтобы избежать ошибок, если объект уже был уничтожен по какой-то причине
            {
                keyObj.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator PlayPriestAttack()
    {
        if (isAttacking) yield break;
        
        isAttacking = true;
        
        priestAnimator.Rebind();
        priestAnimator.Update(0f);
        
        priestTransform.localScale = new Vector3(-1, 1, 1);
        priestAnimator.Play("attack", 0, 0f);
        
        yield return new WaitForSeconds(priestAnimator.GetCurrentAnimatorStateInfo(0).length);
        
        priestAnimator.Play("idle", 0, 0f);
        
        isAttacking = false;
        currentPriestAttack = null;
    }

    IEnumerator PlayWolfAttack()
    {
        isWolfAttacking = true;
        
        wolfAnimator.Rebind();
        wolfAnimator.Update(0f);
        
        wolfAnimator.Play("attack", 0, 0f);
        
        AnimatorStateInfo stateInfo = wolfAnimator.GetCurrentAnimatorStateInfo(0);
        float startTime = Time.time;
        while (Time.time - startTime < stateInfo.length)
        {
            yield return null;
        }
        
        wolfAnimator.Play("walk1", 0, 0f);
        
        isWolfAttacking = false;
        currentWolfAttack = null;
    }

    IEnumerator WolfRunAwayLeft()
    {
        wolfAnimator.Play("walk", 0, 0f);
        
        float startX = currentWolf.transform.position.x;
        float endX = startX - 5f;
        float runSpeed = 3f;
        
        while (currentWolf.transform.position.x > endX)
        {
            currentWolf.transform.position += Vector3.left * runSpeed * Time.deltaTime;
            yield return null;
        }
        
        Destroy(currentWolf);
    }

    IEnumerator WolfRunAway()
    {
        wolfAnimator.Play("walk", 0, 0f);
        float startX = currentWolf.transform.position.x;
        float endX = startX + 5f;
        while (currentWolf.transform.position.x < endX)
        {
            currentWolf.transform.position += Vector3.right * Time.deltaTime * 2f;
            yield return null;
        }
        Destroy(currentWolf);
    }

    IEnumerator PriestVictory()
    {
        priestAnimator.Play("victory", 0, 0f);
        yield return null;
    }

    IEnumerator PriestHurt()
    {
        priestAnimator.Play("hurt", 0, 0f);
        yield return null;
    }

    public void RestartBattle()
    {
        currentKeyIndex = 0;
        correctCount = 0;
        wrongCount = 0;
        inputActive = true;
        
        if (battleTimerCoroutine != null)
        {
            StopCoroutine(battleTimerCoroutine);
        }
        
        timerText.gameObject.SetActive(false);
        StartCoroutine(BattleStartCountdown());
    }
}