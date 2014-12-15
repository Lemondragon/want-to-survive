using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Observable {

	private List<IObserver> m_Observers = new List<IObserver>();

	public void notify(ObserverMessages p_Message)
	{
		foreach(IObserver o in this.m_Observers)
		{
			o.update(p_Message,null);
		}
	}

	public void notify(ObserverMessages p_Message,object p_Argument)
	{
		foreach(IObserver o in this.m_Observers)
		{
			o.update(p_Message,p_Argument);
		}
	}

	public void subscribe(IObserver p_Observer)
	{
		if(p_Observer==null)
		{
			Debug.LogError("Tried to add a null Observer.");
		}
		else
		{
			this.m_Observers.Add(p_Observer);
		}
	}

	public void unsubscribeAll()
	{
		this.m_Observers.Clear();
	}

	public void unsubscribe(IObserver p_Observer)
	{
		this.m_Observers.Remove(p_Observer);
	}
}
