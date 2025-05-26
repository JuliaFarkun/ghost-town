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
    public Vector3 priestStartPosition;
    
    [Header("Timer Settings")]
    public TMP_Text timerText;
    public Image timerBackground;
    public Color timerNormalColor = Color.white;
    public Color timerWarningColor = Color.red;
    public float warningThreshold = 3f;
    
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
    private Coroutine battleTimerCoroutine;
    private bool isBattleStarted = false;

    void Start()
    {
        SetupTimerUI();
        StartCoroutine(BattleStartCountdown());
    }

    void SetupTimerUI()
    {
        // Настраиваем RectTransform для таймера
        RectTransform timerRect = timerText.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0, 0);
        timerRect.anchorMax = new Vector2(0, 0);
        timerRect.pivot = new Vector2(0, 0);
        timerRect.anchoredPosition = new Vector2(20, 20);
        
        // Настраиваем фон таймера
        if (timerBackground != null)
        {
            RectTransform bgRect = timerBackground.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(0, 0);
            bgRect.pivot = new Vector2(0, 0);
            bgRect.anchoredPosition = new Vector2(10, 10);
            bgRect.sizeDelta = new Vector2(120, 40);
        }
        
        timerText.fontSize = 24;
        timerText.color = timerNormalColor;
        timerText.alignment = TextAlignmentOptions.Center;
    }

    IEnumerator BattleStartCountdown()
    {
        timerText.text = "3";
        yield return new WaitForSeconds(1f);
        timerText.text = "2";
        yield return new WaitForSeconds(1f);
        timerText.text = "1";
        yield return new WaitForSeconds(1f);
        timerText.text = "FIGHT!";
        yield return new WaitForSeconds(0.5f);
        
        InitializeBattle();
        battleTimerCoroutine = StartCoroutine(BattleTimer());
    }

    IEnumerator BattleTimer()
    {
        float battleTime = 5f;
        isBattleStarted = true;
        
        while (battleTime > 0)
        {
            battleTime -= Time.deltaTime;
            
            // Меняем цвет при приближении к концу времени
            if (battleTime <= warningThreshold)
            {
                timerText.color = timerWarningColor;
            }
            
            timerText.text = Mathf.CeilToInt(battleTime).ToString();
            yield return null;
        }
        
        timerText.text = "TIME UP!";
        yield return new WaitForSeconds(0.5f);
        
        EndBattleByTimer();
    }

    void EndBattleByTimer()
    {
        inputActive = false;
        isBattleStarted = false;
        
        if (currentKeyIndex >= keysToPress.Count)
        {
            ShowResult();
        }
        else
        {
            resultText.text = "TIME OVER!";
            StartCoroutine(PriestHurt());
            StartCoroutine(WolfRunAwayLeft());
        }
    }

    void InitializeBattle()
    {
        SpawnRandomWolf();
        GenerateKeySequence();
        CreateKeyButtons();
        
        isAttacking = false;
        isWolfAttacking = false;
        if (currentPriestAttack != null) StopCoroutine(currentPriestAttack);
        if (currentWolfAttack != null) StopCoroutine(currentWolfAttack);
        
        priestAnimator.Rebind();
        priestAnimator.Update(0f);
        wolfAnimator.Rebind();
        wolfAnimator.Update(0f);
        
        priestAnimator.Play("idle", 0, 0f);
        wolfAnimator.Play("walk", 0, 0f);
        
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
        wolfAnimator = currentWolf.GetComponent<Animator>();
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
        
        currentWolf.transform.localScale = new Vector3(1, 1, 1);
        wolfAnimator.Play("attack", 0, 0f);
        
        AnimatorStateInfo stateInfo = wolfAnimator.GetCurrentAnimatorStateInfo(0);
        float startTime = Time.time;
        while (Time.time - startTime < stateInfo.length)
        {
            yield return null;
        }
        
        wolfAnimator.Play("walk", 0, 0f);
        
        isWolfAttacking = false;
        currentWolfAttack = null;
    }

    IEnumerator WolfRunAwayLeft()
    {
        currentWolf.transform.localScale = new Vector3(-1, 1, 1);
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
        currentWolf.transform.localScale = new Vector3(1, 1, 1);
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
        
        timerText.color = timerNormalColor;
        StartCoroutine(BattleStartCountdown());
    }
}