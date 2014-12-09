using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour {
	[HideInInspector]
	public Item m_Item;
	public Image m_QualityImg;
	public Image m_ItemLogo;
	public Text m_QteText;
	public Button m_Button;

	public void updateUI()
	{
		if(m_Item!=null)
		{
			this.m_Button.interactable=true;
			this.m_QualityImg.color= EV.QualityColor(this.m_Item.m_ItemQuality);
			this.m_ItemLogo.enabled=true;
			this.m_ItemLogo.sprite = this.m_Item.m_ItemImage;
			if(this.m_Item.m_ItemQuantity>1)
				this.m_QteText.text="X "+this.m_Item.m_ItemQuantity;
			else
				this.m_QteText.text="";
		}
		else
		{
			this.m_Button.interactable=false;
			this.m_QualityImg.color= Color.red;
			this.m_ItemLogo.enabled=false;
			this.m_QteText.text="";
		}
	}

	public void click()
	{
		if(PlayerUI.m_InventoryView.getSelected()==this.m_Item)
		{
			this.m_Item.UseAction(0);
		}
		else
		{
			PlayerUI.m_InventoryView.selectItem(this.m_Item);
		}
	}
}
