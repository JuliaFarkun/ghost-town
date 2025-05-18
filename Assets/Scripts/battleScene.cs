using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class DynamicBattleSceneController : MonoBehaviour
{
    public Animator priestAnimator;
    public Transform priestTransform;
    
    [Header("Wolf Settings")]
    public List<GameObject> wolfPrefabs; // Префабы разных волков
    public Transform wolfSpawnPoint;
    private Animator wolfAnimator;
    private GameObject currentWolf;
    
    [Header("Key Settings")]
    public GameObject keyPrefab; // Префаб кнопки
    public Transform keysParent; // Родитель для кнопок
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
    
    private List<KeyCode> keysToPress = new List<KeyCode>();
    private List<GameObject> keyObjects = new List<GameObject>();
    private int currentKeyIndex = 0;
    private int correctCount = 0;
    private int wrongCount = 0;
    private bool inputActive = true;

    void Start()
    {
        InitializeBattle();
    }

    void InitializeBattle()
    {
        // Создаем случайного волка
        SpawnRandomWolf();
        
        // Генерируем последовательность кнопок
        GenerateKeySequence();
        
        // Создаем визуальные кнопки
        CreateKeyButtons();
        
        // Настраиваем начальные параметры
        priestAnimator.Play("idle");
        wolfAnimator.Play("walk");
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
        
        // Удаляем старого волка, если есть
        if (currentWolf != null)
        {
            Destroy(currentWolf);
        }
        
        // Выбираем случайного волка
        int wolfIndex = Random.Range(0, wolfPrefabs.Count);
        currentWolf = Instantiate(wolfPrefabs[wolfIndex], wolfSpawnPoint.position, Quaternion.identity);
        wolfAnimator = currentWolf.GetComponent<Animator>();
        
        // Убедимся, что волк смотрит в правильную сторону
        currentWolf.transform.localScale = new Vector3(1, 1, 1);
    }

    void GenerateKeySequence()
    {
        keysToPress.Clear();
        int keyCount = Random.Range(minKeys, maxKeys + 1);
        
        for (int i = 0; i < keyCount; i++)
        {
            int randomKeyIndex = Random.Range(0, possibleKeys.Count);
            keysToPress.Add(possibleKeys[randomKeyIndex]);
        }
    }

    void CreateKeyButtons()
    {
        // Очищаем старые кнопки
        foreach (var key in keyObjects)
        {
            Destroy(key);
        }
        keyObjects.Clear();
        
        // Создаем новые кнопки
        float spacing = 100f;
        float startX = -((keysToPress.Count - 1) * spacing) / 2f;
        
        for (int i = 0; i < keysToPress.Count; i++)
        {
            GameObject keyObj = Instantiate(keyPrefab, keysParent);
            keyObj.transform.localPosition = new Vector3(startX + i * spacing, 0, 0);
            
            // Настраиваем текст кнопки
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
        if (!inputActive || currentKeyIndex >= keysToPress.Count)
            return;

        if (Input.GetKeyDown(keysToPress[currentKeyIndex]))
        {
            correctCount++;
            StartCoroutine(PlayPriestAttack());
            currentKeyIndex++;
            HighlightCurrentKey();
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
                StartCoroutine(PlayWolfAttack());
                currentKeyIndex++;
                HighlightCurrentKey();
            }
        }

        if (currentKeyIndex >= keysToPress.Count)
        {
            inputActive = false;
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
                if (i == currentKeyIndex)
                    image.color = Color.yellow;
                else if (i < currentKeyIndex)
                    image.color = Color.gray;
                else
                    image.color = Color.white;
            }
        }
    }

    System.Collections.IEnumerator PlayPriestAttack()
    {
        priestTransform.localScale = new Vector3(-1, 1, 1);
        priestAnimator.Play("attack");
        yield return new WaitForSeconds(0.7f);
        priestAnimator.Play("idle");
    }

    System.Collections.IEnumerator PlayWolfAttack()
    {
        currentWolf.transform.localScale = new Vector3(1, 1, 1);
        wolfAnimator.Play("attack");
        yield return null;
        float length = wolfAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length);
        wolfAnimator.Play("walk");
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
            StartCoroutine(PlayWolfAttack());
        }
    }

    System.Collections.IEnumerator PriestVictory()
    {
        priestAnimator.Play("victory");
        yield return null;
    }

    System.Collections.IEnumerator PriestHurt()
    {
        priestAnimator.Play("hurt");
        yield return null;
    }

    System.Collections.IEnumerator WolfRunAway()
    {
        wolfAnimator.Play("walk");
        float startX = currentWolf.transform.position.x;
        float endX = startX - 5f;
        while (currentWolf.transform.position.x > endX)
        {
            currentWolf.transform.position += Vector3.left * Time.deltaTime * 2f;
            yield return null;
        }
        Destroy(currentWolf);
    }

    // Метод для перезапуска боя
    public void RestartBattle()
    {
        currentKeyIndex = 0;
        correctCount = 0;
        wrongCount = 0;
        inputActive = true;
        
        InitializeBattle();
    }
}