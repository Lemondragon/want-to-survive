using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CanevaAdjuster : MonoBehaviour {

	void Start () 
	{
		Canvas can = this.GetComponent<Canvas>();
		can.transform.parent = null;
		//can.planeDistance=can.worldCamera.nearClipPlane+0.001f;
		Destroy(this);
	}

}
