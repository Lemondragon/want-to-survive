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
			trans.offsetMax=new Vector2(0.1f,0.1f);
			trans.position= new Vector3(0,0,0);
			trans.anchorMax= new Vector2(1,1);
			trans.anchorMin= new Vector2(0,1);
			//Button button = host.GetComponent<Button>();
			//button.guiText.text=m_hostData[i].gameName;
		}
	}
}
