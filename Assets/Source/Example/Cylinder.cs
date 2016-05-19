using UnityEngine;
using System.Collections;
using Svelto.Communication.SignalChain;

public class Cylinder : MonoBehaviour, IChainListener
{
	public void Listen<T>(T message)
	{
		if (message is string && (message as string) == "event")
		{
			Renderer r = GetComponent<Renderer>();
			r.material.color = Color.red;
			//this.transform.GetComponent<Renderer>().material.color = Color.red;
		}
	}
}
