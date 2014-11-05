using UnityEngine;
using System.Collections;

public class FireArm_Clip : FireArm 
{
	// Sounds: 0-Fire 1-Empty 2-Reload
	[HideInInspector]public Clip m_Clip; //Chargeur utilisé.
	
	public override void Start ()
	{
		base.Start ();
		this.m_PossibleActionNames.Add("Unload");
		this.m_PossibleActions.Add(()=> this.RemoveClip());
	}
	
	public override void Trigger ()
	{
		if(this.m_Clip !=null && this.m_Clip.m_Bullets.Count > 0)
		{
			this.Fire(this.m_Clip.PopBullet());
		}
		else
		{
			EV.gameManager.GUIMessage(this.m_ItemName+" is Empty.",Color.red);
			networkView.RPC("RPC_PlaySound",RPCMode.All,1);
		}
	}

	public bool Reload(Clip p_Clip)
	{
		bool answer = false;
		if(p_Clip.m_AmmoType==this.m_AmmoType)
		{
			this.m_Master.StartAction(this.m_ReloadSpeed/this.m_Master.getBonusMultiplier(Bonus.BonusType.FillClipSpeed),"Reload "+this.m_ItemName,()=>this.Task_Reload(p_Clip),false);
		}
		else
		{
			EV.gameManager.GUIMessage(this.m_ItemName+" cannot use "+p_Clip.m_AmmoType.ToString()+" clips.",Color.red);
		}
		return answer;
	}
	
	public bool Task_Reload (Clip p_Clip)
	{
		bool answer = false;
		if(p_Clip.m_AmmoType==this.m_AmmoType)
		{
			if(this.m_Clip!=null)
			{
				this.m_Master.TakeItem(this.m_Clip);
			}
			this.networkView.RPC("RPC_LinkMagazine",RPCMode.All,p_Clip.networkView.viewID);
			this.networkView.RPC("RPC_PlaySound",RPCMode.All,2);
			p_Clip.RemoveFromInventory();
		}
		else
		{
			EV.gameManager.GUIMessage(this.m_ItemName+" cannot use "+p_Clip.m_AmmoType.ToString()+" clips.",Color.red);
		}
		return answer;
	}
	
	//MenuCall remove clip
	public void RemoveClip()
	{
		if(this.m_Clip!=null)
		{
			this.m_Master.StartAction(this.m_ReloadSpeed/2,"Unload "+this.m_ItemName,() => this.Task_RemoveClip(),false);
		}
		else
		{
			EV.gameManager.GUIMessage(this.m_ItemName+" have no clip to be removed.",Color.yellow);
		}
	}
	public void Task_RemoveClip()
	{
		if(this.m_Clip != null)
		{
			this.m_Master.TakeItem(this.m_Clip);
			this.networkView.RPC("RPC_UnLinkMagazine",RPCMode.All);
		}
		else
		{
			EV.gameManager.GUIMessage(this.m_ItemName+" have no clip to be removed.",Color.yellow);
		}
	}
	[RPC]
	void RPC_LinkMagazine(NetworkViewID p_mag_ID)
	{
		Clip mag = (Clip)NetworkView.Find(p_mag_ID).GetComponent(typeof(Clip));
		this.m_Clip=mag;
	}
	
	[RPC]
	void RPC_UnLinkMagazine()
	{
		this.m_Clip=null;
	}
	
}
