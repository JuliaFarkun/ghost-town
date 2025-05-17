using UnityEngine;
using UnityEngine.UI; // Для работы с UI элементами типа Dropdown, Slider
using UnityEngine.Audio; // Если будешь использовать AudioMixer
using System.Collections.Generic; // Для List
using TMPro; // Для TextMeshPro элементов

public class SettingsManager : MonoBehaviour
{
    // --- Звук ---
    // Если используешь AudioMixer, перетащи его сюда в Inspector
    // public AudioMixer mainMixer;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText; // Опционально, для отображения значения

    // --- Разрешение ---
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;

    // --- Сохранение/Загрузка ---
    private const string VOLUME_KEY = "MasterVolume";
    private const string RESOLUTION_WIDTH_KEY = "ResolutionWidth";
    private const string RESOLUTION_HEIGHT_KEY = "ResolutionHeight";
    private const string RESOLUTION_REFRESH_KEY = "ResolutionRefresh";


    void Start()
    {
        // --- Настройка разрешений ---
        resolutions = Screen.resolutions; // Получаем все доступные разрешения
        resolutionDropdown.ClearOptions(); // Очищаем стандартные опции

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio + "Hz";
            options.Add(option);

            // Находим текущее разрешение экрана, чтобы выбрать его в Dropdown по умолчанию
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height &&
                resolutions[i].refreshRateRatio.value == Screen.currentResolution.refreshRateRatio.value)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);

        // Загружаем настройки при старте
        LoadSettings(currentResolutionIndex);

        // Добавляем слушателей событий для UI элементов
        // Это можно сделать и через Inspector, но так нагляднее, что происходит при изменении
        if (volumeSlider) volumeSlider.onValueChanged.AddListener(SetVolume);
        if (resolutionDropdown) resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume; // Глобальная громкость
        // Если используешь AudioMixer:
        // mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20); // Громкость в децибелах

        if (volumeValueText) volumeValueText.text = Mathf.RoundToInt(volume * 100).ToString() + "%";
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= resolutions.Length) return;

        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
        Debug.Log($"Resolution set to: {resolution.width}x{resolution.height}@{resolution.refreshRateRatio}Hz");
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, AudioListener.volume);
        // PlayerPrefs.SetFloat(VOLUME_KEY, volumeSlider.value); // Если громкость берется напрямую со слайдера

        if (resolutionDropdown.value < resolutions.Length)
        {
            Resolution currentRes = resolutions[resolutionDropdown.value];
            PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, currentRes.width);
            PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, currentRes.height);
            PlayerPrefs.SetInt(RESOLUTION_REFRESH_KEY, (int)currentRes.refreshRateRatio.value); // Сохраняем как int
        }
        PlayerPrefs.Save(); // Не забываем сохранить изменения в PlayerPrefs
        Debug.Log("Settings Saved!");
    }

    public void LoadSettings(int defaultResolutionIndex)
    {
        // Громкость
        float volume = PlayerPrefs.GetFloat(VOLUME_KEY, 0.75f); // 0.75f - значение по умолчанию
        if (volumeSlider) volumeSlider.value = volume;
        SetVolume(volume); // Применяем громкость

        // Разрешение
        int savedWidth = PlayerPrefs.GetInt(RESOLUTION_WIDTH_KEY, Screen.currentResolution.width);
        int savedHeight = PlayerPrefs.GetInt(RESOLUTION_HEIGHT_KEY, Screen.currentResolution.height);
        int savedRefresh = PlayerPrefs.GetInt(RESOLUTION_REFRESH_KEY, (int)Screen.currentResolution.refreshRateRatio.value);

        int loadedResolutionIndex = defaultResolutionIndex; // По умолчанию текущее
        for(int i=0; i < resolutions.Length; i++)
        {
            if(resolutions[i].width == savedWidth &&
               resolutions[i].height == savedHeight &&
               (int)resolutions[i].refreshRateRatio.value == savedRefresh)
            {
                loadedResolutionIndex = i;
                break;
            }
        }

        if (resolutionDropdown)
        {
             resolutionDropdown.value = loadedResolutionIndex;
             resolutionDropdown.RefreshShownValue(); // Обновить отображаемое значение
        }
        // Применять разрешение при загрузке настроек не всегда нужно,
        // т.к. оно уже должно быть установлено системой или при предыдущем запуске.
        // Но если нужно: SetResolution(loadedResolutionIndex);

        Debug.Log("Settings Loaded!");
    }

    // Вызови этот метод, например, по кнопке "Применить" или при выходе из настроек
    // В нашем случае, можно вызывать SaveSettings() при закрытии окна настроек (в ReturnToPreviousScene)
    // или добавить кнопку "Применить"
}