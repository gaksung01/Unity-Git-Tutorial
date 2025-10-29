using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody2D rig;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    public float speed;
    private bool isFacingRight = true;
    private bool isDead = false;
    public float inpX, inpY;
    public Vector2 inputVec;
    public Scanner scanner;

    public RuntimeAnimatorController[] animCon;
    public Hand[] hands;

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    void Update()
    {
        if (isDead) return;

        inpX = Input.GetAxisRaw("Horizontal");
        inpY = Input.GetAxisRaw("Vertical");

        // ✅ 이동 벡터 계산 (애니메이션에 사용)
        inputVec = new Vector2(inpX, inpY);

        // ✅ 방향 전환 (Flip)
        if (inpX > 0 && !isFacingRight)
            Flip();
        else if (inpX < 0 && isFacingRight)
            Flip();

        // ✅ 🔥 애니메이션 Speed 파라미터 갱신
        if (anim != null)
            anim.SetFloat("Speed", inputVec.magnitude);
    }

    void FixedUpdate()
    {
        if (isDead) return;
        Move();
    }

    void Move()
    {
        Vector3 move = new Vector3(inpX, inpY, 0f).normalized;
        transform.Translate(move * moveSpeed * Time.deltaTime);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        anim.SetTrigger("Dead");
        rig.velocity = Vector2.zero;
        this.enabled = false;
    }
}
