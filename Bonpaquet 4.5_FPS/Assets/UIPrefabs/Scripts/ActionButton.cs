using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour {

	private Item m_Item;
	private int m_ActionNumber;
	private bool isHovered = false;

	public void setAction(Item p_Item,int p_ActionNumber)
	{
		this.m_Item=p_Item;
		this.m_ActionNumber=p_ActionNumber;
		this.updateUI();
	}

	public void updateUI()
	{
		if(this.m_Item!=null)
		{
			this.GetComponentInChildren<Text>().text=this.m_Item.m_PossibleActionNames[this.m_ActionNumber];
		}
		else
		{
			Destroy(this.gameObject);
		}
	}

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
					PlayerUI.m_QuickSlotManager.setQuickSlot(this.m_Item,this.m_ActionNumber,j);
				}
			}
		}
	}
}
