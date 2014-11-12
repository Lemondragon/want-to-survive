using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerLabel : MonoBehaviour 
{
	public Text m_PlayerName;
	public Toggle m_IsReadyToggle;
	public Button m_KickButton;
	public int m_Index;

	public bool isReady()
	{
		if(this.m_IsReadyToggle.enabled)
			return this.m_IsReadyToggle.isOn;
		return false;
	}
	
	public void updateView()
	{
		if(this.m_Index<EV.networkManager.m_PlayerInfos.Count)
		{
			NetworkManager.PlayerInfos infos = this.getMyInfos();
			this.m_PlayerName.text = infos.m_Name;
			this.m_PlayerName.color = infos.m_Color;
			this.m_IsReadyToggle.gameObject.SetActive(true);
			this.m_KickButton.gameObject.SetActive(Network.isServer||infos.m_AssociatedPlayer.Equals(Network.player));
		}
		else
		{
			this.m_PlayerName.text="Available";
			this.m_PlayerName.color=Color.gray;
			this.m_KickButton.gameObject.SetActive(false);
			this.m_IsReadyToggle.gameObject.SetActive(false);
		}
	}

	private NetworkManager.PlayerInfos getMyInfos()
	{
		int index = 0;
		NetworkManager.PlayerInfos infos = null;
		foreach(NetworkManager.PlayerInfos pi in EV.networkManager.m_PlayerInfos.Values)
		{
			if(index==this.m_Index)
			{
				infos = pi;
				break;
			}
			index++;
		}
		return infos;
	}

	public void kick()
	{
		EV.networkManager.kickPlayer(this.getMyInfos().m_AssociatedPlayer);
	}
}
