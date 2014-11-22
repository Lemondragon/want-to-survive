using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Container {

	public List<Item[]> m_Contents = new List<Item[]>();
	private int m_BagLength = 3;
	private int m_BagHeight = 3;
	protected PlayerMotor m_Owner;
	
	public Container(int p_Length,int p_Height, PlayerMotor p_Owner)
	{
		this.m_Owner=p_Owner;
		this.m_BagLength=p_Length;
		this.m_BagHeight=p_Height;
		this.addNewBag();
	}

	private void addNewBag()
	{
		this.m_Contents.Add(new Item[this.m_BagHeight*this.m_BagHeight]);
	}

	public void addItem(Item p_Item)
	{
		if(p_Item.m_IsStackable)
		{
			Item stackable = this.findStackable(p_Item.m_StackableID);
			if(stackable!=null)
			{
				stackable.networkView.RPC("ChangeQuantity",RPCMode.AllBuffered,p_Item.m_ItemQuantity);
				Network.Destroy(p_Item.gameObject);
				this.OnStackPickup(stackable);
				return;
			}
		}
		foreach(Item[] bag in this.m_Contents)
		{
			if(this.addIntoBag(p_Item,bag))return;
		}
		this.addNewBag();
		this.addIntoBag(p_Item,this.m_Contents[m_Contents.Count]);
		p_Item.Pickup(this.m_Owner);
		this.OnNewPickup(p_Item);
	}

	public void removeItem(Item p_Item)
	{
		foreach (Item[] bag in this.m_Contents)
		{
			for(int i = 0;i<bag.Length;i++)
			{
				if(bag[i]==p_Item)
				{
					bag[i].m_Master=null;
					this.OnRemoveItem(bag[i]);
					bag[i]=null;
					if(this.isBagEmpty(bag))
					{
						this.m_Contents.Remove(bag);
					}
				}
			}
		}
	}
	public int getWeight()
	{
		int weight = 0;
		foreach(Item[] bag in this.m_Contents)
		{
			foreach(Item i in bag)
			{
				weight+=i.m_ItemWeight;
			}
		}
		return weight;
	}

	private void removeEmptyBags()
	{
		foreach(Item[] bag in this.m_Contents)
		{
			if(this.isBagEmpty(bag))
			{
				this.m_Contents.Remove(bag);
			}
		}
	}

	private bool isBagEmpty(Item[] p_Bag)
	{
		foreach(Item i in p_Bag)
		{
			if(i!=null)return false;
		}
		return true;
	}
	private bool addIntoBag(Item p_Item,Item[] p_Bag)
	{
		for (int i = 0;i<p_Bag.Length;i++)
		{
			if(p_Bag[i]==null)
			{
				p_Bag[i]=p_Item;
				return true;
			}
		}
		return false;
	}
	private Item findStackable(int p_StackableID)
	{
		foreach(Item[] bag in this.m_Contents)
		{
			for(int i = 0;i<bag.Length;i++)
			{
				if(bag[i]!=null && bag[i].m_StackableID==p_StackableID)
				{
					return bag[i];
				}
			}
		}
		return null;
	}
	protected virtual void OnStackPickup(Item p_Item){}
	protected virtual void OnNewPickup(Item p_Item){}
	protected virtual void OnRemoveItem(Item p_Item){}

		                         
}
