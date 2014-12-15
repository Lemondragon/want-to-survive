using UnityEngine;
using System.Collections;

public class FireArm_DirectAmmo : FireArm {
	//Sounds : 0-Fire 1-Empty 2-Reload
	public int m_MaxAmmo = 6; //Balles au maximum dans l'arme.
	public Queue m_Bullets; //Qualité des balles utilisés.
	public override void Start ()
	{
		base.Start ();
		this.m_Bullets = new Queue();
		this.m_PossibleActionNames.Add("Reload (Unidentified)");
		this.m_PossibleActions.Add(()=> this.Reload(EV.ItemQuality.Junk));
		this.m_PossibleActionNames.Add("Reload (Poor)");
		this.m_PossibleActions.Add(()=> this.Reload(EV.ItemQuality.Common));
		this.m_PossibleActionNames.Add("Reload (Standard)");
		this.m_PossibleActions.Add(()=> this.Reload(EV.ItemQuality.Uncommon));
		this.m_PossibleActionNames.Add("Reload (High-Quality)");
		this.m_PossibleActions.Add(()=> this.Reload(EV.ItemQuality.Rare));
		this.m_PossibleActionNames.Add("Unload");
		this.m_PossibleActions.Add(()=> this.Unload());

	}
	
	public override void Trigger ()
	{
		if(this.m_Bullets.Count > 0)
		{
			this.Fire((byte)this.m_Bullets.Peek());
			this.networkView.RPC("RPC_UseBullet",RPCMode.All);
		}
		else
		{
			EV.gameManager.GUIMessage(this.FullName+" is Empty.",Color.red);
			networkView.RPC("RPC_PlaySound",RPCMode.All,1);
		}
	}
	
	//Direct Ammo Reloading
	public void Reload (EV.ItemQuality p_Quality)
	{
		if (this.m_Bullets.Count<this.m_MaxAmmo)
		{
			if(((PlayerMotor.BulletSupplies)this.m_Master.m_BulletSupplies[this.m_AmmoType]).supplies[p_Quality].Count>0)
			{
				this.m_Master.StartAction(this.m_ReloadSpeed,"Reload "+this.FullName+" with "+EV.QualityBulletName(p_Quality)+" bullets.",()=>this.Task_Reload(p_Quality),false);
				this.m_Master.m_RepeatAction=true;
			}
			else
			{
				this.m_Master.InteruptAction();
				EV.gameManager.GUIMessage("You don't have any "+EV.QualityBulletName(p_Quality)+" "+this.m_AmmoType.ToString()+" bullets left.",Color.red);
			}
		}
		else
		{
			this.m_Master.InteruptAction();
			EV.gameManager.GUIMessage("Cannot insert bullet, already full.",Color.red);
		}
	}
	
	public void Task_Reload (EV.ItemQuality p_Quality)
	{
		if (this.m_Bullets.Count<this.m_MaxAmmo)
		{
			PlayerMotor.BulletSupplies concernedSupplies = this.m_Master.m_BulletSupplies[this.m_AmmoType];
			if(concernedSupplies.supplies[p_Quality].Count>0)
			{
				this.networkView.RPC("RPC_AddBullet",RPCMode.All,(int)((byte)concernedSupplies.supplies[p_Quality].Pop()));
				this.networkView.RPC("RPC_PlaySound",RPCMode.All,2);
				if(this.m_Bullets.Count==this.m_MaxAmmo||concernedSupplies.supplies[p_Quality].Count==0)
				{
					this.m_Master.InteruptAction();
				}
			}
			else
			{
				EV.gameManager.GUIMessage("You don't have any "+p_Quality.ToString()+" bullets left.",Color.red);
				this.m_Master.InteruptAction();
			}
		}
		else
		{
			EV.gameManager.GUIMessage("Cannot insert bullet, already full.",Color.red);
			this.m_Master.InteruptAction();
		}
	}
	
	public void Unload ()
	{
		if(this.m_Bullets.Count>0)
		{
			this.m_Master.StartAction(this.m_ReloadSpeed,"Unload "+this.FullName,()=>this.Task_Unload(),false);
			this.m_Master.m_RepeatAction=true;
		}
		else
		{
			EV.gameManager.GUIMessage("Cannot unload: "+this.FullName+" is empty,",Color.red);
		}
	}
	
	public void Task_Unload()
	{
		((PlayerMotor.BulletSupplies)this.m_Master.m_BulletSupplies[this.m_AmmoType]).supplies[EV.ItemQuality.Junk].Push(this.m_Bullets.Peek());
		this.networkView.RPC("RPC_UseBullet",RPCMode.All);
	}
	
	[RPC]
	void RPC_AddBullet(int p_Quality)
	{
		this.m_Bullets.Enqueue((byte)p_Quality);
	}
	
	[RPC]
	void RPC_UseBullet()
	{
		this.m_Bullets.Dequeue();
	}
}
