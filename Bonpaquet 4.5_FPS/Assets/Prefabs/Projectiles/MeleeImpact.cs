using UnityEngine;
using System.Collections;

public class MeleeImpact : Projectile {
	
	private Vector3 m_StickPosition= Vector3.zero;
	
	void Start()
	{
		this.m_StickPosition= this.transform.localPosition;
	}
	
	public override void Move ()
	{
		this.transform.localPosition=this.m_StickPosition;
		this.rigidbody.velocity=Vector3.zero;
	}
}
