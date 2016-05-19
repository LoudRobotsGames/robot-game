using UnityEngine;
using System.Collections;
using System;

public class Health : MonoBehaviour, IDamageable
{
    public int MaxHealth;
    [SerializeField]
    private int CurrentHealth;

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
    }

    // Use this for initialization
    void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
