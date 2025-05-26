using UnityEngine;
using UnityEditor;

public class GridTools // Переименовал класс для большей общности
{
    private static float gridSize = 1.0f;
    // Целевой масштаб, к которому будут приводиться объекты
    private static Vector3 targetScale = Vector3.one; // Vector3.one это (1, 1, 1)

    [MenuItem("Инструменты/Выровнять по центру ячеек и масштабировать %g")] // Обновил название и оставил хоткей
    public static void SnapSelectedToCellCenterAndScale()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("Не выбраны объекты для выравнивания и масштабирования.");
            return;
        }

        string inputGridSize = EditorInputDialog.Show("Параметры сетки и масштаба",
                                                    "Введите размер ячейки сетки:",
                                                    gridSize.ToString());
        if (string.IsNullOrEmpty(inputGridSize) || !float.TryParse(inputGridSize, out float currentGridSize))
        {
            Debug.LogWarning("Отмена операции или некорректный ввод размера сетки.");
            return;
        }
        gridSize = currentGridSize;

        // Опционально: можно добавить диалог для ввода целевого масштаба, если он не всегда (1,1,1)
        // string inputScaleX = EditorInputDialog.Show("Параметры...", "Введите целевой масштаб X:", targetScale.x.ToString());
        // string inputScaleY = EditorInputDialog.Show("Параметры...", "Введите целевой масштаб Y:", targetScale.y.ToString());
        // string inputScaleZ = EditorInputDialog.Show("Параметры...", "Введите целевой масштаб Z:", targetScale.z.ToString());
        // if (float.TryParse(inputScaleX, out float sx) && float.TryParse(inputScaleY, out float sy) && float.TryParse(inputScaleZ, out float sz))
        // {
        //     targetScale = new Vector3(sx, sy, sz);
        // } else { Debug.LogWarning("Используется масштаб по умолчанию (1,1,1) из-за некорректного ввода."); }


        Undo.RecordObjects(Selection.transforms, "Выровнять по центру ячеек и масштабировать");

        int adjustedCount = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            Transform t = go.transform; // Удобнее работать с переменной transform
            Vector3 currentPosition = t.position;
            Vector3 currentScale = t.localScale;

            // --- Выравнивание по центру ячейки ---
            // Сначала находим "левый нижний" (или ближайший меньший) условный угол ячейки,
            // в которую попадает текущий центр объекта
            float cellOriginX = Mathf.Floor(currentPosition.x / gridSize) * gridSize;
            float cellOriginY = Mathf.Floor(currentPosition.y / gridSize) * gridSize;
            // float cellOriginZ = Mathf.Floor(currentPosition.z / gridSize) * gridSize; // Если нужно и для Z

            // Затем смещаем на половину размера ячейки, чтобы попасть в ее центр
            float snappedX = cellOriginX + (gridSize / 2.0f);
            float snappedY = cellOriginY + (gridSize / 2.0f);
            // float snappedZ = cellOriginZ + (gridSize / 2.0f); // Если нужно и для Z

            // Для 2D обычно Z-координата остается той же
            Vector3 newPosition = new Vector3(snappedX, snappedY, currentPosition.z);

            bool objectChanged = false;

            // Применяем новую позицию, если она изменилась
            if (t.position != newPosition)
            {
                t.position = newPosition;
                objectChanged = true;
            }

            // --- Масштабирование ---
            // Применяем целевой масштаб, если он отличается
            if (t.localScale != targetScale)
            {
                t.localScale = targetScale;
                objectChanged = true;
            }

            if (objectChanged)
            {
                adjustedCount++;
                EditorUtility.SetDirty(go); // Помечает объект как "измененный" для сохранения сцены
            }
        }

        if (adjustedCount > 0)
        {
            Debug.Log($"Скорректировано {adjustedCount} объектов: выровнены по центру ячеек (размер {gridSize}) и масштабированы до {targetScale}.");
        }
        else
        {
            Debug.Log("Выбранные объекты уже соответствуют целевым параметрам или не требуют корректировки.");
        }
    }


    // --- Вспомогательный класс для простого диалога ввода ---
    // (Можно вынести в отдельный файл Editor/EditorInputDialog.cs, если еще не сделано)
    public static class EditorInputDialog
    {
        public static string Show(string title, string message, string initialValue = "")
        {
            string text = initialValue;
            bool okPressed = false;

            EditorWindow window = EditorWindow.CreateInstance<InputDialogWindow>();
            InputDialogWindow dialog = window as InputDialogWindow;
            if (dialog != null)
            {
                dialog.titleContent = new GUIContent(title);
                dialog.message = message;
                dialog.inputText = initialValue;
                dialog.onOk = (result) => { text = result; okPressed = true; };
                dialog.onCancel = () => { okPressed = false; };
                dialog.ShowModal();
            }
            return okPressed ? text : null;
        }

        private class InputDialogWindow : EditorWindow
        {
            public string message;
            public string inputText;
            public System.Action<string> onOk;
            public System.Action onCancel;

            void OnGUI()
            {
                EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);
                inputText = EditorGUILayout.TextField(inputText);
                GUILayout.Space(10);
                if (GUILayout.Button("OK"))
                {
                    onOk?.Invoke(inputText);
                    Close();
                }
                if (GUILayout.Button("Отмена"))
                {
                    onCancel?.Invoke();
                    Close();
                }
            }
            void OnLostFocus() { /* Close(); // Можно раскомментировать, если хотите, чтобы окно закрывалось при потере фокуса */ }
        }
    }
}