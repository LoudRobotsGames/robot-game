using UnityEngine;
using System.Collections;
using System;

public class WeaponControl : MonoBehaviour
{
    private const int SHOT_COUNT = 4;

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
    private float m_ShotInterval;
    [SerializeField]
    private float m_MaxVerticalAngle = 10;
    [SerializeField]
    private float m_MaxHorizontalAngle = 5;
    [SerializeField]
    private float m_ConvergenceSmoothing = 0.125f;
    [SerializeField]
    private string m_FireButtonName = "Fire1";

    private float m_Timer = 0f;

    // Shot we will fire and its effect settings
    private class WeaponShot
    {
        public GameObject Shot;
        public GameObject MuzzleFlash;
        public EffectSettings FXSettings;

        public bool Active;
        
        public void DeactivateMuzzleFlash(object sender, EventArgs e)
        {
            MuzzleFlash.SetActive(false);
            Active = false;
        }
    }
    private WeaponShot[] m_ShotPool = new WeaponShot[SHOT_COUNT];

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < SHOT_COUNT; ++i)
        {
            WeaponShot weaponShot = new WeaponShot();
            GameObject shot = GameObject.Instantiate<GameObject>(m_ShotPrefab);
            EffectSettings fxSettings = shot.GetComponent<EffectSettings>();
            fxSettings.Target = m_ShotTarget.gameObject;
            fxSettings.EffectDeactivated += weaponShot.DeactivateMuzzleFlash;

            GameObject muzzleFlash = GameObject.Instantiate<GameObject>(m_MuzzleFlashPrefab);

            shot.SetActive(false);
            muzzleFlash.SetActive(false);
            //shot.hideFlags = HideFlags.HideAndDontSave;
            //muzzleFlash.hideFlags = HideFlags.HideAndDontSave;
            muzzleFlash.transform.parent = m_FirePoint;
            muzzleFlash.transform.localPosition = Vector3.zero;

            weaponShot.Shot = shot;
            weaponShot.MuzzleFlash = muzzleFlash;
            weaponShot.FXSettings = fxSettings;
            weaponShot.Active = false;
            m_ShotPool[i] = weaponShot;
        }

        m_Timer = m_ShotInterval;
    }
    
    public void LateUpdate()
    {
        if (m_Timer >= 0f)
        {
            m_Timer -= Time.deltaTime;
        }
    }

    private WeaponShot GetFreeShot()
    {
        for (int i = 0; i < SHOT_COUNT; ++i)
        {
            if (m_ShotPool[i].Active == false)
                return m_ShotPool[i];
        }
        return null;
    }

    public void AttemptFire(bool overrideInput = false)
    {
        WeaponShot ws = GetFreeShot();
        if (ws == null)
        {
            return;
        }

        // Didn't press the button? Don't fire
        if (!overrideInput && !Input.GetButtonDown(m_FireButtonName))
        {
            return;
        }

        if (m_Timer > 0f)
        {
            return;
        }

        // Adjust the timer
        m_Timer += m_ShotInterval;

        ws.Active = true;
        ws.FXSettings.Target = m_ShotTarget.gameObject;

        ws.Shot.transform.position = m_FirePoint.position;
        ws.Shot.transform.rotation = m_FirePoint.rotation;
        ws.Shot.SetActive(true);
        ws.MuzzleFlash.SetActive(true);
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
