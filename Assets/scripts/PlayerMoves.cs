using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public Vector2 inputVec;
    public FloatingJoystick joystick;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    private Vector2 actionInput = Vector2.zero;
    private Vector2 keyboardInput = Vector2.zero;

    private Vector2 currentVelocity = Vector2.zero;

    public float acceleration = 60f;
    public float deceleration = 80f;

    public float sprintMultiplier = 1.6f;
    public float sprintDuration = 3f;
    public float sprintCooldown = 30f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    private bool isSprintingActive = false;
    private bool sprintAvailable = true;
    private float sprintTimer = 0f;
    private float cooldownTimer = 0f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    void OnEnable()
    {
        speed *= Charater.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        keyboardInput = Vector2.zero;
        bool keyboardDetected = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                keyboardInput.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                keyboardInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                keyboardInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                keyboardInput.x += 1f;

            keyboardDetected = keyboardInput != Vector2.zero;
        }
        else
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                keyboardInput.y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                keyboardInput.y -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                keyboardInput.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                keyboardInput.x += 1f;

            keyboardDetected = keyboardInput != Vector2.zero;
        }

        Vector2 chosenInput = keyboardDetected ? keyboardInput : actionInput;

        if (chosenInput.sqrMagnitude > 1f)
            chosenInput = chosenInput.normalized;

        bool shiftPressedThisFrame = false;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame)
                shiftPressedThisFrame = true;
        }
        else
        {
            if (Input.GetKeyDown(sprintKey))
                shiftPressedThisFrame = true;
        }

        if (shiftPressedThisFrame && sprintAvailable && !isSprintingActive)
        {
            StartSprint();
        }

        if (isSprintingActive)
        {
            sprintTimer += Time.deltaTime;
            if (sprintTimer >= sprintDuration)
            {
                StopSprintAndStartCooldown();
            }
        }
        else if (!sprintAvailable)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= sprintCooldown)
            {
                sprintAvailable = true;
                cooldownTimer = 0f;
            }
        }

        inputVec = chosenInput;

        if (anim != null)
            anim.SetBool("IsSprinting", isSprintingActive);
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;

        Vector2 desiredDir = inputVec;
        float targetSpeed = speed;

        if (isSprintingActive)
            targetSpeed *= sprintMultiplier;

        Vector2 targetVelocity = desiredDir * targetSpeed;

        if (targetVelocity.sqrMagnitude > currentVelocity.sqrMagnitude)
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, deceleration * Time.fixedDeltaTime);
        }

        Vector2 moveDelta = currentVelocity * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + moveDelta);

        float animSpeedRatio = 0f;
        if (speed != 0f)
        {
            float denom = speed * (isSprintingActive ? sprintMultiplier : 1f);
            animSpeedRatio = currentVelocity.magnitude / denom;
            animSpeedRatio = Mathf.Clamp(animSpeedRatio, 0f, 1f);
        }
        anim.SetFloat("Speed", animSpeedRatio);

        if (inputVec.x != 0)
            spriter.flipX = inputVec.x < 0;
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;

        if (inputVec.x != 0)
            spriter.flipX = inputVec.x < 0;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive)
            return;
        GameManager.instance.health -= Time.deltaTime * 10;

        if (GameManager.instance.health < 0)
        {
            for (int i = 2; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }
    }

    void OnMove(InputValue value)
    {
        actionInput = value.Get<Vector2>();
    }

    private void StartSprint()
    {
        isSprintingActive = true;
        sprintAvailable = false;
        sprintTimer = 0f;
        cooldownTimer = 0f;
        if (anim != null)
            anim.SetBool("IsSprinting", true);
    }

    private void StopSprintAndStartCooldown()
    {
        isSprintingActive = false;
        sprintTimer = 0f;
        cooldownTimer = 0f;
        sprintAvailable = false;
        if (anim != null)
            anim.SetBool("IsSprinting", false);
    }
}
