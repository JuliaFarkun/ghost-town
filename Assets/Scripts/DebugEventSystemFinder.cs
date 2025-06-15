using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class DebugEventSystemFinder : MonoBehaviour
{
    void Start()
    {
        // Найти все активные EventSystem в сцене
        EventSystem[] allEventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        Debug.Log($"[DebugEventSystemFinder] Обнаружено EventSystem в сцене при старте: {allEventSystems.Length}");

        if (allEventSystems.Length > 1)
        {
            Debug.LogWarning("[DebugEventSystemFinder] Обнаружено более одной EventSystem!");
            for (int i = 0; i < allEventSystems.Length; i++)
            {
                Debug.LogWarning($"[DebugEventSystemFinder] EventSystem {i}: Имя: {allEventSystems[i].name}, Активен: {allEventSystems[i].gameObject.activeInHierarchy}");
            }
        }
    }
} 