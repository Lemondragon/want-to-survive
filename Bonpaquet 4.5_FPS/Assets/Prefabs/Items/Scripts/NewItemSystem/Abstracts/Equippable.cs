using UnityEngine;
using System.Collections;

public abstract class Equippable : Item {

	public enum TriggerType {Manual,Automatic,Charged};
	
		
	//Inspector
	[HideInInspector]public int m_HeldByHand = -1;//Main par laquelle l'objet est tenu, -1 signifie qu'il n'est pas tenu.
	public bool m_IsTwoHanded = false;//Si l'arme se porte à deux mains.
	public float m_UseDelay = 5f; //Délai entre chaque utilisation possible (trigger).

	public TriggerType m_TriggerType;
	private float m_NextUse = 0; //Quand la prochaine utilisation sera possible.
	private bool m_Trigger = false; //Si le trigger est activé.
	private float m_Charge = 0f;
	
	// Update is called once per frame
	void Update () 
	{
		if(this.m_Master!=null)
		{
			if(this.m_Master.m_AssociatedPlayer==Network.player)
			{
				//Determinating if is triggered.
				if(this.m_HeldByHand!=-1&&!this.m_Master.m_ShowInventory)
				{
					switch(this.m_TriggerType)
					{
					case TriggerType.Automatic:
						m_Trigger=Input.GetMouseButton(m_HeldByHand);
						break;
					case TriggerType.Manual:
						m_Trigger=Input.GetMouseButtonDown(m_HeldByHand);
						break;
					case TriggerType.Charged:
						if(Input.GetMouseButtonDown(m_HeldByHand))
						{
							this.OnStartCharge();
						}
						if(Input.GetMouseButton(m_HeldByHand))
						{
							this.m_Charge+=Time.deltaTime;
						}
						if(Input.GetMouseButtonUp(m_HeldByHand))
						{
							m_Trigger=this.m_Charge>=this.m_UseDelay;
							if(!m_Trigger)
							{
								this.m_Master.networkView.RPC("PlayHandAction",RPCMode.All,this.m_HeldByHand,"hand_Grab");
								if(this.m_IsTwoHanded){this.m_Master.networkView.RPC("PlayHandAction",RPCMode.All,1,"hand_UnGrab");}
							}
							this.m_Charge=0;
						}
						break;
					}
				}
				//On acceptable trigger.
				if(this.m_NextUse<Time.time)
				{
					if(this.m_Trigger)
					{
						float divider = 1;
						if(this is Melee)
						{
							divider=this.m_Master.getBonusMultiplier(Bonus.BonusType.MeleeSpeed);
						}
						this.m_NextUse=Time.time+this.m_UseDelay/divider;
						
						this.m_Trigger=false;
						this.Trigger();
					}
					else
					{
						this.OnReady();
					}
				}
			}
		}
	}
	override public void Start ()
	{	
		this.m_PossibleActionNames.Add("Equip");
		this.m_PossibleActions.Add(()=> this.Equip());
		base.Start ();
	}
	
	public virtual void Equip()
	{
		this.m_Master.Equip(this);
		this.m_Master.networkView.RPC("PlayHandAction",RPCMode.All,this.m_HeldByHand,"hand_Grab");
	}
	
	/// <summary>
	/// Fait l'action par défaut de l'équippable.
	/// </summary>
	public abstract void Trigger();
	/// <summary>
	/// Se déclenche a toutes les frames que l'objet peut être activé mais ne l'est pas.
	/// </summary>
	public virtual void OnReady(){}
	/// <summary>
	/// Se déclenche lorsqu'on commence une charge.
	/// </summary>
	public virtual void OnStartCharge(){}
	//Remote Procedure Calls
	/// <summary>
	/// Joue le son correspondant à l'index p_Sound via le réseau.
	/// </summary>
	/// <param name='p_Sound'>
	/// Sound.
	/// </param>
	[RPC]
	void RPC_PlaySound(int p_Sound)
	{
		this.audio.PlayOneShot(this.m_Sounds[p_Sound]);
	}
	/// <summary>
	/// Équippe cet équippable via le réseau.
	/// </summary>
	/// <param name='p_Parent_ID'>
	/// P_ parent_ I.
	/// </param>
	[RPC]
	public virtual void RPC_Equip(NetworkViewID p_Parent_ID)
	{
		this.transform.parent = NetworkView.Find(p_Parent_ID).transform;
		this.transform.position=this.transform.parent.position;
		this.transform.rotation=this.transform.parent.rotation;
		this.renderer.enabled=true;
		//this.rigidbody.detectCollisions=this.h_type==HeldableType.Melee;
	}
	/// <summary>
	/// Déséquippe cet équippable.
	/// </summary>
	[RPC]
	void RPC_Unequip()
	{
		this.rigidbody.detectCollisions=false;
		this.renderer.enabled=false;
		this.transform.parent=null;
	}
}
