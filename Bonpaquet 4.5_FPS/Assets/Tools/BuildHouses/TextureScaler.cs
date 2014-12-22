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
		foreach(Material m in this.renderer.materials)
		{
			if(m_SwapXY)
			{
				m.mainTextureScale = new Vector2(this.transform.localScale.x*m_Scalex,this.transform.localScale.z*m_Scaley);
			}
			else
			{
				m.mainTextureScale = new Vector2(this.transform.localScale.x*m_Scalex,this.transform.localScale.y*m_Scaley);
			}
		}
		this.transform.parent=oldParent;
		Destroy(this);
	}
	
}
