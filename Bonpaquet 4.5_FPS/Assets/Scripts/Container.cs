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
		if(p_Item.IsStackable)
		{
			Item stackable = this.findStackable(p_Item.StackableID);
			if(stackable!=null)
			{
				stackable.networkView.RPC("ChangeQuantity",RPCMode.AllBuffered,p_Item.Quantity);
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
					bag[i].Master=null;
					bag[i]=null;
					if(this.m_Contents.Count>1&&this.isBagEmpty(bag))
					{
						this.removeBag(bag);
					}
					this.m_Observable.notify(ObserverMessages.ItemLost,p_Item);
				}
			}
		}
	}

	public void Swap (int p_From,int p_To,int p_Bag)
	{
		Item temp = this.m_Contents[p_Bag][p_From];
		this.m_Contents[p_Bag][p_From]=this.m_Contents[p_Bag][p_To];
		this.m_Contents[p_Bag][p_To]=temp;
		this.m_Observable.notify(ObserverMessages.BagDispositionChanged,p_Bag);
	}

	public int getIndexOf(Item p_Item,int p_Bag)
	{
		for(int i = 0;i<this.m_Contents[p_Bag].Length;i++)
		{
			Item itm = this.m_Contents[p_Bag][i];
			if(itm!=null&&itm.Equals(p_Item))
			{
				return i;
			}
		}
		return -1;
	}

	public int getWeight()
	{
		int weight = 0;
		foreach(Item[] bag in this.m_Contents)
		{
			foreach(Item i in bag)
			{
				weight+=i.Weight;
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
				if(bag[i]!=null && bag[i].StackableID==p_StackableID)
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
