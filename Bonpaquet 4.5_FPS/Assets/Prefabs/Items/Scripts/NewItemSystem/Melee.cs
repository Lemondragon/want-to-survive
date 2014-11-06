using UnityEngine;
using System.Collections;

public class Melee : Weapon 
{
	private bool m_isDangerous = false;
	public enum MeleeStyle {Stab,Slash};
	public MeleeStyle m_MeleeStyle = MeleeStyle.Stab; 

	public override void AfterUpdate ()
	{
		if(this.m_isDangerous)
		{
			this.isDangerous();
		}
	}
	

	public override void OnReady ()
	{
		this.m_isDangerous = false;
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
			this.m_isDangerous=true;
		}
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

	private void isDangerous()
	{
		Debug.Log("SLICE");
		RaycastHit hitInfo;
		if(Physics.Raycast(this.transform.position,this.transform.TransformDirection(Vector3.up),out hitInfo,Vector3.Distance(this.transform.position,this.m_Tip.position)))
		{
			Health colHealth = hitInfo.collider.GetComponent<Health>();
			if(colHealth!=null)
			{
				float multiplier = this.m_Master.getBonusMultiplier(Bonus.BonusType.MeleeDamage)-1;
				if(this.m_IsTwoHanded){multiplier+=this.m_Master.getBonusMultiplier(Bonus.BonusType.BothHandsMeleeDamage)-1;}

				float bleedDamage = this.m_Power*this.m_BleedRatio*(this.m_Master.getBonusMultiplier(Bonus.BonusType.MeleeBleedDamage)+multiplier);
				float commoDamage = (1-this.m_BleedRatio)*this.m_Power*(this.m_Master.getBonusMultiplier(Bonus.BonusType.MeleeCommotionDamage)+multiplier);
				colHealth.networkView.RPC("AddBleed",RPCMode.AllBuffered,bleedDamage,this.networkView.viewID);
				colHealth.networkView.RPC("AddCommotion",RPCMode.AllBuffered,commoDamage,this.networkView.viewID);
				this.onHit(colHealth.gameObject,bleedDamage+commoDamage);
				this.m_isDangerous=false;
				Debug.Log("HIT "+colHealth.name);
			}
		}
	}
}
