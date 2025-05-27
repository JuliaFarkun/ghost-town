using UnityEngine;
using UnityEditor;

public class WallToolsEditor
{
    private static string wallTagName = "Wall"; // Имя тега для стен
    // private static string wallLayerName = "Walls"; // Имя слоя для стен (раскомментируйте, если будете использовать)

    [MenuItem("Инструменты/Сделать выделенное стенами")]
    public static void MakeSelectedObjectsWalls()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Не выбраны объекты для превращения в стены.");
            return;
        }

        // Проверяем, существует ли тег "Wall"
        bool tagExists = false;
        foreach (string t in UnityEditorInternal.InternalEditorUtility.tags)
        {
            if (t.Equals(wallTagName))
            {
                tagExists = true;
                break;
            }
        }

        if (!tagExists)
        {
            EditorUtility.DisplayDialog("Ошибка тега",
                $"Тег '{wallTagName}' не найден в проекте. Пожалуйста, добавьте его через Edit -> Project Settings -> Tags and Layers, а затем запустите скрипт снова.",
                "OK");
            return;
        }

        /* // Раскомментируйте этот блок, если хотите использовать слои
        int wallLayer = LayerMask.NameToLayer(wallLayerName);
        if (wallLayer == -1) // LayerMask.NameToLayer возвращает -1, если слой не найден
        {
            EditorUtility.DisplayDialog("Ошибка слоя",
                $"Слой '{wallLayerName}' не найден в проекте. Пожалуйста, добавьте его через Edit -> Project Settings -> Tags and Layers, а затем запустите скрипт снова.",
                "OK");
            return;
        }
        */

        Undo.RecordObjects(selectedObjects, "Сделать объекты стенами"); // Для изменений тегов и слоев

        int objectsModified = 0;
        foreach (GameObject go in selectedObjects)
        {
            bool modifiedThisObject = false;

            // 1. Добавляем BoxCollider2D, если его нет
            BoxCollider2D collider = go.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = Undo.AddComponent<BoxCollider2D>(go); // Undo.AddComponent регистрирует добавление
                // По умолчанию BoxCollider2D попытается подогнаться под SpriteRenderer,
                // если он есть. Для квадратов 1x1 с пивотом в центре это обычно то, что нужно.
                // Если ваши квадраты всегда 1x1, можно явно задать:
                // collider.size = Vector2.one;
                // collider.offset = Vector2.zero;
                modifiedThisObject = true;
            }
            // Если коллайдер уже есть, мы его не трогаем, но можно добавить логику для его настройки, если нужно.

            // 2. Устанавливаем тег
            if (go.tag != wallTagName)
            {
                go.tag = wallTagName;
                modifiedThisObject = true;
            }

            /* // 3. Устанавливаем слой (раскомментируйте, если используете)
            if (go.layer != wallLayer)
            {
                go.layer = wallLayer;
                modifiedThisObject = true;
            }
            */

            if (modifiedThisObject)
            {
                EditorUtility.SetDirty(go); // Помечаем объект как измененный для сохранения
                objectsModified++;
            }
        }

        if (objectsModified > 0)
        {
            Debug.Log($"{objectsModified} объектов были настроены как стены (добавлен BoxCollider2D и/или тег '{wallTagName}').");
        }
        else
        {
            Debug.Log("Выбранные объекты уже настроены как стены или не требуют изменений.");
        }
    }
}