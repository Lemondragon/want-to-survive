using UnityEngine;
using System.Collections;

public class QuickSlotManager : MonoBehaviour,IObserver{

	private QuickSlot[] m_QuickSlots = new QuickSlot[8]; //Actions rapides.«
	private Observable m_Observable = new Observable();
	private bool m_ShortcutsActive = true;
	/// <summary>
	/// Classe représentant une action rapide.
	/// </summary>
	private class QuickSlot
	{
		public Item m_Item; //Objet ciblé par l'action.
		public byte m_ActionNumber; //Numéro de l'action à effectuer sur l'objet.
		/// <summary>
		/// Initializes a new instance of the <see cref="PlayerMotor.QuickSlot"/> class.
		/// </summary>
		/// <param name='p_Item'>
		/// Action's target.
		/// </param>
		/// <param name='p_ActionNumber'>
		/// Target's action's number.
		/// </param>
		public QuickSlot (Item p_Item,byte p_ActionNumber)
		{
			this.m_ActionNumber=p_ActionNumber;
			this.m_Item = p_Item;
		}

		public void Execute()
		{
			this.m_Item.UseAction(this.m_ActionNumber);
		}
	}

	public void setQuickSlot(Item p_Item, int p_ActionNumber,int p_Quickslot)
	{
		this.m_QuickSlots[p_Quickslot]= new QuickSlot(p_Item,(byte)p_ActionNumber);
		this.m_Observable.notify (ObserverMessages.QuickSlotUpdated, p_Quickslot);
	}

	public Observable getObservable()
	{
		return this.m_Observable;
	}

	public void dismissQuickSlotsOf(Item p_Item)
	{
		for(int i = 0;i<8;i++)
		{
			if(this.m_QuickSlots[i]!=null&&this.m_QuickSlots[i].m_Item.Equals(p_Item))
			{
				this.m_QuickSlots[i]=null;
				this.m_Observable.notify(ObserverMessages.QuickSlotUpdated,i);
			}
		}
	}

	public void useQuickSlot(int p_Quickslot)
	{
		if(this.m_QuickSlots[p_Quickslot]!=null)
		{
			this.m_QuickSlots[p_Quickslot].Execute();
		}
	}

	public Sprite getQuickSlotIcon(int p_QuickSlot)
	{
		if (this.m_QuickSlots [p_QuickSlot] != null) 
		{
			return this.m_QuickSlots[p_QuickSlot].m_Item.Image;
		}
		return null;
	}

	public void Update()
	{
		if(this.m_ShortcutsActive)
		{
			for (int j = 0;j<8;j++)
			{
				if(Input.GetButtonDown("QuickSlot_"+(j)))
				{
					this.useQuickSlot(j);
				}
			}
		}
	}

	public void update(ObserverMessages p_Message,object p_Argument)
	{
		switch(p_Message)
		{
		case ObserverMessages.ItemLost:
			this.dismissQuickSlotsOf(p_Argument as Item);
			break;
		case ObserverMessages.InventoryStateChanged:
			this.m_ShortcutsActive=!((bool)p_Argument);
			break;
		}
	}
}
