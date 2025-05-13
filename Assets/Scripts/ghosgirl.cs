using UnityEngine;

public class GirlMovement : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 5f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer не найден на объекте!");
        }
    }

    void Update()
    {
        float horizontalInput = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1;
        if (Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1;

        float newX = transform.position.x + horizontalInput * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        // Поворот спрайта
        if (horizontalInput != 0)
        {
            spriteRenderer.flipX = horizontalInput < 0;
        }
    }
}