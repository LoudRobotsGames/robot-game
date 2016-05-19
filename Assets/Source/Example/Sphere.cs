using UnityEngine;
using System.Collections;
using Svelto.Communication.SignalChain;

public class Sphere : MonoBehaviour, IChainListener
{
	public void Listen<T>(T message)
	{
		if (message is BetterEvent)
		{
			Renderer r = GetComponent<Renderer>();
			r.material.color = (message as BetterEvent).color;
			//this.transform.GetComponent<Renderer>().material.color = (message as BetterEvent).color;
		}
	}
}
