using UnityEngine;
using System.Collections;

public class Ceilling : MonoBehaviour 
{
	public Renderer[] m_ChildRenderers;

	/*
	void OnTriggerEnter(Collider colliderInfo)
	{
		this.TriggerRenderer(false,colliderInfo);
	}
	
	void OnTriggerExit(Collider colliderInfo)
	{
		this.TriggerRenderer(true,colliderInfo);
	}
    */
	void TriggerRenderer(bool p_State,Collider p_Collider)
	{
		if(p_Collider.networkView!=null)
		{
			if(p_Collider.networkView.isMine && p_Collider.CompareTag("Player"))
			{
				this.renderer.enabled=p_State;
				foreach(Renderer r in this.m_ChildRenderers)
				{
					r.enabled=p_State;
				}
			}
		}
	}
	public void setColor(Color p_Color)
	{
		this.networkView.RPC("RPC_SetColor",RPCMode.All,new Vector3(p_Color.r,p_Color.g,p_Color.b));
	}
	[RPC]
	public void RPC_SetColor(Vector3 p_Color)
	{
		Color col = new Color (p_Color.x,p_Color.y,p_Color.z,1);
		this.renderer.materials[1].SetColor("_Color",col);
		foreach(Renderer r in this.m_ChildRenderers)
		{
			r.materials[1].SetColor("_Color",col);
		}
	}
}
