using UnityEngine;
using System.Collections;

[RequireComponent (typeof (NetworkView))]
public abstract class Puppet : MonoBehaviour 
{
	public void Start()
	{
		this.OnStart();
	}
	void Update () 
	{
		if(this.networkView.isMine)
		{
			this.AsMaster();
		}
		else
		{
			this.AsPuppet();
		}
	}
	protected abstract void OnStart();
	protected abstract void AsPuppet();
	protected abstract void AsMaster();
}
