using UnityEngine;
using System.Collections;

public class DeathTimer : MonoBehaviour {
	
	float DieAt;
	public float Life = 1f;
	// Use this for initialization
	void Start () 
	{
		this.DieAt=Time.time+Life;
	}
	
	// Update is called once per frame
	void Update () {
	if(Time.time>=DieAt)
		{
			Destroy(this.gameObject);
		}
	}
}
