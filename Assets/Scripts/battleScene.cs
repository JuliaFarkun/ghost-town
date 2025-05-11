using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BattleSceneController : MonoBehaviour
{
    public Animator priestAnimator;
    public Animator wolfAnimator;
    public Transform priestTransform;
    public Transform wolfTransform;

    public List<GameObject> keyObjects; // Перетащите сюда ваши объекты-клавиши в нужном порядке
    public List<KeyCode> keysToPress = new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F };

    public TMP_Text resultText;

    public Vector3 priestStartPosition;
    public Vector3 wolfStartPosition;

    private int currentKeyIndex = 0;
    private int correctCount = 0;
    private int wrongCount = 0;
    private bool inputActive = true;

    void Start()
    {
        Debug.Log("Animator assigned: " + (priestAnimator != null));
        Debug.Log("Has controller: " + (priestAnimator.runtimeAnimatorController != null));
        priestAnimator.Play("idle");
        wolfAnimator.Play("walk"); // Волк по умолчанию бежит на месте
        resultText.text = "";
        priestTransform.localScale = new Vector3(-1, 1, 1); // Герой всегда зеркален
        wolfTransform.localScale = new Vector3(1, 1, 1);    // Волк всегда зеркален
        HighlightCurrentKey();

        // Сохраняем стартовые позиции (можно задать в инспекторе или здесь)
        priestTransform.position = priestStartPosition;
        wolfTransform.position = wolfStartPosition;
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
            // Проверяем, что нажата не та клавиша, которая ожидается
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
            var image = keyObjects[i].GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                if (i == currentKeyIndex)
                    image.color = Color.yellow; // Подсветка активной клавиши
                else if (i < currentKeyIndex)
                    image.color = Color.gray;   // Уже нажатые
                else
                    image.color = Color.white;  // Обычные
            }
        }
    }

    System.Collections.IEnumerator PlayPriestAttack()
    {
        priestTransform.localScale = new Vector3(-1, 1, 1); // Герой всегда зеркален
        priestAnimator.Play("attack");
        yield return new WaitForSeconds(0.7f);
        priestAnimator.Play("idle");
    }

    System.Collections.IEnumerator PlayWolfAttack()
    {
        wolfTransform.localScale = new Vector3(1, 1, 1);
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
        priestTransform.localScale = new Vector3(-1, 1, 1); // Герой всегда зеркален
        priestAnimator.Play("victory");
        yield return null;
    }

    System.Collections.IEnumerator PriestHurt()
    {
        priestTransform.localScale = new Vector3(-1, 1, 1); // Герой всегда зеркален
        priestAnimator.Play("hurt");
        yield return null;
    }

    System.Collections.IEnumerator WolfRunAway()
    {
        wolfTransform.localScale = new Vector3(1, 1, 1); // Волк всегда зеркален
        wolfAnimator.Play("walk");
        float startX = wolfTransform.position.x;
        float endX = startX - 5f;
        while (wolfTransform.position.x > endX)
        {
            wolfTransform.position += Vector3.left * Time.deltaTime * 2f;
            yield return null;
        }
        wolfAnimator.Play("walk");
    }
}