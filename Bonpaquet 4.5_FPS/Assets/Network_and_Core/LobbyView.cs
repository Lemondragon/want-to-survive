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
		if (this.m_ActiveView == 3) 
		{
			this.showLevelList();
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
}