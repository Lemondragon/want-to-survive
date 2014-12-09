using UnityEngine;
using System.Collections;

public class QuickslotManager : MonoBehaviour,IObserver{

	private QuickSlot[] m_QuickSlots = new QuickSlot[8]; //Actions rapides.
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
	}

	public void dismissQuickSlotsOf(Item p_Item)
	{
		for(int i = 0;i<8;i++)
		{
			if(this.m_QuickSlots[i]!=null&&this.m_QuickSlots[i].m_Item.Equals(p_Item))
			{
				this.m_QuickSlots[i]=null;
			}
		}
	}

	public void Update()
	{
		for (int j = 0;j<8;j++)
		{
			if(Input.GetButtonDown("QuickSlot_"+(j)))
			{
				if(this.m_QuickSlots[j]!=null)
				{
					this.m_QuickSlots[j].Execute();
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
		}
	}
}
