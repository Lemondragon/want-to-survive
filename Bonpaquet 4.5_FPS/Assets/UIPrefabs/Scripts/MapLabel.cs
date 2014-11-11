using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapLabel : MonoBehaviour {

	public Text m_MapName;
	private string m_Name;
	public Image m_MapPreview;
	
	public void setMap(string p_MapName)
	{
		this.m_Name = p_MapName;
		this.m_MapName.text = p_MapName;
		if (p_MapName.Equals (EV.networkManager.m_SelectedLevel)) 
		{
			this.m_MapName.color=Color.blue;
		}
	}

	public void select()
	{
		EV.networkManager.m_SelectedLevel = this.m_Name;
		EV.networkManager.m_LobbyView.showLevelList ();
	}


}
