using UnityEngine;
using System.Collections;
using System;

public class WeaponControl : MonoBehaviour
{
    public enum ShotType
    {
        Projectile,
        Laser
    }

    [SerializeField]
    private Transform m_Mount;
    [SerializeField]
    private Transform m_VerticalPivot;
    [SerializeField]
    private Transform m_HorizontalPivot;
    [SerializeField]
    private GameObject m_ShotPrefab;
    [SerializeField]
    private GameObject m_MuzzleFlashPrefab;
    [SerializeField]
    private Transform m_FirePoint;
    [SerializeField]
    private Transform m_ShotTarget;
    [SerializeField]
    private ShotType m_ShotType;
    [SerializeField]
    private float m_MaxVerticalAngle = 10;
    [SerializeField]
    private float m_MaxHorizontalAngle = 5;
    [SerializeField]
    private float m_ConvergenceSmoothing = 0.125f;
    [SerializeField]
    private string m_FireButtonName = "Fire1";

    // Shot we will fire and its effect settings
    private GameObject m_Shot;
    private GameObject m_MuzzleFlash;
    private EffectSettings m_EffectSettings;

    // Use this for initialization
    void Start()
    {
        m_Shot = GameObject.Instantiate<GameObject>(m_ShotPrefab);
        m_EffectSettings = m_Shot.GetComponent<EffectSettings>();
        m_EffectSettings.Target = m_ShotTarget.gameObject;
        m_EffectSettings.EffectDeactivated += DeactivateMuzzleFlash;

        m_MuzzleFlash = GameObject.Instantiate<GameObject>(m_MuzzleFlashPrefab);

        m_Shot.SetActive(false);
        m_MuzzleFlash.SetActive(false);
        m_Shot.hideFlags = HideFlags.HideAndDontSave;
        m_MuzzleFlash.hideFlags = HideFlags.HideAndDontSave;
        m_MuzzleFlash.transform.parent = m_FirePoint;
        m_MuzzleFlash.transform.localPosition = Vector3.zero;
    }

    private void DeactivateMuzzleFlash(object sender, EventArgs e)
    {
        m_MuzzleFlash.SetActive(false);
    }

    public void LateUpdate()
    {
    }

    public void AttemptFire(bool overrideInput = false)
    {
        if (m_Shot.activeSelf)
        {
            return;
        }

        // Didn't press the button? Don't fire
        if (!overrideInput && !Input.GetButtonDown(m_FireButtonName))
        {
            return;
        }

        //m_Shot = GameObject.Instantiate<GameObject>(m_ShotPrefab);
        m_EffectSettings = m_Shot.GetComponent<EffectSettings>();
        m_EffectSettings.Target = m_ShotTarget.gameObject;

        m_Shot.transform.position = m_FirePoint.position;
        m_Shot.transform.rotation = m_FirePoint.rotation;
        m_Shot.SetActive(true);
        m_MuzzleFlash.SetActive(true);
    }

    internal void AimAt(Vector3 aimLocation)
    {
        Vector3 dir = aimLocation - m_FirePoint.position;

        Vector3 fDir = new Vector3(dir.x, 0f, dir.z);
        fDir.Normalize();
        Debug.DrawRay(m_FirePoint.position, fDir, Color.blue);
        float yAngle = Vector3.Angle(fDir, m_Mount.forward);
        float xAngle = Vector3.Angle(fDir, m_Mount.up) - 90f;
        xAngle = 0f;// Mathf.Clamp(xAngle, -m_MaxVerticalAngle, m_MaxVerticalAngle);
        yAngle = Mathf.Clamp(yAngle, -m_MaxHorizontalAngle, m_MaxHorizontalAngle);

        if (m_HorizontalPivot != m_VerticalPivot)
        {
            m_HorizontalPivot.localRotation = Quaternion.Slerp(m_HorizontalPivot.localRotation, Quaternion.Euler(xAngle, 0f, 0f), m_ConvergenceSmoothing * Time.deltaTime);
            m_VerticalPivot.localRotation = Quaternion.Slerp(m_HorizontalPivot.localRotation, Quaternion.Euler(yAngle, 0f, 0f), m_ConvergenceSmoothing * Time.deltaTime);
        }
        else
        {
            m_HorizontalPivot.localRotation = Quaternion.Slerp(m_HorizontalPivot.localRotation, Quaternion.Euler(xAngle, yAngle, 0f), m_ConvergenceSmoothing * Time.deltaTime);
        }
        Debug.DrawRay(m_FirePoint.position, m_FirePoint.forward, Color.yellow);
    }

    public void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawRay(m_FirePoint.position, m_FirePoint.forward * 50f);
    }
}
