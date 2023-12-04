//�÷��̾�� ĳ����
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    private enum PlayerState
    {
        Stand,
        Walk,
        Sprint,
        Jump,
        Crouch,
        Slide,
        Roll,
        Climb,
        Dead
    }

    private StateMachine stateMachine;

    //������Ʈ���� ����
    private Dictionary<PlayerState, IState> dicState = new Dictionary<PlayerState, IState>();

    // Start is called before the first frame update
    void Start()
    {
        //���� ����
        IState stand = new StateStanding();
        IState walk = new StateWalking();
        IState sprint = new StateSprinting();
        IState jump = new StateJumping();
        IState crouch = new StateCrouching();
        IState slide = new StateSliding();
        IState roll = new StateRolling();
        IState climb = new StateCllimbing();
        IState dead = new StateDeading();

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

    // Update is called once per frame
    void Update()
    {
        //Ű�Է� �ޱ�
        KeyboardInput();

        //�������� �����ؾ��ϴ� ���� ȣ��.
        stateMachine.DoOperateUpdate();
    }

    //Ű���� �Է�
    void KeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //�޸���, �����̵� ���� ���� ���� ����
            stateMachine.SetState(dicState[PlayerState.Jump]);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            //�޸��� �߿��� �����̵� ����.
            if (stateMachine.CurrentState == dicState[PlayerState.Sprint])
            {
                stateMachine.SetState(dicState[PlayerState.Slide]);
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            stateMachine.SetState(dicState[PlayerState.Dead]);
        }
    }
}