using UnityEngine;
using System.Collections;

public class ActionButton : MonoBehaviour {

	public Item m_Item;
	public int m_ActionNumber;
	private bool isHovered = false;

	public void action()
	{
		this.m_Item.UseAction((byte)m_ActionNumber);
	}

	public void setHover(bool p_Hover)
	{
		this.isHovered=p_Hover;
	}

	public void Update()
	{
		if(this.isHovered)
		{
			for(int j = 0;j<8;j++)
			{
				if(Input.GetButtonDown("QuickSlot_"+(j)))
				{
					PlayerUI.m_InventoryView.setQuickSlot(this.m_ActionNumber,j);
				}
			}
		}
	}
}
