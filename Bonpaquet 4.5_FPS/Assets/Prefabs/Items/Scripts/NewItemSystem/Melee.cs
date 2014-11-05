using UnityEngine;
using System.Collections;

public class Melee : Weapon 
{
	private GameObject m_MeleeImpact; 
	public enum MeleeStyle {Stab,Slash};
	public MeleeStyle m_MeleeStyle = MeleeStyle.Stab; 
	
	public override void OnReady ()
	{
		if(this.m_MeleeImpact!=null)
		{
			Network.Destroy(this.m_MeleeImpact);
			this.m_MeleeImpact=null;
		}
	}
	
	public override void Trigger ()
	{
		if(this.m_IsTwoHanded)
		{
			this.m_Master.networkView.RPC("PlayHandActionScaled",RPCMode.All,0,"hand_DualSlam",2f);
			this.m_Master.networkView.RPC("PlayHandActionScaled",RPCMode.All,1,"hand_DualSlam",2f);
		}
		else
		{
			switch(this.m_MeleeStyle)
			{
			case MeleeStyle.Stab:
				this.m_Master.networkView.RPC("PlayHandAction",RPCMode.All,this.m_HeldByHand,"hand_Stab");
				break;
			case MeleeStyle.Slash:
				this.m_Master.networkView.RPC("PlayHandAction",RPCMode.All,this.m_HeldByHand,"hand_Slash");
				break;
			}
		}
		GameObject clone = (GameObject)Network.Instantiate(this.m_ImpactPrefab,this.m_Tip.position,this.transform.rotation,0);
		clone.transform.parent=this.transform;
		this.m_MeleeImpact=clone;
		Projectile ss = (Projectile)clone.GetComponent(typeof(Projectile));
		ss.m_Origin=this;
		float multiplier = this.m_Master.getBonusMultiplier(Bonus.BonusType.MeleeDamage)-1;
		if(this.m_IsTwoHanded){multiplier+=this.m_Master.getBonusMultiplier(Bonus.BonusType.BothHandsMeleeDamage)-1;}
		
		ss.m_BleedDamage=this.m_Power*this.m_BleedRatio*(this.m_Master.getBonusMultiplier(Bonus.BonusType.MeleeBleedDamage)+multiplier);
		ss.m_CommotionDamage=(1-this.m_BleedRatio)*this.m_Power*(this.m_Master.getBonusMultiplier(Bonus.BonusType.MeleeCommotionDamage)+multiplier);
	}
	
	public override void OnStartCharge ()
	{
		float speed = 1/(this.m_UseDelay/0.22f);
		if(this.m_IsTwoHanded)
		{
			this.m_Master.networkView.RPC("PlayHandActionScaled",RPCMode.All,0,"hand_DualPrep",speed);
			this.m_Master.networkView.RPC("PlayHandActionScaled",RPCMode.All,1,"hand_DualPrep",speed);
		}
		else
		{
			switch(this.m_MeleeStyle)
			{
			case MeleeStyle.Stab:
				this.m_Master.networkView.RPC("PlayHandActionScaled",RPCMode.All,this.m_HeldByHand,"hand_StabPrep",speed);
				break;
			case MeleeStyle.Slash:
				this.m_Master.networkView.RPC("PlayHandActionScaled",RPCMode.All,this.m_HeldByHand,"hand_SlashPrep",speed);
				break;
			}
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
