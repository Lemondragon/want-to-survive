using UnityEngine;
using System.Collections;

public class SelectionCube : MonoBehaviour 
	{
	
	[HideInInspector]public bool PosIsValid = false;
	private Vector3 lastPos;
	
	void Update () 
	{
		if(this.transform.position!=this.lastPos)
		{
			this.PosIsValid=this.ValidatePosition();
			if(this.PosIsValid)
			{
				this.renderer.material.SetColor("_TintColor",Color.cyan);
			}
			else
			{
				this.renderer.material.SetColor("_TintColor",Color.red);
			}
			this.lastPos=this.transform.position;
		}
	}
	public bool ValidatePosition()
	{
		return EV.gameManager.getInnerZone(Mathf.RoundToInt(this.transform.position.x),Mathf.RoundToInt(this.transform.position.z))==null;
	}
	public void ForceValidation()
	{
		this.lastPos=new Vector3(-1,-1,-1);
	}
}
