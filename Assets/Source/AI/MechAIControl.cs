using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(MechLocomotion))]
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class MechAIControl : MonoBehaviour
{ 
    public UnityEngine.AI.NavMeshAgent Agent { get; private set; }             // the navmesh agent required for the path finding
    public MechLocomotion Character { get; private set; } // the character we are controlling
    public Transform Target;                                    // target to aim for

	private Transform m_Transform;
	private Vector2 m_SmoothDeltaPosition = Vector2.zero;
	private Vector2 m_Velocity = Vector2.zero;

    private void Start()
    {
        // get the components on the object we need ( should not be null due to require component so no need to check )
        Agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        Character = GetComponent<MechLocomotion>();

	    Agent.updatePosition = false;
		Agent.updateRotation = false;
		m_Transform = transform;
    }

    private void Update()
    {
        if (Target != null)
        {
            Agent.SetDestination(Target.position);
        }

		Vector3 worldDeltaPosition = Agent.nextPosition - m_Transform.position;
       
        if (Agent.remainingDistance > Agent.stoppingDistance)
        {
			Character.Move( worldDeltaPosition, false );
        }
        else
            Character.Move(Vector3.zero, false);

		// Pull character towards agent
		//if( worldDeltaPosition.magnitude > agent.radius )
		//	m_Transform.position = agent.nextPosition - 0.9f * worldDeltaPosition;
		// Pull agent towards character
		if( worldDeltaPosition.magnitude > Agent.radius )
			Agent.nextPosition = m_Transform.position + 0.9f * worldDeltaPosition;
    }

    public bool MoveToTarget(Vector3 target)
    {
        this.Target = null;
        return Agent.SetDestination(target);
    }

    public void SetTarget(Transform target)
    {
        if (target != null)
        {
            if (target == this.Target)
            {
                Agent.Resume();
            }
            else
            {
                Agent.ResetPath();
            }
        }
        else
        {
            Agent.Stop();
        }
        this.Target = target;
    }

    public void Stop()
    {
        Agent.Stop();
    }

    public void OnDrawGizmos()
    {
        if (Target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Target.position, 0.5f);
        }
    }
}

