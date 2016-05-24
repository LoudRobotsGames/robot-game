using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MechUserControl : MonoBehaviour
{
    [SerializeField] private Transform m_Torso;
    [SerializeField] private Transform m_TiltPivot;
    [SerializeField] private Camera m_Camera;
    [Header("Input")]
    public string m_VerticalAxisName = "Vertical";
    public string m_HorizontalAxisName = "Horizontal";
    public string m_AimAxisName = "Mouse X";
    public string m_TiltAxisName = "Mouse Y";
    public float m_RotationSpeed = 1f;
    public float m_Acceleration = 1f;
    public float m_RotationSmoothing = 20f;
    [Header("Torso")]
    [SerializeField] private float m_MinVerticalAngle = -5f;
    [SerializeField] private float m_MaxVerticalAngle = 10f;
    [SerializeField] private float m_MaxHorizontalAngle = 45f;
    [Header("Weapons")]
    [SerializeField]
    public Image m_Crosshair;
    public Transform m_AimPoint;

    private float m_CurrentSpeed;
    private float m_CurrentTurnRate;
    private MechLocomotion m_MechLocomotion;
    private Transform m_Transform;
    private bool m_CursorLocked = true;
    private WeaponControl[] m_Weapons;
    private Vector3 m_AimLocation;
    private Quaternion m_TargetTorsoRotation;
    private Quaternion m_TargetTorsoTilt;

    public bool overrideInput = false;
    [Range(-1f, 1f)]
    public float speed = 0f;
    [Range(-1f, 1f)]
    public float turn = 0f;

    void Start()
    {
        m_MechLocomotion = GetComponent<MechLocomotion>();
        m_Weapons = GetComponentsInChildren<WeaponControl>(true);
        m_TargetTorsoRotation = Quaternion.identity;
        m_TargetTorsoTilt = Quaternion.identity;
        m_Transform = transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_CursorLocked = !m_CursorLocked;
        }
        UpdateCursorLock();
        UpdateThrottle();
        UpdateTurnRate();

        ConvergeWeapons();

        m_CurrentSpeed = Mathf.Clamp(m_CurrentSpeed, -1f, 1f);
        Vector3 move = m_CurrentSpeed * m_Transform.forward + m_CurrentTurnRate * m_Transform.right;

        if (overrideInput)
        {
            move = speed * m_Transform.forward + turn * m_Transform.right;
        }

        m_MechLocomotion.Move(move, false);

        TurnTorso();
        TiltTorso();
        FireWeapons();
    }

    private void UpdateTurnRate()
    {
        float h = Input.GetAxis(m_HorizontalAxisName);
        m_CurrentTurnRate = h;// Mathf.Lerp(m_CurrentTurnRate, h, 0.3f);
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

        //Vector3 screenPoint = Camera.main.WorldToScreenPoint(m_AimLocation);
        //if (m_Crosshair != null)
        //{
        //    m_Crosshair.transform.position = screenPoint;
        //}
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
        float yRot = m_RotationSpeed * 0.1f * Input.GetAxis(m_AimAxisName);

        m_TargetTorsoRotation *= Quaternion.Euler(0f, yRot, 0f);

        // Clamp
        Quaternion q = m_TargetTorsoRotation;
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
        angleY = Mathf.Clamp(angleY, -m_MaxHorizontalAngle, m_MaxHorizontalAngle);
        q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);
        m_TargetTorsoRotation = q;

        m_Torso.localRotation = Quaternion.Slerp(m_Torso.localRotation, m_TargetTorsoRotation,
                    m_RotationSmoothing * Time.deltaTime);
    }

    private void TiltTorso()
    {
        float xRot = -m_RotationSpeed * 0.1f * Input.GetAxis(m_TiltAxisName);

        m_TargetTorsoTilt *= Quaternion.Euler(xRot, 0f, 0f);

        // Clamp
        Quaternion q = m_TargetTorsoTilt;
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, m_MinVerticalAngle, m_MaxVerticalAngle);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        m_TargetTorsoTilt = q;

        m_TiltPivot.localRotation = Quaternion.Slerp(m_TiltPivot.localRotation, m_TargetTorsoTilt,
                m_RotationSmoothing * Time.deltaTime);
    }

    private void UpdateCursorLock()
    {
        if (m_CursorLocked)
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

    public void Footstep(int side)
    {
    }
}
