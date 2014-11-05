using UnityEngine;
using System.Collections;

public class Structure : Integrity 
{
	public override void OnDeath ()
	{
		Network.Destroy(this.gameObject);
	}
}
