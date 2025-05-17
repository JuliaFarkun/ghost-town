using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    // --- Звук ---
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;

    // --- Разрешение ---
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;

    // --- Полноэкранный режим ---
    public Toggle fullscreenToggle; // Сюда перетащишь Toggle из Hierarchy

    // --- Ключи для PlayerPrefs ---
    private const string VOLUME_KEY = "MasterVolume";
    private const string RESOLUTION_WIDTH_KEY = "ResolutionWidth";
    private const string RESOLUTION_HEIGHT_KEY = "ResolutionHeight";
    private const string RESOLUTION_REFRESH_KEY = "ResolutionRefresh";
    private const string FULLSCREEN_KEY = "IsFullScreen"; // Новый ключ

    void Start()
    {
        // --- Настройка разрешений (остается как было) ---
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        // ... (код заполнения resolutionDropdown остается) ...
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio + "Hz";
            options.Add(option);
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

        // Добавляем слушателей событий
        if (volumeSlider) volumeSlider.onValueChanged.AddListener(SetVolume);
        if (resolutionDropdown) resolutionDropdown.onValueChanged.AddListener(SetResolutionFromDropdown); // Переименовал для ясности
        if (fullscreenToggle) fullscreenToggle.onValueChanged.AddListener(SetFullscreen); // Новый слушатель
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        if (volumeValueText) volumeValueText.text = Mathf.RoundToInt(volume * 100).ToString() + "%";
    }

    // Переименовал метод, чтобы не путать с прямым вызовом Screen.SetResolution
    public void SetResolutionFromDropdown(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= resolutions.Length) return;
        Resolution resolution = resolutions[resolutionIndex];
        SetScreenResolution(resolution.width, resolution.height, Screen.fullScreen); // Используем текущее состояние fullscreen
    }

    // Общий метод для установки разрешения и полноэкранного режима
    void SetScreenResolution(int width, int height, bool isFullscreen)
    {
        Screen.SetResolution(width, height, isFullscreen);
        Debug.Log($"Resolution set to: {width}x{height}, Fullscreen: {isFullscreen}");
    }


    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log("Fullscreen mode set to: " + isFullscreen);
        // При смене полноэкранного режима, текущее разрешение окна сохраняется,
        // если это оконный режим, или используется разрешение рабочего стола/выбранное ранее для полноэкранного.
        // Если нужно принудительно установить конкретное разрешение при переходе в полноэкранный режим,
        // можно вызвать SetScreenResolution здесь с нужными параметрами.
        // Screen.SetResolution(Screen.width, Screen.height, isFullscreen); // Обновит с текущими размерами
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, AudioListener.volume);

        if (resolutionDropdown.value >= 0 && resolutionDropdown.value < resolutions.Length)
        {
            Resolution currentRes = resolutions[resolutionDropdown.value];
            PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, currentRes.width);
            PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, currentRes.height);
            PlayerPrefs.SetInt(RESOLUTION_REFRESH_KEY, (int)currentRes.refreshRateRatio.value);
        }

        // Сохраняем состояние полноэкранного режима
        if (fullscreenToggle)
        {
            PlayerPrefs.SetInt(FULLSCREEN_KEY, fullscreenToggle.isOn ? 1 : 0); // 1 для true, 0 для false
        }

        PlayerPrefs.Save();
        Debug.Log("Settings Saved!");
    }

    public void LoadSettings(int defaultResolutionIndex)
    {
        // Громкость
        float volume = PlayerPrefs.GetFloat(VOLUME_KEY, 0.75f);
        if (volumeSlider) volumeSlider.value = volume;
        SetVolume(volume);

        // Разрешение
        int savedWidth = PlayerPrefs.GetInt(RESOLUTION_WIDTH_KEY, Screen.resolutions[defaultResolutionIndex].width);
        int savedHeight = PlayerPrefs.GetInt(RESOLUTION_HEIGHT_KEY, Screen.resolutions[defaultResolutionIndex].height);
        // PlayerPrefs не хранит RefreshRateRatio напрямую, так что возьмем int
        int savedRefresh = PlayerPrefs.GetInt(RESOLUTION_REFRESH_KEY, (int)Screen.resolutions[defaultResolutionIndex].refreshRateRatio.value);


        int loadedResolutionIndex = defaultResolutionIndex;
        for(int i=0; i < resolutions.Length; i++)
        {
            if(resolutions[i].width == savedWidth &&
               resolutions[i].height == savedHeight &&
               (int)resolutions[i].refreshRateRatio.value == savedRefresh) // Сравниваем как int
            {
                loadedResolutionIndex = i;
                break;
            }
        }
        if (resolutionDropdown)
        {
             resolutionDropdown.value = loadedResolutionIndex;
             resolutionDropdown.RefreshShownValue();
        }

        // Загружаем и применяем настройку полноэкранного режима
        // По умолчанию (если ключ не найден) пусть будет полноэкранный (1). Или 0 для оконного.
        bool isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) == 1;
        if (fullscreenToggle)
        {
            fullscreenToggle.isOn = isFullscreen; // Устанавливаем состояние галочки
        }
        // Применяем загруженное состояние. Делаем это после установки галочки,
        // чтобы не вызвать onValueChanged, если значение не изменилось.
        // Либо можно временно отписать слушателя, применить, и подписать обратно.
        // Но проще просто применить:
        Screen.fullScreen = isFullscreen;


        // Применяем загруженное разрешение с учетом загруженного состояния fullscreen
        // Это нужно, чтобы при первом запуске или после сброса настроек все было корректно
        // Screen.SetResolution(savedWidth, savedHeight, isFullscreen);
        // Однако, если пользователь менял разрешение, оно уже должно быть установлено.
        // Вызов SetResolutionFromDropdown выше уже учтет это.
        // Достаточно, чтобы Screen.fullScreen был установлен.

        Debug.Log("Settings Loaded!");
    }
}