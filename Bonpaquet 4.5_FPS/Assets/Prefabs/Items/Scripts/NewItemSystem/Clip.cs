using UnityEngine;
using System.Collections;

public class Clip : Item 
{
	public Stack m_Bullets; //Qualité des balles utilisés.
	public bool m_StartFull = false; //Si il chargeur commence plein.
	public int m_MagSize = 8; //Maximim de munitions dans le chargeur.
	public EV.AmmoType m_AmmoType; //Type de munitions utilisées.
	
	public override void Start () 
	{
		if(this.m_Bullets==null)
		{
			this.m_Bullets=new Stack();
		}
		this.m_PossibleActionNames.Add("Reload Main");
		this.m_PossibleActions.Add(()=> this.Reload(0));
		this.m_PossibleActionNames.Add("Reload Off-Hand");
		this.m_PossibleActions.Add(()=> this.Reload(1));
		this.m_PossibleActionNames.Add("Unload");
		this.m_PossibleActions.Add(()=> this.Unload());
		this.m_PossibleActionNames.Add("Fill (Unidentified)");
		this.m_PossibleActions.Add(()=> this.Fill(EV.ItemQuality.Junk));
		this.m_PossibleActionNames.Add("Fill (Poor)");
		this.m_PossibleActions.Add(()=> this.Fill(EV.ItemQuality.Common));
		this.m_PossibleActionNames.Add("Fill (Standard)");
		this.m_PossibleActions.Add(()=> this.Fill(EV.ItemQuality.Uncommon));
		this.m_PossibleActionNames.Add("Fill (High-Quality)");
		this.m_PossibleActions.Add(()=> this.Fill(EV.ItemQuality.Rare));
		
		if (this.m_StartFull&&Network.isServer)
		{
			for (int i = 0 ; i<m_MagSize;i++)
			{
				this.InsertBullet((byte)(Random.value*50+40),EV.ItemQuality.Junk);
			}
		}
		base.Start();
	}
	
	public bool InsertBullet(byte p_Quality, EV.ItemQuality p_KnownQuality)
	{
		bool success = true;
		if(this.m_Bullets.Count < this.m_MagSize)
		{
			this.networkView.RPC("RPC_AddBullet",RPCMode.All,(int)p_Quality,(int)p_KnownQuality);
		}
		else
		{
			success = false;
		}
		return success;
	}
	
	public byte PopBullet()
	{
		byte bullet = 101;	
		if(this.m_Bullets.Count!=0)
		{
			bullet = (byte)this.m_Bullets.Peek();
			this.networkView.RPC("RPC_UseBullet",RPCMode.All);
		}
		return bullet;
	}
	
	//Menu Called
	void Reload(int p_hand)
	{
		Item concernedItem = this.m_Master.m_HeldInHands[p_hand];
		if(concernedItem!=null)
		{
			if(concernedItem is FireArm_Clip)
			{
				((FireArm_Clip)concernedItem).Reload(this);
			}
			else
			{
				EV.gameManager.GUIMessage("The "+concernedItem.m_ItemName+" do not use clips.",Color.red);
			}
		}
		else
		{
			if(p_hand==0)
			{
				EV.gameManager.GUIMessage("You main hand is empty.",Color.red);
			}
			else
			{
				EV.gameManager.GUIMessage("You offhand hand is empty.",Color.red);
			}
		}
	}
	/// <summary>
	/// This is a Menu Action.The action will unload each bullet in the clip and give it to the player.
	/// </summary>
	void Unload()
	{
		if(this.m_Bullets.Count>0)
		{
			this.m_Master.StartAction(0.1f,"Unload "+this.m_ItemName,() => this.Task_Unload(),false);
			this.m_Master.m_RepeatAction=true;
		}
		else
		{
			EV.gameManager.GUIMessage(this.m_ItemName+" is already Unloaded.",Color.yellow);
		}
	}
	
	/// <summary>
	/// Finished Task. Will unload each bullet in the clip and give it to the player.
	/// </summary>
	void Task_Unload()
	{
		if(this.m_Bullets.Count>0)
		{
			this.m_Master.TakeBullet(this.m_AmmoType,(byte)this.m_Bullets.Peek());
			this.networkView.RPC("RPC_UseBullet",RPCMode.All);
		}
		else
		{
			this.m_Master.InteruptAction();
			EV.gameManager.GUIMessage(this.m_ItemName+" is Unloaded.",Color.yellow);
		}
	}
	
