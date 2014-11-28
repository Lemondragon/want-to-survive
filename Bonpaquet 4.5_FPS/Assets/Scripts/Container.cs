using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Container {

	public List<Item[]> m_Contents = new List<Item[]>();
	private Observable m_Observable = new Observable();
	private int m_Size;
	protected PlayerMotor m_Owner;
	
	public Container(int p_Size, PlayerMotor p_Owner)
	{
		this.m_Owner=p_Owner;
		this.m_Size=p_Size;
		this.addNewBag();
	}

	public Observable getObservable()
	{
		return this.m_Observable;
	}

	private void addNewBag()
	{
		this.m_Observable.notify(ObserverMessages.BagQuantityChange);
		this.m_Contents.Add(new Item[this.m_Size]);
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
				this.m_Observable.notify(ObserverMessages.ItemStacked,stackable);
				return;
			}
		}
		foreach(Item[] bag in this.m_Contents)
		{
			if(this.addIntoBag(p_Item,bag))return;
		}
		this.addNewBag();
		this.addIntoBag(p_Item,this.m_Contents[m_Contents.Count-1]);
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
					this.m_Observable.notify(ObserverMessages.ItemLost,bag[i]);
					bag[i]=null;
					if(this.isBagEmpty(bag))
					{
						this.removeBag(bag);
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
		bool remove = false;
		foreach(Item[] bag in this.m_Contents)
		{
			if(this.isBagEmpty(bag))
			{
				remove = true;
				this.removeBag(bag);
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
				p_Item.Pickup(this.m_Owner);
				this.m_Observable.notify(ObserverMessages.ItemGained,p_Item);
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

	private void removeBag(Item[] p_Bag)
	{
		this.m_Contents.Remove(p_Bag);
		this.m_Observable.notify(ObserverMessages.BagQuantityChange);
	}
}
