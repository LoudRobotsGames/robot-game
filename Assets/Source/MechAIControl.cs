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
    
    private void Start()
    {
        // get the components on the object we need ( should not be null due to require component so no need to check )
        agent = GetComponentInChildren<NavMeshAgent>();
        character = GetComponent<MechLocomotion>();

	    agent.updateRotation = false;
	    agent.updatePosition = false;
    }

    private void Update()
    {
        if (target != null)
            agent.SetDestination(target.position);

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            character.Move(agent.desiredVelocity, false);
            agent.velocity = character.Velocity;
            agent.Warp(character.transform.position);
        }
        else
            character.Move(Vector3.zero, false);
    }


    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}

