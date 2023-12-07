using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed;

    [HideInInspector] public bool isMoving;

    [SerializeField] float walkSpeed;
    [SerializeField] float slowWalkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float slowSprintSpeed;

    [SerializeField] float groundDrag;

    public enum State
    {
        standing,
        walking,
        sprinting,
        crouching
    }

    public State state;

    [SerializeField] Transform obj;

    [Header("Jumping")]
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMultiplier;

    public bool readyToJump;

    [Header("Crouching")]
    [SerializeField] float crouchSpeed;

    [SerializeField] float crouchYScale;
    float startYScale;

    [SerializeField] Transform playerScaler;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    [SerializeField] LayerMask whatIsGround;
    public bool grounded;

    [SerializeField] Transform orientation;

    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;

    Vector3 moveDirection;

    [HideInInspector] public Rigidbody rb;
    public Animator animator;

    #region SingletonPattern
    private static PlayerMovementController instance = null;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
        }
    }

    public static PlayerMovementController Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }
    #endregion

    #region StatePattern
    private enum PlayerState
    {
        Stand, // ���ֱ�
        Walk, // �ȱ�
        Sprint, // �޸���
        Jump, // ����
        Crouch, // ��ũ����
        Slide, // �����̵�
        Roll, // ������
        Climb, // ������
        Dead // �ױ�
    }

    private StateMachine stateMachine;

    // ������Ʈ���� ����
    private Dictionary<PlayerState, IState> dicState = new Dictionary<PlayerState, IState>();
    #endregion

    void Start()
    {
        Init();
        StateInit();
    }

    private void Init()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        playerScaler.parent = null;
        transform.parent = playerScaler;

        startYScale = playerScaler.localScale.y;
    }

    private void StateInit()
    {
        //���� ����
        IState stand = new States.StateStanding();
        IState walk = new States.StateWalking();
        IState sprint = new States.StateSprinting();
        IState jump = new States.StateJumping();
        IState crouch = new States.StateCrouching();
        IState slide = new States.StateSliding();
        IState roll = new States.StateRolling();
        IState climb = new States.StateCllimbing();
        IState dead = new States.StateDeading();

        //Ű�Է� � ���� ������ ���¸� ���� �� �� �ְ� ��ųʸ��� ����
        dicState.Add(PlayerState.Stand, stand);
        dicState.Add(PlayerState.Walk, walk);
        dicState.Add(PlayerState.Sprint, sprint);
        dicState.Add(PlayerState.Jump, jump);
        dicState.Add(PlayerState.Crouch, crouch);
        dicState.Add(PlayerState.Slide, slide);
        dicState.Add(PlayerState.Roll, roll);
        dicState.Add(PlayerState.Climb, climb);
        dicState.Add(PlayerState.Dead, dead);

        //�⺻���´� �޸���� ����.
        stateMachine = new StateMachine(stand);
    }

    private void Update()
    {
        animator.SetFloat("Horizontal", horizontalInput);
        animator.SetFloat("Vertical", verticalInput);

        //�������� �����ؾ��ϴ� ���� ȣ��.
        stateMachine.DoOperateUpdate();

        MyInput();

        ControlSpeed();

        HandleState();

        HandleDrag();
    }

    private void MyInput()
    {
        // �Է°�
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        isMoving = horizontalInput != 0 || verticalInput != 0;

        // ����
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            stateMachine.SetState(dicState[PlayerState.Jump]);

            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // ��ũ���� ����
        if (Input.GetKeyDown(crouchKey) && grounded) 
        {
            playerScaler.localScale = new Vector3(playerScaler.localScale.x, crouchYScale, playerScaler.localScale.z);

            obj.localScale = new Vector3(1, 2, 1);
        }

        // ��ũ���� ��
        if (Input.GetKeyUp(crouchKey))
        {
            playerScaler.localScale = new Vector3(playerScaler.localScale.x, startYScale, playerScaler.localScale.z);

            obj.localScale = new Vector3(1, 1, 1);
        }
    }

    #region Jump
    private void Jump()
    {
        if (isMoving)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }
        else
        {
            rb.velocity = Vector3.zero;
        }

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        if (grounded)
        {
            stateMachine.SetState(dicState[PlayerState.Stand]);
        }

        readyToJump = true;
    }
    #endregion

    private void ControlSpeed()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // �̵� �ӷ��� �̵� �ӵ����� Ŭ ���, �ӵ��� ����
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void HandleState()
    {
        if (Input.GetKey(crouchKey) && grounded)
        {
            stateMachine.SetState(dicState[PlayerState.Crouch]);

            state = State.crouching;

            moveSpeed = crouchSpeed;
        }

        else if (grounded && Input.GetKey(sprintKey) && isMoving)
        {
            stateMachine.SetState(dicState[PlayerState.Sprint]);

            state = State.sprinting;

            if (verticalInput < 0 || horizontalInput != 0)
            {
                moveSpeed = slowSprintSpeed;
            }
            else
            {
                moveSpeed = sprintSpeed;
            }
        }

        else if (grounded && isMoving)
        {
            stateMachine.SetState(dicState[PlayerState.Walk]);

            state = State.walking;

            if (verticalInput < 0 || horizontalInput != 0)
            {
                moveSpeed = slowWalkSpeed;
            }
            else
            {
                moveSpeed = walkSpeed;
            }
        }

        else
        {
            if (grounded)
            {
                stateMachine.SetState(dicState[PlayerState.Stand]);

                state = State.standing;
            }
        }
    }

    private void HandleDrag()
    {
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10 * airMultiplier, ForceMode.Force);
        }
    }
}
