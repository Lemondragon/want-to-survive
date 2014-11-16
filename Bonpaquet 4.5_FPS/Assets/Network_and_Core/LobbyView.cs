using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LobbyView : MonoBehaviour 
{
	public Canvas[] m_Canevas;
	public InputField m_IFUsername;
	public InputField m_IFServername;

	public Button m_HostPrefab;
	public GameObject m_HostPanel;
	public Button m_MapPrefab;
	public GameObject m_MapPanel;
	public PlayerLabel[] m_PlayerLabels;
	public Text m_ServerNameLabel;
	public Toggle[] m_StartingFocusToogle;
	public Toggle m_UseExtendedSkillTreeToogle;
	private int m_ActiveView = 0;

	public void Start()
	{
		this.changeView(0);
	}

	public void changeView(int p_View)
	{
		this.m_Canevas[this.m_ActiveView].gameObject.SetActive(false);
		this.m_ActiveView=p_View;
		this.m_Canevas[p_View].gameObject.SetActive(true);
		switch(this.m_ActiveView)
		{
		case 3:
			this.showLevelList();
			break;
		case 4:
			this.updatePlayerLabels();
			this.m_ServerNameLabel.text=EV.networkManager.m_ServerName;
			break;
		}
	}

	public void setUsername()
	{
		EV.networkManager.m_MyName=this.m_IFUsername.text.text;
	}

	public void setServerName()
	{
		EV.networkManager.m_ServerName=this.m_IFServername.text.text;
	}

	public void refreshHostList()
	{
		EV.networkManager.RefreshHostList();
	}

	public void showHostList(HostData[] m_hostData)
	{
		foreach(Button button in this.m_HostPanel.GetComponentsInChildren<Button>())
		{
			Destroy(button.gameObject);
		}

		for(int i = 0;i<m_hostData.Length;i++)
		{
			GameObject host = GameObject.Instantiate(this.m_HostPrefab.gameObject) as GameObject;
			RectTransform trans = host.transform as RectTransform;
			trans.parent=this.m_HostPanel.transform;
			trans.localScale= new Vector3(1,1,1);
			trans.localPosition= new Vector3(0,50+(34*(i+1)),0);
			trans.sizeDelta=new Vector2(-18,34);

			HostLabel hostlabel = host.GetComponent<HostLabel>();
			hostlabel.setHost(m_hostData[i]);
		}
	}

	public void showLevelList()
	{
		foreach(Button button in this.m_MapPanel.GetComponentsInChildren<Button>())
		{
			Destroy(button.gameObject);
		}
		
		for(int i = 0;i<EV.networkManager.m_LevelNames.Length;i++)
		{
			GameObject map = GameObject.Instantiate(this.m_MapPrefab.gameObject) as GameObject;
			RectTransform trans = map.transform as RectTransform;
			trans.parent=this.m_MapPanel.transform;
			trans.localScale= new Vector3(1,1,1);
			trans.localPosition= new Vector3(0,-5+(34*(i+1)),0);
			trans.sizeDelta=new Vector2(-18,34);
			
			MapLabel mapLabel = map.GetComponent<MapLabel>();
			mapLabel.setMap(EV.networkManager.m_LevelNames[i]);
		}
	}

	public void updatePlayerLabels()
	{
		foreach(PlayerLabel pl in this.m_PlayerLabels)
		{
			pl.updateView();
		}
	}

	public void setReadyState(bool p_State)
	{
		this.networkView.RPC("RPC_setReadyState",RPCMode.AllBuffered,Network.player,p_State);
	}

	[RPC]
	public void RPC_setReadyState(NetworkPlayer p_Player,bool p_State)
	{
		bool ready = true;
		foreach(PlayerLabel pl in this.m_PlayerLabels)
		{
			NetworkManager.PlayerInfos pi = pl.getMyInfos();
			if(pi!=null)
			{
				if(pi.m_AssociatedPlayer.Equals(p_Player))
				{
					pl.m_IsReadyToggle.isOn=p_State;
				}
				ready&=pl.m_IsReadyToggle.isOn;
			}
		}
		if(ready)
		{
			EV.networkManager.m_StartFocusBonus = new bool[this.m_StartingFocusToogle.Length];
			for (int i = 0;i<this.m_StartingFocusToogle.Length;i++)
			{
				EV.networkManager.m_StartFocusBonus[i]=this.m_StartingFocusToogle[i].isOn;
			}
			EV.networkManager.m_UseExtendedSkillTree=this.m_UseExtendedSkillTreeToogle.isOn;
			EV.networkManager.startGame();
		}
	}
}