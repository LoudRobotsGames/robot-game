using UnityEngine;
using System.Collections;
using System;

public class Health : MonoBehaviour, IDamageable
{
    public int MaxHealth;
    [SerializeField]
    private int CurrentHealth;

    public void TakeDamage(int damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    // Use this for initialization
    void Start()
	{
        CurrentHealth = MaxHealth;
	}

	// Update is called once per frame
	void Update()
	{

	}
}
