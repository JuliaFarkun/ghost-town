using UnityEngine;
using UnityEditor;

public class GridAlignerEditor : EditorWindow
{
    // Статические переменные для хранения настроек между вызовами окна
    private static Vector2 _cellSize = Vector2.one;
    private static Vector2Int _targetCellSpan = Vector2Int.one;

    // Метод, который будет вызываться для отображения окна
    [MenuItem("Tools/Align Selected to Grid")]
    public static void ShowWindow()
    {
        // Получаем существующее окно или создаем новое
        GridAlignerEditor window = GetWindow<GridAlignerEditor>("Grid Aligner");
        window.minSize = new Vector2(250, 150); // Минимальный размер окна
    }

    // Отрисовка GUI окна
    void OnGUI()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        _cellSize = EditorGUILayout.Vector2Field("Cell Size", _cellSize);
        _targetCellSpan = EditorGUILayout.Vector2IntField("Target Cell Span", _targetCellSpan);

        // Валидация введенных значений
        if (_cellSize.x <= 0) _cellSize.x = 0.01f;
        if (_cellSize.y <= 0) _cellSize.y = 0.01f;
        if (_targetCellSpan.x < 1) _targetCellSpan.x = 1;
        if (_targetCellSpan.y < 1) _targetCellSpan.y = 1;

        GUILayout.Space(10);

        if (GUILayout.Button("Apply to Selected Objects"))
        {
            ApplyAlignmentToSelected();
        }

