using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HostLabel : MonoBehaviour {

	public Text m_ServerName;
	public Image[] m_ClientIcons;
	private HostData m_Host;
	// Update is called once per frame
	public void setHost(HostData p_HostData)
	{
		this.m_Host = p_HostData;
		this.m_ServerName.text = p_HostData.gameName;
		for (int i = 0; i<p_HostData.connectedPlayers; i++) 
		{
			this.m_ClientIcons[i].color=Color.green;
		}
	}

	public void connect()
	{
		EV.networkManager.connect (this.m_Host);
	}


}
