using UnityEngine;
using System.Collections;

public abstract class Projectile : MonoBehaviour 

{
	[HideInInspector]public float m_BleedDamage = 30f;
	[HideInInspector]public float m_CommotionDamage = 0f;
	[HideInInspector]public Weapon m_Origin = null;

	void Update () 
	{
		this.Move();
	}
	
	public abstract void Move();
	
	void OnCollisionEnter(Collision collisionInfo)
	{
		if(this.networkView.isMine)
		{
			Health colHealth = collisionInfo.gameObject.GetComponent<Health>();
			if(colHealth!=null)
			{
				colHealth.networkView.RPC("AddBleed",RPCMode.AllBuffered,this.m_BleedDamage,this.m_Origin.networkView.viewID);
				colHealth.networkView.RPC("AddCommotion",RPCMode.AllBuffered,this.m_CommotionDamage,this.m_Origin.networkView.viewID);
			}
			this.OnDamage(collisionInfo);
		}
	}
	public virtual void OnDamage(Collision p_Collision)
	{
		if(this.m_Origin!=null)
		{
			this.m_Origin.onHit(p_Collision.gameObject,this.m_CommotionDamage+this.m_BleedDamage);
		}
		Network.Destroy(this.gameObject);
	}
}
