using UnityEngine;
using System.Collections;

public class Ragdoller : MonoBehaviour {

	[SerializeField]
	private GameObject[] m_Parts;
	[SerializeField]
	private float m_DespawnTime = 10f;

	public void Ragdoll(Vector3 p_ImpactPoint,float p_ImpactPower)
	{
		foreach(Component c in this.GetComponentsInChildren<Transform>())
		{
			Destroy(c.gameObject,this.m_DespawnTime);
		}
		foreach(Collider c in this.GetComponentsInChildren<Collider>())
		{
			Destroy(c);
		}
		foreach(GameObject o in this.m_Parts)
		{
			o.transform.parent=null;
			o.gameObject.AddComponent<Rigidbody>();
			o.gameObject.AddComponent<BoxCollider>();
			o.rigidbody.AddForceAtPosition((o.transform.position - p_ImpactPoint).normalized*p_ImpactPower,p_ImpactPoint);
		}
	}
}
