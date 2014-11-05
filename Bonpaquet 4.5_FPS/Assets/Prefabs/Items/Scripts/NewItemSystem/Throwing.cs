using UnityEngine;
using System.Collections;

public class Throwing : Weapon {

	public override void Trigger ()
	{
		if(this.m_ItemQuantity > 0)
		{
			GameObject clone = (GameObject)Network.Instantiate(this.m_ImpactPrefab,this.m_Tip.position,this.transform.rotation,0);
			clone.transform.rotation=this.transform.rotation*Quaternion.Euler(new Vector3(0,0,(float)(Random.value-0.5)*(this.m_Master.m_Fear)));
			clone.tag="";
			Projectile ss = (Projectile)clone.GetComponent(typeof(Projectile));
			ss.m_Origin=this;
			ss.enabled=true;
			ss.m_BleedDamage=this.m_Power*this.m_BleedRatio;
			ss.m_CommotionDamage=(1-this.m_BleedRatio)*this.m_Power;
			this.m_Master.InteruptAction();
			this.m_ItemQuantity--;
			this.Rename();
			if(this.m_ItemQuantity<1)
			{
				this.RemoveFromInventory();
				Destroy(this.gameObject);
			}
		}
		else //THIS SHOULD NOT HAPPEN 
		{
			EV.gameManager.GUIMessage(this.m_ItemName+" is no more available.",Color.red);
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
