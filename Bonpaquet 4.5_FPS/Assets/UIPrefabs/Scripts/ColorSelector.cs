using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorSelector : MonoBehaviour {

	public Slider[] m_Sliders;
	public Image m_Preview;

	public void SliderChanged()
	{
		EV.networkManager.m_MyColor = new Vector3 (this.m_Sliders [0].value, this.m_Sliders [1].value, this.m_Sliders [2].value);
		EV.networkManager.networkView.RPC ("RPC_SetLobbyColor", RPCMode.All, Network.player, EV.networkManager.m_MyColor);
		this.m_Preview.color = new Color (this.m_Sliders [0].value, this.m_Sliders [1].value, this.m_Sliders [2].value,1);
	}
}
