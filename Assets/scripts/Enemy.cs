using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float speed = 2f;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animCon;

    [Header("State")]
    public bool isLive;

    [Header("References")]
    public Transform target; // Rigidbody2D �� Transform ���� ����

    Rigidbody2D rigid;
    Collider2D coll;
    SpriteRenderer spriter;
    Animator anim;
    WaitForFixedUpdate wait;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        wait = new WaitForFixedUpdate();
    }

    void OnEnable()
    {
        // Ÿ�� ����
        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.transform;
        else
        {
            GameObject p = GameObject.FindWithTag("Player");
            target = p != null ? p.transform : null;
        }

        // �⺻ ���� �ʱ�ȭ
        isLive = true;
        health = maxHealth;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;

        if (anim != null)
            anim.SetBool("Dead", false);

        // �α� Ȯ�ο�
        if (target == null)
            Debug.LogWarning($"{name}: Target is NULL!");
        else
            Debug.Log($"{name}: Target found -> {target.name}");
    }

    void FixedUpdate()
    {
        if (GameManager.instance == null || !GameManager.instance.isLive)
            return;
        if (!isLive)
            return;
        if (target == null)
            return;
        if (anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        // �̵� ���� ���
        Vector2 dirVec = (target.position - transform.position).normalized;

        // �̵� ���� (Dynamic Rigidbody2D �ʿ�)
        Vector2 nextPos = rigid.position + dirVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(nextPos);
    }

    void LateUpdate()
    {
        if (target == null)
            return;
        spriter.flipX = target.position.x < transform.position.x;
    }

    public void Init(SpawnData data)
    {
        if (animCon != null && data.spriteType >= 0 && data.spriteType < animCon.Length)
            anim.runtimeAnimatorController = animCon[data.spriteType];
        else
            Debug.LogWarning("animCon index out of range or animCon null");

        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLive)
            return;

        if (!collision.CompareTag("Bullet"))
            return;

        if (!collision.TryGetComponent<Bullet>(out Bullet bullet))
            return;

        health -= bullet.damage;
        StartCoroutine(KnockBack());

        if (health > 0)
        {
            anim?.SetTrigger("Hit");
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        isLive = false;
        coll.enabled = false;
        rigid.simulated = false;
        spriter.sortingOrder = 1;
        anim?.SetBool("Dead", true);

        GameManager.instance.kill++;
        GameManager.instance.GetExp();

        if (GameManager.instance.isLive)
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.Dead);
    }

    IEnumerator KnockBack()
    {
        yield return wait;

        if (GameManager.instance == null || GameManager.instance.player == null)
            yield break;

        Vector3 dirVec = (transform.position - GameManager.instance.player.transform.position).normalized;
        if (rigid != null && rigid.simulated)
            rigid.AddForce(dirVec * 3f, ForceMode2D.Impulse);
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��ǵ��� (���� �ִϸ��̼� ��)
    void Dead()
    {
        gameObject.SetActive(false);
    }
}
