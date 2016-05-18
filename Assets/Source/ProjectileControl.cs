using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileControl : MonoBehaviour, IWeaponEffect
{
    [SerializeField]
    private GameObject m_MuzzleFlash;
    [SerializeField]
    private GameObject m_Shot;
    [SerializeField]
    private GameObject m_Impact;
    [SerializeField]
    private float m_Range = 10f;
    [SerializeField]
    private float m_Speed = 10f;

    private Transform m_CachedTransform;
    private Vector3 m_FireDirection;
    private Vector3 m_FirePoint;
    
    public void Start()
    {
        m_CachedTransform = transform;
        ProjectileCollision[] projCollisions = m_Shot.GetComponentsInChildren<ProjectileCollision>();
        for (int i = 0; i < projCollisions.Length; ++i)
        {
            projCollisions[i].OnCollision += OnProjectileCollision;
        }

        m_Shot.SetActive(false);
        m_Impact.SetActive(false);
        m_MuzzleFlash.SetActive(false);
    }

    private void OnProjectileCollision(object sender, EventArgs e)
    {
        ProjectileCollision projCol = sender as ProjectileCollision;
        if (projCol != null)
        {
            Collision collision = projCol.ActiveCollision;
            Impact(collision.contacts[0].point);
        }
    }

    private void Impact(Vector3 position)
    {
        m_Impact.transform.position = position;
        m_Impact.SetActive(true);
    }

    [ContextMenu("Fire")]
    public void Fire()
    {
        m_MuzzleFlash.SetActive(true);

        m_Shot.transform.localPosition = Vector3.zero;//= m_CachedTransform.position;
        m_Shot.transform.localRotation = Quaternion.identity;// = m_CachedTransform.rotation;
        m_FireDirection = m_Shot.transform.forward;
        m_FirePoint = m_CachedTransform.position;

        // Reset the trail renderers if their are any, otherwise trails will appear wierd.
        TrailRenderer[] trailRenderers = m_Shot.GetComponentsInChildren<TrailRenderer>(true);
        for (int i = 0; i < trailRenderers.Length; ++i)
        {
            //trailRenderers[i].Clear();
        }

        m_Shot.SetActive(true);

        Invoke("Deactivate", 10f);
    }

    public void Deactivate()
    {
        m_Shot.SetActive(false);
        m_MuzzleFlash.SetActive(false);
        m_Impact.SetActive(false);
    }

    public void Update()
    {
        if (m_Shot.activeSelf == false)
        {
            return;
        }

        if (Vector3.Distance(m_FirePoint, m_Shot.transform.position) >= m_Range)
        {
            Impact(m_Shot.transform.position);
        }
        else
        {
            m_Shot.transform.position += m_FireDirection * m_Speed * Time.deltaTime;
        }
    }
}