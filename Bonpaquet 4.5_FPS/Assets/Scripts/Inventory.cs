using UnityEngine;
using System.Collections;

public class Inventory : Container {

	public Inventory (PlayerMotor p_Owner):base(3,3,p_Owner)
	{
		if(p_Owner==null)throw new MissingReferenceException("An inventory's owner can't be null");
	}

	protected override void OnNewPickup (Item p_Item)
	{
		EV.gameManager.GUIMessage("You recieved :"+p_Item.m_ItemName,EV.QualityColor(p_Item.m_ItemQuality));
	}

	protected override void OnStackPickup (Item p_Item)
	{
		EV.gameManager.GUIMessage("You recieved more :"+p_Item.m_ItemName,EV.QualityColor(p_Item.m_ItemQuality));
	}

	protected override void OnRemoveItem (Item p_Item)
	{
		for(int i = 0;i<this.m_Owner.m_QuickSlots.Length;i++)
		{
			PlayerMotor.QuickSlot qs = this.m_Owner.m_QuickSlots[i];
			if(qs!=null)
			{
				if(qs.m_Item.Equals(this))
				{
					this.m_Owner.m_QuickSlots[i]=null;
				}
			}
		}
	}



}
