using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MissileBehaviours.Controller;

/// <summary>
/// Spawns missiles, believe it or not.
/// </summary>
public class FireDreadnought : MonoBehaviour
{
    public GameObject missilePrefab;
    public List<Transform> silos;
    public Transform target;

	void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var item in silos)
            {
                GameObject missile = Instantiate(missilePrefab, item.transform.position, item.transform.rotation) as GameObject;
                missile.GetComponent<MissileController>().Target = target;
            }
        }
	}
}