	/// <summary>
	/// This is a Menu Action. This action fills the clip with a specified quality taken from the player's bullets Supplies.
	/// </summary>
	/// <param name='p_Quality'>
	/// The quality of bullets to be used.
	/// </param>
	void Fill(EV.ItemQuality p_Quality)
	{
		if(this.m_Bullets.Count<this.m_MagSize)
		{
			int quantityLeft = (((PlayerMotor.BulletSupplies)this.m_Master.m_BulletSupplies[this.m_AmmoType]).supplies[p_Quality]as Stack).Count;
			if(quantityLeft>0)
			{
				this.m_Master.StartAction(1.5f/this.m_Master.getBonusMultiplier(Bonus.BonusType.FillClipSpeed),"Loading "+this.m_ItemName+" with "+EV.QualityBulletName(p_Quality)+" bullets.",() => this.Task_Fill(p_Quality),true);
				this.m_Master.m_RepeatAction= true;
			}
			else
			{
				EV.gameManager.GUIMessage("You have no more "+EV.QualityBulletName(p_Quality)+" bullets.",Color.yellow);
			}
		}
		else
		{
			EV.gameManager.GUIMessage("This clip is already full.",Color.yellow);
		}
	}
	
	void Task_Fill(EV.ItemQuality p_Quality)
	{
		Stack inputStack = ((Stack)((PlayerMotor.BulletSupplies)this.m_Master.m_BulletSupplies[this.m_AmmoType]).supplies[p_Quality]);
		this.InsertBullet((byte)inputStack.Pop(),p_Quality);
		if(inputStack.Count<=0||this.m_Bullets.Count>=this.m_MagSize)
		{
			if(this.m_Bullets.Count>=this.m_MagSize)
			{
				EV.gameManager.GUIMessage("Filled "+this.m_ItemName+".",Color.yellow);
			}
			else
			{
				EV.gameManager.GUIMessage("You have no more "+EV.QualityBulletName(p_Quality)+" bullets.",Color.yellow);
			}
			this.m_Master.InteruptAction();
		}
	}
	
	/// <summary>
	/// Updates this.ItemName with a name reflecting actual object's characteristics.
	/// </summary>
	public override void Rename ()
	{
		string temp = "";
		if(this.m_Bullets.Count==0)
		{
			temp = "Empty ";
			this.m_ItemQuality=EV.ItemQuality.Junk;
		}
		else
		{
			temp=EV.QualityBulletName(this.m_ItemQuality)+" ";
		}
		
		temp += this.m_AmmoType.ToString()+" Clip";
		if (this.m_Bullets.Count>0)
		{
			temp+=" ("+this.m_Bullets.Count+")";
		}
		this.m_ItemName=temp;
	}
	
	[RPC]
	void RPC_UseBullet()
	{
		this.m_Bullets.Pop();
		this.Rename();
	}
	/// <summary>
	/// Add a bullet to the clip.
	/// </summary>
	/// <param name='p_Quality'>
	/// Quality of the bullet (byte).
	/// </param>
	/// <param name='p_KnownQuality'>
	/// Ordinal value of the EV.ItemQuality of the bullet. This should only be >0 if the bullet is identified.
	/// </param>
	[RPC]
	void RPC_AddBullet(int p_Quality,int p_KnownQuality)
	{
		EV.ItemQuality quality = (EV.ItemQuality)p_KnownQuality;
		if(this.m_Bullets==null)
		{
			this.m_Bullets=new Stack();
		}
		if(this.m_Bullets.Count==0||this.m_ItemQuality==quality)
		{
			this.m_ItemQuality=quality;
		}
		else
		{
			this.m_ItemQuality=EV.ItemQuality.Junk;
		}
		if(this.m_Bullets==null)
		{
			this.m_Bullets=new Stack();
		}
		this.m_Bullets.Push((byte)p_Quality);
		this.Rename();
	}
}
