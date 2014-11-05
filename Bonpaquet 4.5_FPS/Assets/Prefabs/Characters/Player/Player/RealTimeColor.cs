using UnityEngine;
using System.Collections;

public class RealTimeColor : MonoBehaviour {

	
	// Update is called once per frame
	void Update () 
	{
		foreach(Renderer r in this.GetComponentsInChildren(typeof(Renderer)))
		{
			r.material.SetColor("_Color",new Color(EV.networkManager.m_MyColor.x,EV.networkManager.m_MyColor.y,EV.networkManager.m_MyColor.z,1));
		}
	}
}
