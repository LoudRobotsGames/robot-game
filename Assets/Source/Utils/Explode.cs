using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Explode : MonoBehaviour
{
    public GameObject m_Prefab;

    public bool m_ExplodeOnCollision = false;

    public void OnCollisionEnter(Collision collision)
    {
        if (!m_ExplodeOnCollision)
        {
            return;
        }

		TriggerExplosion( collision.contacts[ 0 ].point, collision.contacts[ 0 ].normal );
    }

	public void TriggerExplosion(Vector3 point, Vector3 normal)
	{
		GameObject go = GameObject.Instantiate<GameObject>( m_Prefab );
		go.name = "Explosion";
		go.transform.position = point;
		go.transform.rotation = Quaternion.LookRotation( normal );

	}
}

