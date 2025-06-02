using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Настройки Фоновой Музыки")]
    public AudioClip backgroundMusicClip;

    private AudioSource audioSource;

    void Awake()
    {
        // Получаем компонент AudioSource, или добавляем его, если его нет
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Настраиваем AudioSource для фоновой музыки
        audioSource.loop = true; // Зацикливаем музыку
        audioSource.playOnAwake = false; // Не проигрываем сразу, будем управлять из скрипта
        audioSource.spatialBlend = 0; // 2D звук (не зависит от позиции в мире)
    }

    void Start()
    {
        if (backgroundMusicClip != null)
        {
            audioSource.clip = backgroundMusicClip;
            audioSource.Play();
            Debug.Log($"[BackgroundMusicManager] Начато воспроизведение фоновой музыки: {backgroundMusicClip.name}");
        }
        else
        {
            Debug.LogWarning("[BackgroundMusicManager] Клип фоновой музыки не назначен в инспекторе!");
        }
    }

    // Опционально: методы для управления музыкой (пауза, остановка, смена клипа)
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("[BackgroundMusicManager] Фоновая музыка остановлена.");
        }
    }

    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("[BackgroundMusicManager] Фоновая музыка на паузе.");
        }
    }

    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying && audioSource.time > 0)
        {
            audioSource.UnPause();
            Debug.Log("[BackgroundMusicManager] Воспроизведение фоновой музыки возобновлено.");
        }
    }

     public void SetAndPlayClip(AudioClip newClip)
    {
        if (audioSource != null && newClip != null)
        {
            audioSource.Stop(); // Останавливаем текущую музыку
            audioSource.clip = newClip;
            audioSource.Play();
            Debug.Log($"[BackgroundMusicManager] Сменен и начато воспроизведение нового клипа: {newClip.name}");
        } else if (newClip == null)
        {
             Debug.LogWarning("[BackgroundMusicManager] Попытка установить пустой клип музыки.");
        }
    }
} 