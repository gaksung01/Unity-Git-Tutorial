using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;




public class PlayerMove : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody2D rig;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private bool isFacingRight = true;
    private bool isDead = false;
    private float inpX, inpY;

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    
    void Update()
    {
        if (isDead) return;

        inpX = UnityEngine.Input.GetAxisRaw("Horizontal");
        inpY = UnityEngine.Input.GetAxisRaw("Vertical");

        if (inpX > 0 && !isFacingRight)
            Flip();
        else if (inpX < 0 && isFacingRight)
            Flip();

        Vector2 move = new Vector2(inpX, inpY);
        anim.SetFloat("Speed", move.magnitude);
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
