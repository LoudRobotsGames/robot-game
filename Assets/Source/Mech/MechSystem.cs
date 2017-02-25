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
    public List<ResourceCount> m_ResourceCounts;
	public Explode m_ExplodeOnDestruction;
	public DestroyType m_DestroyType;
    
    private int m_ResourceStep;

    public void SetupCommonState(BaseBlueprint blueprint)
    {
        m_Armor = new Armor(blueprint.Armor);
        m_InternalStructure = new DamageableStructure(blueprint.Internal);

        int count = blueprint.ResourceCosts.Count;
        m_ResourceCounts = new List<ResourceCount>(count);
        int resourceTotal = 0;
        for (int i = 0; i < count; ++i)
        {
            m_ResourceCounts.Add(new ResourceCount(blueprint.ResourceCosts[i]));
            resourceTotal += blueprint.ResourceCosts[i].Count;
        }
        m_ResourceStep = Mathf.RoundToInt((float)resourceTotal / (float)m_InternalStructure.m_MaxHealth);
    }

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
            ReduceResources(internalDamage);
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

    private void ReduceResources(int internalDamage)
    {
        internalDamage *= m_ResourceStep;
        for (int i = 0; i < m_ResourceCounts.Count; ++i)
        {
            int amount = Mathf.Min(m_ResourceCounts[i].Count, internalDamage);
            m_ResourceCounts[i].Count -= amount;
            internalDamage -= amount;
            if(internalDamage <= 0)
            {
                break;
            }
        }
    }

    public float AssessDamage()
    {
        float armor = (float)m_Armor.m_CurrentHealth / (float)m_Armor.m_MaxHealth;
        float intStructure = (float)m_InternalStructure.m_CurrentHealth / (float)m_InternalStructure.m_MaxHealth;

        return (armor + intStructure) / 2f;
    }

    public bool IsDestroyed()
    {
        return AssessDamage() <= 0.0f;
    }

    public void UpdateUI()
    {
        HUD hud = HUD.Instance;
        if(hud == null)
        {
            return;
        }

        if (m_RootMechObject == hud.SelectedEnemy)
        {
            hud.EnemyHealth.UpdateUIFromSystem(this);
        }
        else if (m_RootMechObject == hud.Player)
        {
            hud.PlayerHealth.UpdateUIFromSystem(this);
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

        if (Mission.CurrentMission != null)
        {
            Mission.CurrentMission.OnPartDestroyed(this);
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
}
