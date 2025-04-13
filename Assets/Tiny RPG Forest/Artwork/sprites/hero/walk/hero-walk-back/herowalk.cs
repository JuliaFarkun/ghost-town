using UnityEngine;

public class HeroController : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Сначала сбрасываем все параметры
        animator.SetBool("WalkBack", false);
        animator.SetBool("WalkFront", false);
        animator.SetBool("WalkSlide", false);

        if (Input.GetKey(KeyCode.UpArrow))
        {
            Debug.Log("Up Arrow Pressed");
            animator.SetBool("WalkBack", true);
            transform.Translate(Vector3.up * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            Debug.Log("Down Arrow Pressed");
            animator.SetBool("WalkFront", true);
            transform.Translate(Vector3.down * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Debug.Log("Right Arrow Pressed");
            animator.SetBool("WalkSlide", true);
            transform.Translate(Vector3.right * Time.deltaTime);
            if (spriteRenderer is not null)
                spriteRenderer.flipX = false;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            Debug.Log("Left Arrow Pressed");
            animator.SetBool("WalkSlide", true);
            transform.Translate(Vector3.left * Time.deltaTime);
            if (spriteRenderer is not null)
                spriteRenderer.flipX = true;
        }
    }
}