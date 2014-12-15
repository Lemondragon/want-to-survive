using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CanevaSpaceShrinker : MonoBehaviour {

	void Start () 
	{
		Canvas can = this.GetComponent<Canvas>();
		can.planeDistance=can.worldCamera.nearClipPlane+0.001f;
		Destroy(this);
	}

}
