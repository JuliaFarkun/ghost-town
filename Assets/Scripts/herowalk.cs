using UnityEngine;

public class HeroController : MonoBehaviour
{
    public float speed = 3f;
    private Animator animator;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator.Play("idle"); // Запуск анимации idle при старте
    }

    void Update()
    {
        movement = Vector2.zero;

        if (Input.GetKey(KeyCode.DownArrow))
        {
            movement.y = -1;
            animator.Play("walkright"); // Анимация ходьбы лицом к игроку
            spriteRenderer.flipX = false;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            movement.y = 1;
            animator.Play("walkback"); // Анимация ходьбы спиной к игроку
            spriteRenderer.flipX = false;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            movement.x = -1;
            animator.Play("walk");
            spriteRenderer.flipX = false; // Лицом вправо
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            movement.x = 1;
            animator.Play("walkright");
            spriteRenderer.flipX = true; // Лицом влево (отражение по X)
        }
        else
        {
            animator.Play("idle"); // Если не нажата ни одна стрелка — idle
        }

        transform.Translate(movement * speed * Time.deltaTime);
    }
}