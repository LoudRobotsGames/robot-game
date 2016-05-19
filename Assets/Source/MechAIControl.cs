using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(MechLocomotion))]
[RequireComponent(typeof(NavMeshAgent))]
public class MechAIControl : MonoBehaviour
{ 
    public NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
    public MechLocomotion character { get; private set; } // the character we are controlling
    public Transform target;                                    // target to aim for

	private Transform m_Transform;
	private Vector2 m_SmoothDeltaPosition = Vector2.zero;
	private Vector2 m_Velocity = Vector2.zero;

    private void Start()
    {
        // get the components on the object we need ( should not be null due to require component so no need to check )
        agent = GetComponentInChildren<NavMeshAgent>();
        character = GetComponent<MechLocomotion>();

	    agent.updatePosition = false;
		agent.updateRotation = false;
		m_Transform = transform;
    }

    private void Update()
    {
        if (target != null)
            agent.SetDestination(target.position);

		Vector3 worldDeltaPosition = agent.nextPosition - m_Transform.position;
       
        if (agent.remainingDistance > agent.stoppingDistance)
        {
			character.Move( worldDeltaPosition, false );
        }
        else
            character.Move(Vector3.zero, false);

		// Pull character towards agent
		//if( worldDeltaPosition.magnitude > agent.radius )
		//	m_Transform.position = agent.nextPosition - 0.9f * worldDeltaPosition;
		// Pull agent towards character
		if( worldDeltaPosition.magnitude > agent.radius )
			agent.nextPosition = m_Transform.position + 0.9f * worldDeltaPosition;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}

