using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Сюда перетащишь объект игрока
    public float smoothSpeed = 10f; // Скорость сглаживания движения камеры (выше = быстрее)

    // Границы карты (в мировых координатах)
    // Их нужно будет настроить в Inspector так, чтобы они соответствовали краям твоей карты
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;
    private float initialZ; // Сохраняем начальную Z-позицию камеры

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: Target (игрок) не назначен!");
            enabled = false; // Выключаем скрипт, если нет цели
            return;
        }

        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraFollow: Компонент Camera не найден на этом GameObject!");
            enabled = false;
            return;
        }

        if (!cam.orthographic)
        {
            Debug.LogWarning("CameraFollow: Камера не ортографическая. Расчеты границ могут быть неточными для перспективной камеры.");
            // Для перспективной камеры расчеты camHalfHeight/Width сложнее.
            // Этот скрипт оптимизирован для ортографической 2D камеры.
        }

        initialZ = transform.position.z; // Сохраняем Z-координату камеры

        // Рассчитываем половину высоты и ширины видимой области камеры
        UpdateCameraDimensions();
    }

    void UpdateCameraDimensions()
    {
        // Для ортографической камеры
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = cam.aspect * camHalfHeight;
    }

    // LateUpdate вызывается после всех Update, что хорошо для слежения,
    // так как игрок уже должен был обновить свою позицию.
    void LateUpdate()
    {
        if (target == null || cam == null) return;

        // Если размер экрана/aspect ratio изменился, пересчитываем размеры камеры
        // Это можно оптимизировать, вызывая только при реальном изменении, но для простоты пока так
        if (cam.aspect * cam.orthographicSize != camHalfWidth) // Простая проверка на изменение
        {
            UpdateCameraDimensions();
        }

        // Желаемая позиция камеры - центр игрока, с сохранением Z камеры
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, initialZ);

        // Сглаженное движение к желаемой позиции
        // Используем 1.0f - Mathf.Exp(-smoothSpeed * Time.deltaTime) для frame-rate independent smoothing
        float t = 1.0f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, t);

        // Теперь ограничиваем позицию камеры границами карты, учитывая размеры самой камеры
        float clampedX = Mathf.Clamp(smoothedPosition.x, minX + camHalfWidth, maxX - camHalfWidth);
        float clampedY = Mathf.Clamp(smoothedPosition.y, minY + camHalfHeight, maxY - camHalfHeight);

        // Применяем новую позицию
        transform.position = new Vector3(clampedX, clampedY, initialZ);
        // transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, initialZ); 
    }

    // Опционально: Нарисовать границы в редакторе для удобства настройки
    void OnDrawGizmosSelected()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        UpdateCameraDimensions(); // Обновить размеры для Gizmos

        Gizmos.color = Color.red;
        // Рисуем фактические границы карты
        Gizmos.DrawLine(new Vector3(minX, minY, 0), new Vector3(maxX, minY, 0));
        Gizmos.DrawLine(new Vector3(maxX, minY, 0), new Vector3(maxX, maxY, 0));
        Gizmos.DrawLine(new Vector3(maxX, maxY, 0), new Vector3(minX, maxY, 0));
        Gizmos.DrawLine(new Vector3(minX, maxY, 0), new Vector3(minX, minY, 0));

        // Рисуем границы, в которых может находиться ЦЕНТР камеры
        Gizmos.color = Color.green;
        float cMinX = minX + camHalfWidth;
        float cMaxX = maxX - camHalfWidth;
        float cMinY = minY + camHalfHeight;
        float cMaxY = maxY - camHalfHeight;
        Gizmos.DrawLine(new Vector3(cMinX, cMinY, 0), new Vector3(cMaxX, cMinY, 0));
        Gizmos.DrawLine(new Vector3(cMaxX, cMinY, 0), new Vector3(cMaxX, cMaxY, 0));
        Gizmos.DrawLine(new Vector3(cMaxX, cMaxY, 0), new Vector3(cMinX, cMaxY, 0));
        Gizmos.DrawLine(new Vector3(cMinX, cMaxY, 0), new Vector3(cMinX, cMinY, 0));
    }
}