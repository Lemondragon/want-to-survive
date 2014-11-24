using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour {

	public Inventory m_DisplayedInventory;
	public ItemButton[] m_ItemButtons;
	public Text m_PageIndicator;
	private int m_Page=0;

	public void setPage(int p_Page)
	{
		this.m_Page=p_Page;
		this.updatePageIndicator();
		this.updateAll();
	}

	public void updateAll()
	{
		for(int i = 0;i<this.m_ItemButtons.Length;i++)
		{
			this.updateButton(i);
		}
	}

	public void updateButton(int p_ButtonIndex)
	{
		if(this.m_DisplayedInventory.m_Contents.Count<=this.m_Page)
		{
			this.m_Page=this.m_DisplayedInventory.m_Contents.Count-1;
			this.updateAll();
		}
		else
		{
			this.m_ItemButtons[p_ButtonIndex].m_Item=this.m_DisplayedInventory.m_Contents[this.m_Page][p_ButtonIndex];
			this.m_ItemButtons[p_ButtonIndex].updateUI();
		}
	}

	public void changePage (int m_Direction)
	{
		this.setPage(Mathf.Clamp(this.m_Page+m_Direction,0,this.m_DisplayedInventory.m_Contents.Count-1));
	}

	public void updatePageIndicator()
	{
		this.m_PageIndicator.text = (this.m_Page+1)+"/"+this.m_DisplayedInventory.m_Contents.Count;
	}
}
