using UnityEngine;
using System;
using System.Collections.Generic;

public class Item : MonoBehaviour 
{
	private Observable m_Observable = new Observable();
	[SerializeField]
	private string m_ItemName = ""; //Nom d'affichage de l'objet.
	private string m_BaseName = "";// Nom de base de l'objet.
	[SerializeField]
	private EV.ItemQuality m_ItemQuality = EV.ItemQuality.Junk; //Qualité de l'objet.
	[SerializeField]
	private Sprite m_ItemImage;//Image pour l'affichage de l'objet.
	[SerializeField]
	private bool m_IsStackable = false;//Si l'objet peut augmenter sa quantité.
	[SerializeField]
	private int m_StackableID = 0; //Identifiant d'empilable, deux objets ayant le même Stackable ID et étant isStackable seront empilés.
	[SerializeField]
	private int m_ItemQuantity = 1; //Nombre d'exemplaires.
	protected List<string> m_PossibleActionNames = new List<string>(); //Liste des noms des actions possibles sur cet objet.
	protected List<Action> m_PossibleActions = new List<Action>(); //Délégués des actions possibles sur cet objet.
	[SerializeField]
	private int m_ItemWeight = 0; //Poids de l'objet.
	protected PlayerMotor m_Master; //Propriétaire de l'objet.
	[SerializeField]
	protected AudioClip[] m_Sounds; //Sons disponibles pour l'objet.
	
	virtual public void Start()
	{
		this.m_BaseName=this.FullName;
		this.m_PossibleActionNames.Add("Drop");
		this.m_PossibleActions.Add(()=> this.Drop());
		this.Rename();
	}

	public int Quantity
	{
		get{return this.m_ItemQuantity;}
		set
		{
			this.m_ItemQuantity=value;
			this.m_Observable.notify(ObserverMessages.ItemUpdated,this);
		}
	}
	public EV.ItemQuality Quality
	{
		get{return this.m_ItemQuality;}
		set
		{
			this.m_ItemQuality=value;
			this.m_Observable.notify(ObserverMessages.ItemUpdated,this);
		}
	}
	public string FullName
	{
		get{return this.m_ItemName;}
		protected set
		{
			this.m_ItemName=value;
			this.m_Observable.notify(ObserverMessages.ItemUpdated,this);
		}
	}

	public string BaseName
	{
		get{return this.m_BaseName;}
	}
	public bool IsStackable
	{
		get{return this.m_IsStackable;}
	}
	public int StackableID
	{
		get{return this.m_StackableID;}
	}

	public List<string> ActionNames
	{
		get{return this.m_PossibleActionNames;}
	}
	public PlayerMotor Master
	{
		get{return this.m_Master;}
		set{this.m_Master=value;}
	}
	public int Weight
	{
		get{return this.m_ItemWeight;}
	}
	public Sprite Image
	{
		get{return this.m_ItemImage;}
	}

	public Observable getObservable()
	{
		return this.m_Observable;
	}

	/// <summary>
	/// Active le délégué avec le numéro sélectionné.
	/// </summary>
	/// <param name='p_ActionNo'>
	/// P_ action no.
	/// </param>
	public void UseAction(byte p_ActionNo)
	{
		this.m_PossibleActions[p_ActionNo]();
	}
	/// <summary>
	/// Se fait obtenir par p_Master.
	/// </summary>
	/// <param name='p_Master'>
	/// P_ master.
	/// </param>
	public virtual void Pickup(PlayerMotor p_Master)
	{
		networkView.RPC("ChangeActive",RPCMode.AllBuffered,false,Vector3.zero,Quaternion.identity);
		this.m_Master = p_Master;
	}
	/// <summary>
	/// Supprime de l'inventaire.
	/// </summary>
	public void RemoveFromInventory()
	{
		this.m_Master.m_Inventory.removeItem(this);
	}
	/// <summary>
	/// Laisse tomber cet objet par terre. Activable par le menu.
	/// </summary>
	public virtual void Drop ()
	{
		Equippable equippable = this as Equippable;
		if(equippable !=null)
		{
			this.m_Master.Unequip(equippable);
		}
		networkView.RPC("ChangeActive",RPCMode.AllBuffered,true,m_Master.transform.position+new Vector3(0,1,0),m_Master.transform.rotation);
		this.RemoveFromInventory();
	}
	/// <summary>
	/// Renomme proprement le nom d'affichage de cet objet.
	/// </summary>
	public virtual void Rename()
	{
		if(this.m_IsStackable)
		{
			this.FullName=this.m_BaseName+"("+this.m_ItemQuantity+")";
		}
	}
	/// <summary>
	/// Change l'état de l'objet via le réseau.
	/// Un Objet inactif est invisible et n'a aucune collision.
	/// Si l'on réactive un objet inactif, il et propulsé.
	/// </summary>
	/// <param name='p_active'>
	/// P_active. Si l'état de l'objet passe a actif ou inactif.
	/// </param>
	/// <param name='spawnPos'>
	/// Spawn position.
	/// </param>
	/// <param name='spawnRot'>
	/// Spawn rot.
	/// </param>
	[RPC]
	void ChangeActive(bool p_active,Vector3 spawnPos,Quaternion spawnRot)
	{
		this.gameObject.renderer.enabled=p_active;
		Component[] colliders = this.GetComponents(typeof(Collider));
		foreach(Collider c in colliders)
		{
			c.enabled=p_active;
		}
		if(this.gameObject.rigidbody==null)
		{
			this.gameObject.AddComponent(typeof(Rigidbody));
		}
		this.gameObject.rigidbody.isKinematic=!p_active;
		this.gameObject.rigidbody.detectCollisions=p_active;
		
		if(p_active)
		{
			this.tag="Pickup";
			this.transform.position=spawnPos;
			this.transform.rotation=spawnRot;
			this.transform.parent=null;
			this.rigidbody.AddRelativeForce(new Vector3 (0,150,-100));
		}
		else
		{
			this.tag="";
		}
	}
	/// <summary>
	/// Change la quantité de p_Quantity via le réseau.
	/// </summary>
	/// <param name='Quantity'>
	/// Quantity.
	/// </param>
	[RPC]
	void ChangeQuantity(int p_Quantity)
	{
		this.m_ItemQuantity+=p_Quantity;
		this.Rename();
	}
}
