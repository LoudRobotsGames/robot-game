using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MechSystem : MonoBehaviour, IDamageable
{
    public enum SystemLocation
    {
        LeftLeg,
        RightLeg,
        CenterTorso,
        LeftTorso,
        RightTorso,
        LeftArm,
        RightArm,
        TopWeapon,
        Head
    }

	public enum DestroyType
	{
        ExplodeAndNotify,
		DeactivateGameObject,
		DettachGameObject,
		SwapGameObject,
		RagdollGameObject,

		COUNT
	}

    public GameObject m_RootMechObject;
    public SystemLocation m_SystemLocation;
    public MechSystem m_ChildSystem;
    public MechSystem m_ParentSystem;
    public Armor m_Armor;
    public DamageableStructure m_InternalStructure;
    public List<InternalSystem> m_InternalSystem;
	public Explode m_ExplodeOnDestruction;
	public DestroyType m_DestroyType;

    public void Start()
    {
        ResetDamage();
    }

    public void ResetDamage()
    {
        m_Armor.Reset();
        m_InternalStructure.Reset();
        UpdateUI();
    }

    public void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // Armor ablates first and reduces damage
        int excessDamage = m_Armor.TakeDamage(damage);

        // Calculate internal damage. Armor might have failed or it might be too weak to stop all the damage
        int internalDamage = m_Armor.CalculatePiercing(excessDamage);
        
        // If any damage done to internals, it may damage an internal system
        if (internalDamage > 0)
        {

        }

        // If any damage remaining, deal it to the internal structure
        m_InternalStructure.TakeDamage(internalDamage);

		if( m_InternalStructure.m_CurrentHealth <= 0 && m_ExplodeOnDestruction != null)
		{
			HandleDestruction();
			m_ExplodeOnDestruction.TriggerExplosion( hitPoint, hitNormal );
		}

        UpdateUI();
    }

    public float AssessDamage()
    {
        float armor = (float)m_Armor.m_CurrentHealth / (float)m_Armor.m_MaxHealth;
        float intStructure = (float)m_InternalStructure.m_CurrentHealth / (float)m_InternalStructure.m_MaxHealth;

        return (armor + intStructure) / 2f;
    }

    public void UpdateUI()
    {
        if (m_RootMechObject == HUD.Instance.SelectedEnemy)
        {
            HUD.Instance.EnemyHealth.UpdateUIFromSystem(this);
        }
        else if (m_RootMechObject == HUD.Instance.Player)
        {
            HUD.Instance.PlayerHealth.UpdateUIFromSystem(this);
        }
    }

    private void HandleDestruction()
	{
        // Ensure the armor values are at zero and Update the UI
        m_Armor.m_CurrentHealth = 0;
        m_InternalStructure.m_CurrentHealth = 0;
        UpdateUI();

		switch( m_DestroyType )
		{
			case DestroyType.DeactivateGameObject:
				gameObject.SetActive( false );
				break;
			case DestroyType.DettachGameObject:
				gameObject.transform.parent = null;
				EnsureHasRigidBody( gameObject );
				DisableAnimator( gameObject );
				break;
		}

        if (m_ChildSystem != null)
        {
            m_ChildSystem.HandleDestruction();
        }
	}

	private void DisableAnimator( GameObject go )
	{
		Animator anim = go.GetComponent<Animator>();
		if( anim != null )
		{
			anim.applyRootMotion = false;
			anim.enabled = false;
		}
	}

	private void EnsureHasRigidBody(GameObject go)
	{
		Rigidbody rb = go.GetComponent<Rigidbody>();
		if( rb == null )
		{
			rb = go.AddComponent<Rigidbody>();
		}
		rb.useGravity = true;
        rb.mass = 10f;
	}

    #region local types
    [Serializable]
    public class DamageableStructure
    {
        public int m_MaxHealth;
        [HideInInspector]
        public int m_CurrentHealth;

        public void Reset()
        {
            m_CurrentHealth = m_MaxHealth;
        }

        public int TakeDamage(int damage)
        {
            int damageDone = Mathf.Min(m_CurrentHealth, damage);
            m_CurrentHealth -= damageDone;

            int excessDamage = damage - damageDone;
            return excessDamage;
        }
    }

    [Serializable]
    public class Armor : DamageableStructure
    {
        public float AblativeStrength = 0.1f;

        public int CalculatePiercing(int damage)
        {
            int ablationAmount = Mathf.RoundToInt((float)m_CurrentHealth * AblativeStrength);
            return Mathf.Max(damage - ablationAmount, 0);
        }

    }
    #endregion
}
