using UnityEngine;
using System.Collections;

public abstract class FireArm : Weapon 
{
	public EV.AmmoType m_AmmoType; //Type de munition utilisé.
	public GameObject m_FxShotBurst; //Effet visuel de tir.
	public float m_Angle = 5; //Angle de tir variable (Précision, 0 étant une précision parfaite).
	public float m_ReloadSpeed = 5;//Temps de recharche (le temps de décharge est la moitié de lui.).
	public int m_BulletPerShot = 1;//Nombre de balles propulsés par coup (Par munition, comme dans le cas d'un fusil à pompe).
	public float m_GeneratedThreatOnFire = 0f;
	
	public void Fire(byte p_Bullet)
	{
		//Shooting
		this.m_Master.gainFocusExp(Focus.Firearms,this.m_Power/5);
		for(int i = 0;i<this.m_BulletPerShot;i++)
		{
			Network.Instantiate(this.m_FxShotBurst,this.m_Tip.position,this.transform.rotation,0);
			GameObject clone = (GameObject)Network.Instantiate(this.m_ImpactPrefab,this.m_Tip.position,this.transform.rotation,0);
			clone.transform.rotation=this.transform.rotation*Quaternion.Euler(new Vector3(0,0,(float)(Random.value-0.5)*(this.m_Angle+this.m_Master.m_Fear)));
			Projectile ss = (Projectile)clone.GetComponent(typeof(Projectile));
			ss.m_Origin=this;
			float multiplier = this.m_Master.getBonusMultiplier(Bonus.BonusType.FireArmDamage);
			switch(this.m_AmmoType)
			{
			case EV.AmmoType.Arrow:
				multiplier+=this.m_Master.getBonusMultiplier(Bonus.BonusType.ArrowDamage)-1;break;
			case EV.AmmoType.Pistol:
				multiplier+=this.m_Master.getBonusMultiplier(Bonus.BonusType.PistolDamage)-1;break;
			case EV.AmmoType.Rifle:
				multiplier+=this.m_Master.getBonusMultiplier(Bonus.BonusType.RifleDamage)-1;break;
			case EV.AmmoType.Shotgun:
				multiplier+=this.m_Master.getBonusMultiplier(Bonus.BonusType.ShotgunDamage)-1;break;
			}
			ss.m_BleedDamage=this.m_Power*this.m_BleedRatio*p_Bullet/75*multiplier;
			ss.m_CommotionDamage=(1-this.m_BleedRatio)*this.m_Power*p_Bullet/75*multiplier;
			this.m_Master.InteruptAction();
		}
		this.m_Master.networkView.RPC("PlayHandAction",RPCMode.All,this.m_HeldByHand,"hand_Recoil");
		if(this.m_IsTwoHanded)
		{
			this.m_Master.networkView.RPC("PlayHandAction",RPCMode.All,1,"hand_OffRecoil");
		}
		if(m_GeneratedThreatOnFire>0)
		{
			this.m_Master.networkView.RPC("RPC_GenerateThreat",RPCMode.All,m_GeneratedThreatOnFire);
		}
		networkView.RPC("RPC_PlaySound",RPCMode.All,0);
	}
	
	override public void Equip()
	{
		if(this.m_IsTwoHanded){this.m_Master.networkView.RPC("PlayHandAction",RPCMode.All,1,"hand_OffGrab");}
		base.Equip();
	}
	
	public override void onHit (GameObject p_Victim,float p_Power)
	{
		Health health = p_Victim.GetComponent<Health>();
		if(health is Structure)
		{
			this.m_Master.gainFocusExp(Focus.Firearms,p_Power/2);
		}
		else if (health is Zombie)
		{
			this.m_Master.gainFocusExp(Focus.Firearms,p_Power*2);
		}
		else if (health is PlayerMotor)
		{
			this.m_Master.gainFocusExp(Focus.Firearms,p_Power);
		}
	}
}
