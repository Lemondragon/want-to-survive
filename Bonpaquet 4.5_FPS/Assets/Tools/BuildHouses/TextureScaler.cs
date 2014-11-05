using UnityEngine;
using System.Collections;

public class TextureScaler : MonoBehaviour {
	public float m_Scalex = 1;
	public float m_Scaley = 1;
	public bool m_SwapXY = false;
	// Use this for initialization
	void Start () 
	{
		Transform oldParent = this.transform.parent;
		this.transform.parent=null;
		if(m_SwapXY)
		{
			this.renderer.material.mainTextureScale = new Vector2(this.transform.localScale.x*m_Scalex,this.transform.localScale.z*m_Scaley);
			this.renderer.material.SetTextureScale("_BumpMap",new Vector2(this.transform.localScale.x*m_Scalex,this.transform.localScale.z*m_Scaley));
		}
		else
		{
			this.renderer.material.mainTextureScale = new Vector2(this.transform.localScale.x*m_Scalex,this.transform.localScale.y*m_Scaley);
			this.renderer.material.SetTextureScale("_BumpMap",new Vector2(this.transform.localScale.x*m_Scalex,this.transform.localScale.y*m_Scaley));
		}
		this.transform.parent=oldParent;
		Destroy(this);
	}
	
}
