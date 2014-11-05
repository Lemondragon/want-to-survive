using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Ceilling))]
class CeillingSwapper : Editor  
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if(GUILayout.Button("Turn 90"))
		{
			Transform trans = (this.target as Ceilling).transform;
			trans.Rotate(new Vector3(0,0,90));
			trans.localScale= new Vector3(trans.localScale.y/2,trans.localScale.x*2,trans.localScale.z);
		}
	}
}
