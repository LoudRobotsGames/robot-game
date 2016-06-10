using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class MechLocomotion : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private bool m_ExtraRotationAtRun;
	[SerializeField] float m_MovingTurnSpeed = 360;
	[SerializeField] float m_StationaryTurnSpeed = 180;
    [SerializeField] private CharacterController m_CharController;
    [SerializeField] private float m_JumpPower = 12f;
    [SerializeField] private float m_GravityMultiplier = 2f;
    [SerializeField] private float m_AnimSpeedMultiplier = 1f;
    [SerializeField] private float m_MoveSpeedMultiplier = 1f;
    [SerializeField] private float m_StickToGroundForce;
	[SerializeField] float m_GroundCheckDistance = 0.1f;

    #region Animator related fields
    private Animator m_Animator;
    private int m_SpeedHash = Animator.StringToHash("Speed");
    private int m_TurnHash = Animator.StringToHash("Turn");
    private int m_GroundedHash = Animator.StringToHash("Grounded");
    private int m_JumpHash = Animator.StringToHash("Jump");
    private int m_RunStateHash = Animator.StringToHash("Base Layer.Run");
    #endregion

    private float m_TurnAmount;
    private float m_ForwardAmount;
    private Vector3 m_GroundNormal;
    private float m_OrigGroundCheckDistance;
    private Transform m_Transform;
    private bool m_PreviouslyGrounded;

    public Vector3 Velocity
    {
        get
        {
            return m_CharController.velocity;
        }
    }

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Transform = transform;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;
    }

    /*public void OnAnimatorMove()
    {
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (m_CharController.isGrounded && Time.deltaTime > 0)
        {
            Vector3 v = m_Animator.deltaPosition * m_MovementSpeed;
            Quaternion q = m_Animator.deltaRotation;
            transform.rotation *= q;

            // we preserve the existing y part of the current velocity.
            v.y = m_CharController.velocity.y - 1f;
            m_CharController.Move(v);
        }
    }*/

    public void Move(Vector3 move, bool jump)
    {
        // convert the world relative moveInput vector into a local-relative 
        // turn amount and forward amount required to head in the desired direction
        //if (move.magnitude > 1f)
        //    move.Normalize();

        move = m_Transform.InverseTransformDirection(move);
        if (move.z < 0f)
        {
            move.z = move.z * 0.5f;
        }
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, Vector3.up);
        m_TurnAmount = Mathf.Atan2(move.x, Mathf.Abs(move.z)) / (Mathf.PI * 0.5f);
        m_ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        UpdateAnimator(move);

        if (m_CharController.isGrounded)
        {
            move = HandleGroundedMovement(move, jump);
        }
        else
        {
            move = HandleAirborneMovement(move);
        }

        UpdateCharacterController(move);

        m_PreviouslyGrounded = m_CharController.isGrounded;
    }

    private void ApplyExtraTurnRotation()
    {
        if (m_ExtraRotationAtRun )
        {
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
            transform.Rotate(0f, m_TurnAmount * turnSpeed * Time.deltaTime, 0f);
        }
    }

    private void UpdateCharacterController(Vector3 move)
    {
        m_CharController.Move(move * Time.deltaTime);
    }

    private void UpdateAnimator(Vector3 move)
    {
        m_Animator.SetFloat(m_SpeedHash, m_ForwardAmount, 0.1f, Time.deltaTime);
        m_Animator.SetFloat(m_TurnHash, m_TurnAmount, 0.1f, Time.deltaTime);
        m_Animator.SetBool(m_GroundedHash, m_CharController.isGrounded);
        
        if(m_CharController.isGrounded && move.magnitude > 0)
        {
            m_Animator.speed = m_AnimSpeedMultiplier;
        }
        else
        {
            m_Animator.speed = 1f;
        }
    }

    private Vector3 HandleAirborneMovement(Vector3 move)
    {
        move += Physics.gravity * m_GravityMultiplier;

        m_GroundCheckDistance = m_CharController.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
        return move;
    }

    private Vector3 HandleGroundedMovement(Vector3 move, bool jump)
    {
        move.x = 0f;
        move.z = 0f;
        move.y -= m_StickToGroundForce;
        if (jump)
            move.y = m_JumpPower;

        m_GroundCheckDistance = m_OrigGroundCheckDistance;
        return move;
    }

    void CheckGroundStatus()
    {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Vector3 start = transform.position + (Vector3.up * 0.1f);
        Vector3 end = transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance);
        Debug.DrawLine(start, end, Color.red);
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            m_GroundNormal = hitInfo.normal;
        }
        else
        {
            m_GroundNormal = Vector3.up;
        }
    }
}