        GUILayout.Space(5);
        EditorGUILayout.HelpBox("This tool will align the position and size of all selected GameObjects according to the specified grid settings.", MessageType.Info);
    }

        private static void ApplyAlignmentToSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Grid Aligner: No objects selected.");
            return;
        }

        Undo.RecordObjects(selectedObjects, "Align Objects to Grid");

        int alignedCount = 0;
        foreach (GameObject go in selectedObjects)
        {
            // --- 1. Выравнивание Позиции (центр объекта в центре ячейки) ---
            Vector3 currentPosition = go.transform.position;

            // Находим ближайший левый нижний угол ячейки сетки
            // (можно использовать Round, если хотим к ближайшей ячейке, а не той, где левый нижний угол объекта)
            // Если хотим строго, чтобы объект попадал в ячейку, куда указывает его pivot:
            // float gridCornerX = Mathf.Floor(currentPosition.x / _cellSize.x) * _cellSize.x;
            // float gridCornerY = Mathf.Floor(currentPosition.y / _cellSize.y) * _cellSize.y;

            // Если хотим, чтобы pivot объекта был привязан к центру ближайшей ячейки
            // Сначала находим индекс ближайшей ячейки
            float cellIndexX = Mathf.Round(currentPosition.x / _cellSize.x);
            float cellIndexY = Mathf.Round(currentPosition.y / _cellSize.y);

            // Затем вычисляем центр этой ячейки
            // Центр ячейки = (индекс_ячейки * размер_ячейки) - (размер_ячейки / 2), если индексы от 0
            // Или: (индекс_ячейки * размер_ячейки) + (размер_ячейки / 2) - размер_ячейки, если pivot в углу
            // Более простой способ:
            // 1. Найти ближайшую линию сетки к currentPosition.x: Mathf.Round(currentPosition.x / _cellSize.x) * _cellSize.x
            // 2. Определить, слева или справа от этой линии находится центр ячейки (т.е. currentPosition.x)
            //    и сместиться на cellSize.x / 2 в нужную сторону.
            //
            // Самый простой и понятный вариант:
            // Сначала округляем позицию до ближайшей линии сетки (это будет угол ячейки)
            float roundedX = Mathf.Round(currentPosition.x / _cellSize.x) * _cellSize.x;
            float roundedY = Mathf.Round(currentPosition.y / _cellSize.y) * _cellSize.y;

            // Затем смещаем на половину ячейки. Направление смещения зависит от того,
            // в какую сторону произошло округление.
            // Чтобы гарантированно попасть в центр ячейки, можно сделать так:
            // 1. Найти "индекс" ячейки, в которую попадает текущий pivot
            float currentCellRawIndexX = currentPosition.x / _cellSize.x;
            float currentCellRawIndexY = currentPosition.y / _cellSize.y;

            // 2. Округлить до ближайшего целого индекса (это будет индекс ячейки, к центру которой мы хотим привязаться)
            float targetCellIndexX = Mathf.Round(currentCellRawIndexX);
            float targetCellIndexY = Mathf.Round(currentCellRawIndexY);

            // 3. Координата центра этой ячейки: (индекс * размер) + (размер / 2), если мы считаем,
            //    что 0 - это линия. Если мы хотим, чтобы 0 - это центр первой ячейки (от -size/2 до +size/2),
            //    то просто (индекс * размер).
            //    Чтобы не путаться, проще так:
            //    Находим ближайшую линию сетки (как было раньше):
            //    float nearestGridLineX = Mathf.Round(currentPosition.x / _cellSize.x) * _cellSize.x;
            //    float nearestGridLineY = Mathf.Round(currentPosition.y / _cellSize.y) * _cellSize.y;
            //    Это координаты углов ячеек. Теперь нам нужно сместиться на половину ячейки от этого угла.
            //    Но в какую сторону?
            //
            // Предлагаю такой подход:
            // 1. Определяем, в какой "слот" сетки попадает текущая позиция (с учетом смещения на половину ячейки)
            float snappedX = (Mathf.Round((currentPosition.x - _cellSize.x / 2.0f) / _cellSize.x) * _cellSize.x) + _cellSize.x / 2.0f;
            float snappedY = (Mathf.Round((currentPosition.y - _cellSize.y / 2.0f) / _cellSize.y) * _cellSize.y) + _cellSize.y / 2.0f;

            go.transform.position = new Vector3(snappedX, snappedY, currentPosition.z);


            // --- 2. Выравнивание Размера ---
            float targetWorldWidth = _targetCellSpan.x * _cellSize.x;
            float targetWorldHeight = _targetCellSpan.y * _cellSize.y;

            SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Undo.RecordObject(spriteRenderer, "Align Sprite Size");

                if (spriteRenderer.sprite == null)
                {
                    go.transform.localScale = new Vector3(targetWorldWidth, targetWorldHeight, go.transform.localScale.z);
                }
                else
                {
                    #if UNITY_2020_1_OR_NEWER
                    if (spriteRenderer.drawMode == SpriteDrawMode.Simple)
                    #else
                    if (spriteRenderer.drawMode == SpriteDrawMode.Simple)
                    #endif
                    {
                        if (spriteRenderer.sprite.bounds.size.x == 0 || spriteRenderer.sprite.bounds.size.y == 0)
                        {
                            go.transform.localScale = new Vector3(targetWorldWidth, targetWorldHeight, go.transform.localScale.z);
                        }
                        else
                        {
                            float scaleX = targetWorldWidth / spriteRenderer.sprite.bounds.size.x;
                            float scaleY = targetWorldHeight / spriteRenderer.sprite.bounds.size.y;
                            go.transform.localScale = new Vector3(scaleX, scaleY, go.transform.localScale.z);
                        }
                    }
                    else if (spriteRenderer.drawMode == SpriteDrawMode.Sliced || spriteRenderer.drawMode == SpriteDrawMode.Tiled)
                    {
                        spriteRenderer.size = new Vector2(targetWorldWidth, targetWorldHeight);
                    }
                }
                EditorUtility.SetDirty(spriteRenderer);
            }
            else
            {
                go.transform.localScale = new Vector3(targetWorldWidth, targetWorldHeight, go.transform.localScale.z);
            }

            EditorUtility.SetDirty(go);
            alignedCount++;
        }

        if (alignedCount > 0)
        {
            Debug.Log($"Grid Aligner: Aligned {alignedCount} selected objects.");
        }
    }
}