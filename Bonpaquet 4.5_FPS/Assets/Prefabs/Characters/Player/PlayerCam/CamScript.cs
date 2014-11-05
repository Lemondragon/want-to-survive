using UnityEngine;
using System.Collections;

public class CamScript : MonoBehaviour {
	
	public Transform target;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(target!=null)
		{
			this.transform.position = new Vector3(target.position.x,this.transform.position.y, target.position.z);
		}
	}
}
