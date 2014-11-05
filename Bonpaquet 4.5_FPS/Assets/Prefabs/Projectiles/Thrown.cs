using UnityEngine;
using System.Collections;

public class Thrown : Projectile {
	
	public float m_InitialVelocity=5;
	public float m_InitialSpinVelocity=0;
	public bool m_Stick = true;
	
	void Start()
	{
		this.rigidbody.AddRelativeForce(new Vector3(0,this.m_InitialVelocity,0));
		this.rigidbody.AddRelativeTorque(new Vector3(0,this.m_InitialSpinVelocity,0));
	}
	
	public override void Move ()
	{
	}
	
	public override void OnDamage (Collision p_Collision)
	{
		NetworkViewID vid= this.networkView.viewID;
		if(this.m_Stick)
		{
			this.transform.parent = p_Collision.gameObject.transform;
			EV.gameManager.GUIMessage("Stuck on "+this.transform.parent.name+" at "+this.transform.localPosition.x+"."+this.transform.localPosition.y+"."+this.transform.localPosition.z,Color.white);
			vid = this.transform.parent.networkView.viewID;
		}
		this.networkView.RPC("RPC_Stick",RPCMode.All,vid,this.transform.localPosition,this.transform.localRotation,(int)Random.value*50);
	}
	[RPC]
	public void RPC_Stick(NetworkViewID p_ID, Vector3 p_LocalPos,Quaternion p_LocalRot,int p_Quality)
	{
		if(this.m_Stick)
		{
			this.networkView.observed=null;
			this.transform.parent=NetworkView.Find(p_ID).transform;
			this.transform.localPosition=p_LocalPos;
			this.transform.localRotation=p_LocalRot;
			Destroy(this.rigidbody);
			
		}
		
		this.tag="Pickup";
		LooseAmmo la = (LooseAmmo)this.GetComponent(typeof(LooseAmmo));
		if(la!=null)
		{
			la.m_Bullets=new Stack((byte)p_Quality);
		}
		Destroy(this);
	}
}
