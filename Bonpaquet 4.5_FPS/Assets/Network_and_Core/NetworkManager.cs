using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour 
{
	const float CAM_DISTANCE = 8;
	//Refs
	public LobbyView m_LobbyView;
	public string[] m_LevelNames;
	public string m_SelectedLevel = "GrassLand";
	public GameObject m_PlayerPrefab;//Player Default Prefab.
	public GameObject m_GameManagerPrefab;//Default GameManager Prefab.
	public GameObject[] m_TestSpawn; //Objets to spawn at map's center for testing purposes
	public Dictionary<NetworkPlayer,PlayerInfos> m_PlayerInfos = new Dictionary<NetworkPlayer, PlayerInfos>();//Dictionnary linking a NetworkPlayer to his PlayerInfo, should be synchonized on all connections.
	
	private string GAMENAME ="Bonpaquet_Gaming"; //GameName for the MasterServer
	private HostData[] m_HostData; //MasterServer Recieved Host Data for Server Selection
	
	//Client Info
	[HideInInspector] public GameObject m_MyPlayer;//Shorthand (Simmilar to -> this.m_PlayerInfos[Network.Player].m_PlayerMotor.gameobjet). Used to prevent zombies to locally detect damage on other players.
	private bool m_RefreshingHosts = false; //If we are waiting for the m_HostData to refresh.
	private float m_BusyTimer = 0f; //Time until when the network manager is busy (Wait for response from masterServer).
	public bool m_IsInGame = false; //If we are in game or in lobby.
	private bool m_ShowMenu = false; //If we show Network Menu while in game (For disconnection).
	[HideInInspector]public Vector3 m_MyColor = new Vector3(0.5f,0.5f,0.5f);//Selected Color by local player.
	private string m_ConsoleMessage; //Single Message Shown while not in Game. Used for Network state messages.
	public string m_MyName = "Player";//Selected name of the local player.
	public bool[] m_StartFocusBonus;
	public bool m_UseExtendedSkillTree=false;
	//Server Info
	public string m_ServerName = "Server";//Connected's Server Name.
	
	/// <summary>
	/// Class defining useful informations on fellow players.
	/// </summary>
	public class PlayerInfos
	{
		public NetworkPlayer m_AssociatedPlayer;//NetworkPlayer associé aux informations, correspond à sa Key dans m_PlayerInfos de NetworkManager
		public string m_Name; //Nom du joueur.
		public Color m_Color = Color.gray; //Couleur du joueur.
		public PlayerMotor m_PlayerMotor; //PlayerMotor du joueur (null lorsque dans le lobby).

		public PlayerInfos (NetworkPlayer p_Player,string p_Name)
		{
			this.m_AssociatedPlayer=p_Player;
			this.m_Name=p_Name;
		}
	}

	//MONOBEHAVIOUR
	
	void Start () 
	{
		//SINGLETON (kind of) avec Persistance entre les chargements.
		if(EV.networkManager==null)
		{
			EV.networkManager=this;
		}
		else
		{
			Destroy(this.gameObject);
		}
		this.m_HostData=new HostData[0];
		DontDestroyOnLoad (this.gameObject);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(m_RefreshingHosts)//Tente de recevoir des serveurs valides du MasterServer.
		{
			this.RefreshHosts();
		}
		if(Input.GetButtonDown("MainMenu"))//Si on appuie sur le bouton pour afficher le menu, on le toggle on/off.
		{
			this.m_ShowMenu = !m_ShowMenu;
		}
	}





	//EVENTS

	void OnServerInitialized()//Lorsque le serveur est initialisé.
	{
		//On crée le gameManager qui sera utilisé pour la partie.
		this.CreateGameManager ();
		//On indique la connection du serveur en tant que joueur.
		this.ConnectAsServer ();
	}
	
	void OnPlayerDisconnected(NetworkPlayer player) 
	{
		if(Network.isServer)//Lorsqu'un joueur se déconnecte, le serveur indique aux autres la perte du joueur.
		{
			this.networkView.RPC("RPC_NewPlayerHasDisconnected",RPCMode.All,player);
		}
    }
	
	void OnPlayerConnected(NetworkPlayer p_Player)
	{
		if(this.m_IsInGame)//Prevents connection from new players while already in game.
		{
			Network.CloseConnection(p_Player,false);
		}
		else 
		{
			this.GreetNewPlayer(p_Player);
		}
	}
	
	void OnDisconnectedFromServer()
	{
		this.m_ConsoleMessage="Disconnected from Server.";
		this.EndGame();
	}





	//PUBLIC 
	
	public void StartServer ()
	{
		Network.InitializeServer(3,25001,!Network.HavePublicAddress());
		MasterServer.RegisterHost(this.GAMENAME,this.m_ServerName,"Private");
	}

	/// <summary>
	/// Initializes the player.
	/// </summary>
	/// <param name='p_Player'>
	/// NetworkPlayer to Initialize its player.
	/// </param>
	public void InitializePlayer(NetworkPlayer p_Player)
	{
		if (!this.m_MyPlayer) //InitializePlayer ne fonctionnera pas si un joueur est DÉJA initialisé.
		{
			//On sélectionne un des objets aléatoirement ayant comme tag "Spawn" pour faire apparaitre le joueur dessus.
			this.m_MyPlayer = this.SpawnPlayerAtSpawn();
			if(Network.isServer) //Le serveur fait apparaitre les objets de départ à des fins de test.
			{
				this.SpawnTestObjects();
			}
			//On assigne les références possibles grâce à la création de ce nouveau joueur.
			PlayerMotor pm = this.m_MyPlayer.GetComponent<PlayerMotor>();
			pm.Init();
			//On lui donne son focus de base.
			this.GiveStartUpFocus(pm);
			pm.m_SkillTree.m_UseExtendedSkills=this.m_UseExtendedSkillTree;
			this.BroadcastPlayerInitialisation();
		}
	}

	/// <summary>
	/// Manual disconnect from game.
	/// </summary>
	public void disconnect()
	{
		if (Network.isServer)
		{
			MasterServer.UnregisterHost();
		}
		Network.Disconnect();
	}
	
	/// <summary>
	/// Manually Start the Game.
	/// </summary>
	public void startGame()
	{
		EV.gameManager.InitializeNetworkLevelLoad(this.m_SelectedLevel);
	}
	/// <summary>
	/// Eject a player from the game.
	/// </summary>
	/// <param name="p_Player">P_ player.</param>
	public void kickPlayer(NetworkPlayer p_Player)
	{
		if(p_Player.Equals(Network.player))
		{
			this.disconnect();
		}
		else
		{
			Network.CloseConnection(p_Player,true);
		}
	}
	/// <summary>
	/// Connect to the specified p_Host.
	/// </summary>
	/// <param name="p_Host">P_ host.</param>
	public void connect(HostData p_Host)
	{
		Network.Connect(p_Host);
	}

	/// <summary>
	/// Refreshs the host list.
	/// </summary>
	public void StartRefreshHostList()
	{
		this.m_ConsoleMessage="Requesting Hosts List...";
		MasterServer.RequestHostList(this.GAMENAME);
		m_RefreshingHosts=true;
		this.m_BusyTimer=Time.time+10;
	}

	//PRIVATE

	private void RefreshHosts()
	{
		if(MasterServer.PollHostList().Length!=0)//Si on en trouve un.
		{
			this.m_ConsoleMessage="Found "+MasterServer.PollHostList().Length+" Servers.";
			this.m_HostData=MasterServer.PollHostList();
			m_RefreshingHosts=false;
			this.m_BusyTimer=0;
			this.m_LobbyView.showHostList(this.m_HostData);
		}
		else if (this.m_BusyTimer<=Time.time)//Si on n'en trouve pas après 10 secondes.
		{
			this.m_ConsoleMessage="No Hosts found after 10 seconds.";
			m_RefreshingHosts=false;
			this.m_BusyTimer=0;
			this.m_LobbyView.showHostList(new HostData[0]);
		}
	}

	private void ConnectAsServer()
	{
		this.networkView.RPC("RPC_NewPlayerHasConnected",RPCMode.AllBuffered,Network.player);
		this.networkView.RPC("RPC_SetPlayerName",RPCMode.AllBuffered,Network.player,this.m_MyName);
	}

	private void CreateGameManager()
	{
		EV.gameManager = ((GameObject)Network.Instantiate(m_GameManagerPrefab,this.transform.position,Quaternion.identity,0)).GetComponent<GameManager>();
	}

	/// <summary>
	/// Spawns a player at a valid spawn.
	/// </summary>
	/// <returns>The player at spawn.</returns>
	private GameObject SpawnPlayerAtSpawn()
	{
		GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawn");
		Transform spawn = spawns[(int)Mathf.Floor(spawns.Length*Random.value)].transform;
		return (GameObject)Network.Instantiate(this.m_PlayerPrefab,spawn.position,spawn.rotation,0);
	}
	/// <summary>
	/// Spawns Objects in this.m_TestSpawn.
	/// </summary>
	private void SpawnTestObjects()
	{
		int spnOffset = 0;
		foreach(GameObject g in this.m_TestSpawn)
		{
			Network.Instantiate(g,new Vector3(0,1+spnOffset,0),Quaternion.identity,1);
			spnOffset+=3;
		}
	}
	/// <summary>
	/// Add focus according to selections to p_Motor.
	/// </summary>
	/// <param name="p_Motor">P_ motor.</param>
	private void GiveStartUpFocus(PlayerMotor p_Motor)
	{
		for(int i = 0;i<this.m_StartFocusBonus.Length;i++)
		{
			if(this.m_StartFocusBonus[i])
			{
				p_Motor.gainFocusExp((Focus)i,100);
			}
		}
	}
	/// <summary>
	/// Send informations about your newly created Player Instance to other Machines.
	/// </summary>
	private void BroadcastPlayerInitialisation()
	{
		//Signifie l'apparition de ce nouveau joueur.
		EV.gameManager.networkView.RPC("RPC_NewPlayer",RPCMode.All,this.m_MyPlayer.networkView.viewID,Network.player);
		//Synchonise les attributs visuels sur chaque machine.
		this.networkView.RPC("SetPlayerAttributes",RPCMode.AllBuffered,Network.player,this.m_MyPlayer.networkView.viewID,new Vector3(this.m_MyColor.x,this.m_MyColor.y,this.m_MyColor.z));
	}

	private void GreetNewPlayer(NetworkPlayer p_Player)
	{
		//Send him the server's name (this will trigger a response from the new player : he will send his local name back)
		this.networkView.RPC("RPC_SyncServerInfo",p_Player,this.m_ServerName);
		//Send Every Player's Names and colors Known.The Buffer should aready have made him know the players.
		foreach (PlayerInfos pi in this.m_PlayerInfos.Values)
		{
			this.networkView.RPC("RPC_SetPlayerName",p_Player,pi.m_AssociatedPlayer,pi.m_Name);
			this.networkView.RPC("RPC_SetLobbyColor",p_Player,pi.m_AssociatedPlayer,new Vector3(pi.m_Color.r,pi.m_Color.g,pi.m_Color.b));
		}
		//Notifies everyone of his existence.(This statement SHOULD arrive at other connections BEFORE the new player tells his name). The opposite will make the player nammed "Player".
		this.networkView.RPC("RPC_NewPlayerHasConnected",RPCMode.AllBuffered,p_Player);
	}



	//RPCS
	
	[RPC]
	/// <summary>
	/// Sets the player attributes.
	/// </summary>
	/// <param name='p_Player'>
	/// Target player.
	/// </param>
	/// <param name='p_viewID'>
	/// ViewID of the player's PlayerMotor.
	/// </param>
	/// <param name='p_MainColor'>
	/// Color of the Player.
	/// </param>
	/// <param name='p_Class'>
	/// Class number of the player.
	/// </param>
	public void SetPlayerAttributes(NetworkPlayer p_Player ,NetworkViewID p_viewID, Vector3 p_MainColor)
	{
		GameObject PlayerObject = NetworkView.Find(p_viewID).gameObject;
		PlayerMotor playerMotor = (PlayerMotor)PlayerObject.GetComponent(typeof(PlayerMotor));
		playerMotor.m_AssociatedPlayer=p_Player;
		if (p_Player != Network.player) 
		{
			playerMotor.DestroyUI ();
		}
		foreach(Renderer r in PlayerObject.GetComponentsInChildren(typeof(Renderer)))
		{
			r.material.SetColor("_Color",new Color(p_MainColor.x,p_MainColor.y,p_MainColor.z,1));
		}
		if(this.m_LobbyView!=null)
		{
			this.m_LobbyView.updatePlayerLabels();
		}
	}
	
	[RPC]
	public void EndGame()
	{
		this.m_PlayerInfos.Clear();
		this.m_PlayerPrefab=null;
		if(EV.gameManager!=null)
		{
			Destroy(EV.gameManager.gameObject);
		}
		this.m_GameManagerPrefab=null;
		this.m_MyPlayer=null;
		this.m_RefreshingHosts = false;
		this.m_BusyTimer = 0f;
		this.m_IsInGame = false;
		this.m_ShowMenu = false;
		this.m_HostData = new HostData[0];
		Application.LoadLevel("Lobby");
	}

	[RPC]
	public void RPC_SyncServerInfo(string p_ServerName)
	{
		this.m_ServerName = p_ServerName;
		this.networkView.RPC("RPC_SetPlayerName",RPCMode.AllBuffered,Network.player,this.m_MyName);
	}
	
	[RPC]
	public void RPC_SetLobbyColor(NetworkPlayer p_Player,Vector3 p_Color)
	{
		Color recontructedColor = new Color (p_Color.x,p_Color.y,p_Color.z,1);
		if(this.m_PlayerInfos.ContainsKey(p_Player))
		{
			this.m_PlayerInfos[p_Player].m_Color=recontructedColor;
		}
		else
		{
			Debug.LogWarning("The player does not exist");
		}
		if(this.m_LobbyView!=null)
		{
			this.m_LobbyView.updatePlayerLabels();
		}
	}
	
	
	[RPC]
	public void RPC_NewPlayerHasConnected (NetworkPlayer p_NetworkPlayer)
	{
		if(!this.m_PlayerInfos.ContainsKey(p_NetworkPlayer))
		{
			this.m_PlayerInfos.Add(p_NetworkPlayer,new PlayerInfos(p_NetworkPlayer,"Player"));
		}
		if(p_NetworkPlayer==Network.player)
		{
			this.m_PlayerInfos[p_NetworkPlayer].m_Name=this.m_MyName;
		}
		if(this.m_LobbyView!=null)
		{
			this.m_LobbyView.updatePlayerLabels();
		}
	}
	
	[RPC]
	public void RPC_SetPlayerName (NetworkPlayer p_NetworkPlayer, string p_Name)
	{
		if(!this.m_PlayerInfos.ContainsKey(p_NetworkPlayer))
		{
			Debug.LogWarning("Trying to assign a name to an unconnected player.Creating it");
			this.RPC_NewPlayerHasConnected(p_NetworkPlayer);
		}
		this.m_PlayerInfos[p_NetworkPlayer].m_Name=p_Name;
		if(this.m_LobbyView!=null)
		{
			this.m_LobbyView.updatePlayerLabels();
		}
	}
	
	[RPC]
	public void RPC_NewPlayerHasDisconnected (NetworkPlayer p_NetworkPlayer)
	{
		Destroy (this.m_PlayerInfos [p_NetworkPlayer].m_PlayerMotor.gameObject);
		this.m_PlayerInfos.Remove(p_NetworkPlayer);

		if(this.m_LobbyView!=null)
		{
			this.m_LobbyView.updatePlayerLabels();
		}
	}


}
