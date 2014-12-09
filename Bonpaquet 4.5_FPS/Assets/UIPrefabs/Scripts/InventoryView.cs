using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour,IObserver{

	private Inventory m_DisplayedInventory;
	public ActionButton m_ActionButtonPrefab;
	private ActionButton[] m_ActionButtons = new ActionButton[0];
	public ItemButton[] m_ItemButtons;
	public Text m_PageIndicator;
	public Text m_SelectedName;
	public GameObject m_ActionPanel;
	private int m_Page=0;
	private Item m_SelectedItem;

	public Inventory getDisplayedInventory()
	{
		return this.m_DisplayedInventory;
	}

	public void setDisplayedInventory(Inventory p_Inventory)
	{
		if(this.m_DisplayedInventory!=null)
		{
			m_DisplayedInventory.getObservable().unsubscribe(this);
		}
		p_Inventory.getObservable().subscribe(this);
		this.m_DisplayedInventory=p_Inventory;
		this.updateAll();
		this.updatePageIndicator();
	}

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
	
	public void selectItem(Item p_Item)
	{
		foreach(ActionButton ab in this.m_ActionButtons)
		{
			if(ab!=null)
			{
				Destroy(ab.gameObject);
			}
		}
		if(p_Item!=null)
		{
			this.m_ActionPanel.SetActive(true);
			this.m_SelectedName.text=p_Item.m_ItemName;
			this.m_ActionButtons = new ActionButton[p_Item.m_PossibleActions.Count];
			for(int i = 0 ;i<p_Item.m_PossibleActions.Count;i++)
			{
				ActionButton newAction = (GameObject.Instantiate(this.m_ActionButtonPrefab.gameObject) as GameObject).GetComponent<ActionButton>();
				newAction.transform.parent=this.m_ActionPanel.transform;
				RectTransform rt = newAction.transform as RectTransform;
				rt.localScale = new Vector3(1,1,1);
				rt.localRotation = Quaternion.identity;
				rt.localPosition = new Vector3 (0,0,0);
				rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top,100+(20*i),20);
				newAction.setAction(p_Item,i);
				this.m_ActionButtons[i]= newAction;
			}
		}
		else
		{
			this.m_ActionPanel.SetActive(false);
		}
		this.m_SelectedItem=p_Item;
	}

	public Item getSelected()
	{
		return this.m_SelectedItem;
	}

	public void update(ObserverMessages p_Message, object p_Argument)
	{
		switch (p_Message)
		{
			case ObserverMessages.ItemLost:
				if(this.m_SelectedItem==p_Argument as Item) this.selectItem(null);
				this.updateAll();
			break;
			case ObserverMessages.ItemGained:
				this.updateAll();
			break;
			case ObserverMessages.BagQuantityChange:
				this.updatePageIndicator();
			break;
		}
	}
}
