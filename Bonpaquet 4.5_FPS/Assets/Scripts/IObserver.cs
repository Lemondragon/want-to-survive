using UnityEngine;
using System.Collections;

public interface IObserver 
{
	void update(ObserverMessages p_Message,object p_Argument);
}
