using UnityEngine;
using System.Collections;

public class Inventory : Container,IObserver {

	public Inventory (PlayerMotor p_Owner):base(9,p_Owner)
	{
		if(p_Owner==null)throw new MissingReferenceException("An inventory's owner can't be null");
		this.getObservable().subscribe(this);
	}

	public void update(ObserverMessages p_Message, object p_Argument)
	{
		Item item = p_Argument as Item;
		switch(p_Message)
		{
			case ObserverMessages.ItemStacked:
				EV.gameManager.GUIMessage("You recieved more :"+item.m_ItemName,EV.QualityColor(item.m_ItemQuality));
			break;
			case ObserverMessages.ItemGained:
				EV.gameManager.GUIMessage("You recieved :"+item.m_ItemName,EV.QualityColor(item.m_ItemQuality));
			break;
		}
	}


}
