using UnityEngine;
using System.Collections;

public class LightFlicker : MonoBehaviour {

	public float m_FlickDuration = 6;
	public float m_StableMinDuration = 5;
	public float m_StableMaxDuration = 10;

	private Light m_Light;
	private bool m_Flick = false;
	private bool m_Powered = true;
	private float m_Intensity;

	private float m_NextChangeAt = 0;

	// Use this for initialization
	void Start () 
	{
		this.m_Light = this.GetComponentInChildren<Light> ();
		this.m_Powered = Random.value > 0.75;//Une chance sur 4 d'avoir un lampadaire allumé.
		this.m_Intensity = this.m_Light.intensity;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (this.m_Flick) 
		{
			this.m_Light.intensity = this.m_Intensity * Random.value;
		} 
		else 
		{
			if(this.m_Powered)
			{
				this.m_Light.intensity = this.m_Intensity;
			}
			else
			{
				this.m_Light.intensity = 0;
			}
		}

		if (this.m_NextChangeAt < Time.time) 
		{
			this.m_Flick=!this.m_Flick;
			if(this.m_Flick)
			{
				this.m_NextChangeAt=Time.time+this.m_FlickDuration * Random.value;
			}
			else
			{
				this.m_NextChangeAt=Time.time+Mathf.Lerp(this.m_StableMinDuration,this.m_StableMaxDuration,Random.value);
			}
		}
	}
}
