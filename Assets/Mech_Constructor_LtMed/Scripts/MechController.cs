using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class MechController : MonoBehaviour
{
    public enum ViewAndControlType
    {
        AimAtMouse,
        TurnByMouse,
    }

    [Header("Movement")]
    public Transform m_Torso;
    public Camera m_Camera;
    public bool m_ExtraRotationAtRun;
    public ViewAndControlType m_ControlType = ViewAndControlType.AimAtMouse;
    public CharacterController m_CharController;
    public float m_MovementSpeed = 100f;
    [Header("Input")]
    public string m_VerticalAxisName = "Vertical";
    public string m_HorizontalAxisName = "Horizontal";
    public string m_AimAxisName = "Mouse X";
    public float m_RotationSpeed = 1f;
    public float m_Acceleration = 1f;
    public float m_RotationSmoothing = 20f;
    [Header("Weapons")]
    [SerializeField]
    public Image m_Crosshair;
    public Transform m_AimPoint;

    #region Animator related fields
    private Animator m_Animator;
    private int m_SpeedHash = Animator.StringToHash("Speed");
    private int m_TurnHash = Animator.StringToHash("Turn");
    private int m_JumpHash = Animator.StringToHash("Jump");
    private int m_RunStateHash = Animator.StringToHash("Base Layer.Run");
    #endregion
    private float m_CurrentSpeed = 0f;
    private float m_CurrentTurnRate = 0f;
    private bool m_CursorLockedInTurnMode = true;
    private WeaponControl[] m_Weapons;
    private Vector3 m_AimLocation;
    private Quaternion m_TargetTorsoRotation;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Weapons = GetComponentsInChildren<WeaponControl>(true);
        m_TargetTorsoRotation = Quaternion.identity;
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_CursorLockedInTurnMode = !m_CursorLockedInTurnMode;
        }
        UpdateCursorLock();
        UpdateCameraParent();
        UpdateThrottle();
        UpdateTurnRate();

        ConvergeWeapons();

        m_CurrentSpeed = Mathf.Clamp(m_CurrentSpeed, -1f, 1f);
        m_Animator.SetFloat(m_SpeedHash, m_CurrentSpeed, 0.1f, Time.deltaTime);
        m_Animator.SetFloat(m_TurnHash, m_CurrentTurnRate, 0.1f, Time.deltaTime);

        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        if (Input.GetKeyDown(KeyCode.Space) && stateInfo.fullPathHash == m_RunStateHash)
        {
            m_Animator.SetTrigger(m_JumpHash);
        }


        if (m_CharController.isGrounded == false)
        {
            Vector3 moveDir = Physics.gravity * Time.fixedDeltaTime;
            m_CharController.Move(moveDir);
        }

        if (m_ExtraRotationAtRun && m_CurrentSpeed > 0.5f)
        {
            transform.Rotate(0f, m_RotationSpeed * Time.deltaTime * m_CurrentTurnRate, 0f);
        }

        TurnTorso();
        FireWeapons();
    }

    private void ConvergeWeapons()
    {
        RaycastHit hit;
        if (Physics.Raycast(m_AimPoint.position, m_AimPoint.forward, out hit, 50f))
        {
            m_AimLocation = hit.point;
        }
        else
        {
            m_AimLocation = m_AimPoint.position + (m_AimPoint.forward * 50f);
        }
        for (int i = 0; i < m_Weapons.Length; ++i)
        {
            m_Weapons[i].AimAt(m_AimLocation);
        }

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(m_AimLocation);
        m_Crosshair.transform.position = screenPoint;
    }

    private void UpdateTurnRate()
    {
        float h = Input.GetAxis(m_HorizontalAxisName);
        m_CurrentTurnRate = Mathf.Lerp(m_CurrentTurnRate, h, 0.3f);
    }

    private void UpdateThrottle()
    {
        float v = Input.GetAxis(m_VerticalAxisName);
        if (Mathf.Abs(v) > 0.05f)
        {
            m_CurrentSpeed += v * m_Acceleration * Time.deltaTime;
        }
        else
        {
            float decel = m_Acceleration * Time.deltaTime;
            decel = Mathf.Min(decel, Mathf.Abs(m_CurrentSpeed));
            if (m_CurrentSpeed > 0)
            {
                m_CurrentSpeed -= decel;
            }
            else if (m_CurrentSpeed < 0)
            {
                m_CurrentSpeed += decel;
            }
        }
    }

    private void UpdateCameraParent()
    {
        if (m_ControlType == ViewAndControlType.AimAtMouse)
        {
            if (m_Camera.transform.parent != transform)
            {
                m_Camera.transform.parent = transform;
            }
        }
        else if (m_ControlType == ViewAndControlType.TurnByMouse)
        {
            if (m_Camera.transform.parent != m_Torso)
            {
                m_Camera.transform.parent = m_Torso;
            }
        }
    }

    private void FireWeapons()
    {
        for (int i = 0; i < m_Weapons.Length; ++i)
        {
            m_Weapons[i].AttemptFire();
        }
    }

    private void TurnTorso()
    {
        if (m_ControlType == ViewAndControlType.AimAtMouse)
        {
            Vector3 mousePoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
            Vector3 worldPoint = m_Camera.ScreenToWorldPoint(mousePoint);
            // Flatten the y for now
            worldPoint.y = m_Torso.position.y;

            // Use look at to compute the target rotation
            //Quaternion q = m_Torso.rotation;
            m_Torso.LookAt(worldPoint);
            //Quaternion target = m_Torso.rotation;
        }
        else if (m_ControlType == ViewAndControlType.TurnByMouse)
        {
            float yRot = m_RotationSpeed * 0.1f * Input.GetAxis(m_AimAxisName);

            m_TargetTorsoRotation *= Quaternion.Euler(0f, yRot, 0f);

            m_Torso.localRotation = Quaternion.Slerp(m_Torso.localRotation, m_TargetTorsoRotation,
                    m_RotationSmoothing * Time.deltaTime);
        }
    }

    private void UpdateCursorLock()
    {
        if (m_CursorLockedInTurnMode && m_ControlType == ViewAndControlType.TurnByMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 aim = m_AimLocation;// m_AimPoint.position + (m_AimPoint.forward * 20f);
        Gizmos.DrawLine(m_AimPoint.position, aim);
        Gizmos.DrawSphere(aim, 0.1f);

    }
}
