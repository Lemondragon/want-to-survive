using UnityEngine;
using System;
using System.Collections.Generic;

public class Item : MonoBehaviour 
{
	public string m_ItemName = ""; //Nom d'affichage de l'objet.
	[HideInInspector]public string m_BaseName = "";// Nom de base de l'objet.
	public EV.ItemQuality m_ItemQuality = EV.ItemQuality.Junk; //Qualité de l'objet.
	public Texture m_ItemImage;//Image pour l'affichage de l'objet.
	public bool m_IsStackable = false;//Si l'objet peut augmenter sa quantité.
	public int m_StackableID = 0; //Identifiant d'empilable, deux objets ayant le même Stackable ID et étant isStackable seront empilés.
	public int m_ItemQuantity = 1; //Nombre d'exemplaires.
	[HideInInspector]public List<string> m_PossibleActionNames; //Liste des noms des actions possibles sur cet objet.
	[HideInInspector]public List<Action> m_PossibleActions = new List<Action>(); //Délégués des actions possibles sur cet objet.
	public int m_ItemWeight = 0; //Poids de l'objet.
	[HideInInspector]public PlayerMotor m_Master; //Propriétaire de l'objet.
	[HideInInspector]public int m_PlaceInMasterInventory=-1; //Index de la position dans l'inventaire du propriétaire.-1 signifie qu'il n'a pas de propriétaire.
	public AudioClip[] m_Sounds; //Sons disponibles pour l'objet.
	
	virtual public void Start()
	{
		this.m_BaseName=this.m_ItemName;
		this.m_PossibleActionNames.Add("Drop");
		this.m_PossibleActions.Add(()=> this.Drop());
		/*if(this.GetComponent("Heldable")!=null)
		{
			this.possibleActionsNames.Add("Equip");
			this.possibleActions.Add(()=> this.Equip());
			this.myHeldable=(Heldable)this.GetComponent("Heldable");
			if(this.myHeldable.h_type==Heldable.HeldableType.FireArm)
			{
				this.possibleActionsNames.Add("Remove Clip");
				this.possibleActions.Add(()=> myHeldable.RemoveClip());
			}
		}*/
		this.Rename();
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
		if(this.m_PlaceInMasterInventory>=0)
		{
			this.m_Master.m_Inventory[this.m_PlaceInMasterInventory]=null;
			this.m_PlaceInMasterInventory=-1;
			if(m_Master.m_SelectedItem==this.gameObject)
			{
				this.m_Master.m_SelectedItem=null;
				this.m_Master.m_ContextualMenuPos.x=-1;
			}
			for(int i = 0;i<4;i++)
			{
				PlayerMotor.QuickSlot qs = this.m_Master.m_QuickSlots[i];
				if(qs!=null)
				{
					if(qs.m_Item.Equals(this))
					{
						this.m_Master.m_QuickSlots[i]=null;
					}
				}
			}
		}
		else
		{
			Debug.LogWarning("Vous avez tenté de retirer de l'inventaire un objet ayant un nombre négatif de position dans l'inventaire, il n'est probablement pas dans un inventaire.");
		}
	}
	/// <summary>
	/// Laisse tomber cet objet par terre. Activable par le menu.
	/// </summary>
	public virtual void Drop ()
	{
		/*if(this.myHeldable!=null)
		{
			if(this.myHeldable.heldByHand!=-1)
			{
				this.master.Unequip(null);
				this.myHeldable.heldByHand=-1;
			}
		}*/
		Equippable equippable = this as Equippable;
		if(equippable !=null)
		{
			this.m_Master.Unequip(equippable);
		}
		this.RemoveFromInventory();
		networkView.RPC("ChangeActive",RPCMode.AllBuffered,true,m_Master.transform.position+new Vector3(0,1,0),m_Master.transform.rotation);
		this.m_Master=null;
	}
	/// <summary>
	/// Renomme proprement le nom d'affichage de cet objet.
	/// </summary>
	public virtual void Rename()
	{
		if(this.m_IsStackable)
		{
			this.m_ItemName=this.m_BaseName+"("+this.m_ItemQuantity+")";
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
