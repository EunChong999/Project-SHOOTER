using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpCollisionController : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        #region JumpUp
        if (other.gameObject.layer == LayerMask.NameToLayer("whatIsGround") &&
            PlayerMovementController.Instance.grounded)
        {
            PlayerMovementController.Instance.grounded = false;
            PlayerMovementController.Instance.animator.SetBool("Land", false);
        }
        #endregion
    }

    private void OnTriggerStay(Collider other)
    {
        #region JumpDown
        if (other.gameObject.layer == LayerMask.NameToLayer("whatIsGround"))
        {
            if (!PlayerMovementController.Instance.grounded)
            {
                PlayerMovementController.Instance.isLanding = false;
                PlayerMovementController.Instance.animator.SetTrigger("JumpDown");
                PlayerMovementController.Instance.grounded = true;
            }
            else
            {
                // �������� �� �̵��ϰų� ��ũ�� ���
                if (PlayerMovementController.Instance.isMoving ||
                    PlayerMovementController.Instance.isCrouching)
                {
                    PlayerMovementController.Instance.animator.SetTrigger("Land");
                    PlayerMovementController.Instance.isLanding = true;
                }
            }
        }
        #endregion
    }
}
