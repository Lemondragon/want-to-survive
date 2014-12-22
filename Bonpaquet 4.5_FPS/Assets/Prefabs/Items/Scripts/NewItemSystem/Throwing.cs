using UnityEngine;
using System.Collections;

public class Throwing : Weapon {
	[SerializeField]
	private int m_ThrownID;

	public override void Trigger ()
	{
		if(this.Quantity > 0)
		{
			GameObject clone = (GameObject)Network.Instantiate(EV.gameManager.m_ThrowingPrefabs[m_ThrownID],this.m_Tip.position,this.transform.rotation,0);
			clone.transform.rotation=this.transform.rotation*Quaternion.Euler(new Vector3(0,0,(float)(Random.value-0.5)*(this.m_Master.Fear)));
			clone.tag="";
			Projectile ss = (Projectile)clone.GetComponent(typeof(Projectile));
			ss.m_Origin=this;
			ss.enabled=true;
			ss.m_BleedDamage=this.m_Power*this.m_BleedRatio;
			ss.m_CommotionDamage=(1-this.m_BleedRatio)*this.m_Power;
			this.m_Master.InteruptAction();
			this.Quantity--;
			this.Rename();
			if(this.Quantity<1)
			{
				this.Drop();
				Destroy(this.renderer);
				Destroy(this.collider);
				Destroy(this.rigidbody);
				Destroy(this.gameObject,10f);
			}
		}
		else //THIS SHOULD NOT HAPPEN 
		{
			EV.gameManager.GUIMessage(this.FullName+" is no more available.",Color.red);
		}
	}
	
	public override void onHit (GameObject p_Victim,float p_Power)
	{
		Health health = p_Victim.GetComponent<Health>();
		if(health is Structure)
		{
			this.m_Master.gainFocusExp(Focus.Melee,p_Power/2);
		}
		else if (health is Zombie)
		{
			this.m_Master.gainFocusExp(Focus.Melee,p_Power*2);
		}
		else if (health is PlayerMotor)
		{
			this.m_Master.gainFocusExp(Focus.Melee,p_Power);
		}
	}
}
