using UnityEngine;
using System.Collections;

public class Bullet : Projectile 
{
	public float m_Speed = 1.0f;
	float m_TravelDistance = 0f;
	public float m_Range = 0f;
	
	public override void Move ()
	{
		this.rigidbody.velocity=transform.up*m_Speed;
		this.m_TravelDistance += m_Speed * Time.deltaTime;
		if(this.networkView.isMine&&m_TravelDistance >= m_Range)
		{
			Network.Destroy(this.gameObject);
		}
	}
}
