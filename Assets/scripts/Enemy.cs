using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;

    public bool isLive;

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
        wait = new WaitForFixedUpdate();
        coll = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        // 안전한 GameManager와 플레이어/타겟 체크
        if (GameManager.instance == null)
            return;
        if (!GameManager.instance.isLive)
            return;
        if (!isLive)
            return;
        if (anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;
        if (target == null)
            return;

        Vector2 dirVec = (Vector2)target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (GameManager.instance == null || GameManager.instance.player == null)
            return;
        if (target == null)
            return;
        spriter.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable()
    {
        // 안전하게 할당
        if (GameManager.instance != null && GameManager.instance.player != null)
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        else
            target = null;

        isLive = true;
        health = maxHealth;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        if (anim != null)
            anim.SetBool("Dead", false);
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

        // 안전하게 Bullet 컴포넌트 얻기
        if (!collision.TryGetComponent<Bullet>(out Bullet bullet))
            return;

        health -= bullet.damage;
        StartCoroutine(KnockBack());

        if (health > 0)
        {
            if (anim != null)
                anim.SetTrigger("Hit");
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 1;
            if (anim != null)
                anim.SetBool("Dead", true);

            GameManager.instance.kill++;
            GameManager.instance.GetExp();

            if (GameManager.instance.isLive)
                AudioManager.instance?.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    IEnumerator KnockBack()
    {
        yield return wait; // 한 프레임 대기(물리 프레임)
        // 플레이어 위치 조회도 안전하게
        if (GameManager.instance == null || GameManager.instance.player == null)
            yield break;

        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;

        // rigid가 시뮬레이션되어 있을 때만 AddForce
        if (rigid != null && rigid.simulated)
        {
            rigid.AddForce(dirVec.normalized * 3f, ForceMode2D.Impulse);
        }
    }

    // 애니메이션 이벤트에서 호출되도록 (예: 죽음 애니메이션 끝)
    void Dead()
    {
        gameObject.SetActive(false);
    }
}
