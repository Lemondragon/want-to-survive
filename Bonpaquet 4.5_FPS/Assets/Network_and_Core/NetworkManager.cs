using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour 
{
	const float CAM_DISTANCE = 8;
	//Refs
	public LobbyView m_LobbyView;
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
	
	//Server Info
	public string m_ServerName = "Server";//Connected's Server Name.
	
	/// <summary>
	/// Class defining useful informations on fellow players.
	/// </summary>
	public class PlayerInfos
	{
		public PlayerInfos (NetworkPlayer p_Player,string p_Name)
		{
			this.m_AssociatedPlayer=p_Player;
			this.m_Name=p_Name;
		}
		public NetworkPlayer m_AssociatedPlayer;//NetworkPlayer associé aux informations, correspond à sa Key dans m_PlayerInfos de NetworkManager
		public string m_Name; //Nom du joueur.
		public Color m_Color = Color.gray; //Couleur du joueur.
		public PlayerMotor m_PlayerMotor; //PlayerMotor du joueur (null lorsque dans le lobby).
	}
	// Use this for initialization
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
		if(Input.GetButtonDown("MainMenu"))//Si on appuie sur le bouton pour afficher le menu, on le toggle on/off.
		{
			this.m_ShowMenu = !m_ShowMenu;
		}
	}

	public void disconnect()
	{
		if (Network.isServer)
		{
			MasterServer.UnregisterHost();
		}
		Network.Disconnect();
	}

	public void startGame()
	{
		EV.gameManager.InitializeNetworkLevelLoad("GrassLand");
	}

	public void kickPlayer(NetworkPlayer p_Player)
	{
		Network.CloseConnection(p_Player,true);
	}

	void OnGUI () 
	{
		if (m_IsInGame) //Lorsque en jeu, l'apport visuel du NetworkManager se limite au MainMenu.
		{
			if(this.m_ShowMenu)
			{//MAIN MENU GUI
				if(GUI.Button(EV.relativeRect(5,5,15,3),"Disconnect"))//Bouton pour se déconnecter du serveur.
				{
					this.disconnect();
				}
			}
		}
		else //Lorsqu'on est pas en jeu.
		{
			GUI.color=Color.white;
			GUI.Label(EV.relativeRect(10,70,50,4),this.m_ConsoleMessage);//On montre le m_ConsoleMessage.
			
			if(Network.isClient||Network.isServer)//Si on est connecté à un serveur (Dans le lobby).
			{//Lobby
				//Color Selection
				GUI.color=new Color(this.m_MyColor.x,this.m_MyColor.y,this.m_MyColor.z);
				GUI.Box(EV.relativeRect(74,5,50,14),"Color");
				float col_x  = GUI.HorizontalSlider(EV.relativeRect(80,9 ,40,3),this.m_MyColor.x,0.1f,0.9f);
				float col_y  = GUI.HorizontalSlider(EV.relativeRect(80,12,40,3),this.m_MyColor.y,0.1f,0.9f);
				float col_z  = GUI.HorizontalSlider(EV.relativeRect(80,15,40,3),this.m_MyColor.z,0.1f,0.9f);
				if(!this.m_MyColor.Equals(new Vector3(col_x,col_y,col_z)))//Si un changement de couleur s'est produit, on met a jour localement et via le réseau. 
				{
					this.m_MyColor=new Vector3(col_x,col_y,col_z);
					this.networkView.RPC("RPC_SetLobbyColor",RPCMode.All,Network.player,m_MyColor);
				}
				
				
				//Show Connected Players
				GUI.color=Color.white;
				GUI.Box(EV.relativeRect(5,5,30,30),"Lobby :"+this.m_ServerName);
				//On montre 4 emplacements disponibles (le counter monte jusqu'a 3 maximum).
				int counter = 0;
				string label;
				foreach(PlayerInfos pi in this.m_PlayerInfos.Values)//Pour tous les joueurs connus.
				{
					label = pi.m_Name;
					if (Network.isServer&&pi.m_AssociatedPlayer!=Network.player)//Le serveur peut expulser les joueurs (excepté lui-même).
					{
						GUI.color=Color.red;
						if(GUI.Button(EV.relativeRect(25,10+(5*counter),8,3),"Kick"))
						{
							this.kickPlayer(pi.m_AssociatedPlayer);
						}
					}
					GUI.color=pi.m_Color; //La couleur du nom et celle du joueur.
					GUI.Label(EV.relativeRect(10,10+(5*counter),15,3),label);//On montre le nom du joueur.
					counter++;
				}
				while (counter<4)//Si il n'y a pas assez de joueurs pour remplir les 4 emplacements. On montre une emplacement par défaut gris indiqué "Slot available".
				{
					GUI.color = Color.gray;
					label ="Slot Available";
					GUI.Label(EV.relativeRect(10,10+(5*counter),15,3),label);
					counter++;
				}
				
				if(Network.isServer)//Le serveur peut commencer la partie.
				{
					if(GUI.Button(EV.relativeRect(10,30,10,3),"Start"))
					{
						this.startGame();
					}
				}
			}
			else if (!Network.isClient&&!Network.isServer) //Si l'on est ni en jeu ni dans un lobby, on est a l'acceuil.
			{//HOME
				this.m_ServerName = GUI.TextField(EV.relativeRect(25,5,30,3),this.m_ServerName); //On permet la sélection d'un nom de serveur qui sera utilisé si on en démarre un.
				this.m_MyName = GUI.TextField(EV.relativeRect(25,10,30,3),this.m_MyName); //On permet la sélection d'un nom qui ne sera plus modifiable une fois connecté.
				if(GUI.Button(EV.relativeRect(5,5,15,3),"Start Server")) //On peut démarrer un serveur.
				{
					this.m_ConsoleMessage="Starting Server...";
					this.StartServer(m_ServerName);
				}
				if(GUI.Button(EV.relativeRect(5,10,15,3),"Refresh Hosts")) //On peut demander la liste des serveurs actuellement disponibles.
				{
					this.RefreshHostList();
				}
				
				int serverNumber = 0;
				foreach (HostData host in this.m_HostData)//On montre les serveurs trouvés.
				{
					if(GUI.Button(EV.relativeRect(25,15+(5*serverNumber),25,3),host.gameName+"("+host.connectedPlayers+"/"+host.playerLimit+")"))//Si l'on clique sur un, on s'y connecte.
					{
						Network.Connect(host);
					}
					serverNumber++;
				}
			}
		}

	}
	/// <summary>
	/// Refreshs the host list.
	/// </summary>
	public void RefreshHostList()
	{
		this.m_ConsoleMessage="Requesting Hosts List...";
		MasterServer.RequestHostList(this.GAMENAME);
		m_RefreshingHosts=true;
		this.m_BusyTimer=Time.time+10;
	}

	void OnServerInitialized()//Lorsque le serveur est initialisé.
	{
		//On crée le gameManager qui sera utilisé pour la partie.
		EV.gameManager = ((GameObject)Network.Instantiate(m_GameManagerPrefab,this.transform.position,Quaternion.identity,0)).GetComponent<GameManager>();
		//On indique la connection du serveur en tant que joueur.
		this.networkView.RPC("RPC_NewPlayerHasConnected",RPCMode.AllBuffered,Network.player);
		this.networkView.RPC("RPC_SetPlayerName",RPCMode.AllBuffered,Network.player,this.m_MyName);
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
			GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawn");
			Transform spawn = spawns[(int)Mathf.Floor(spawns.Length*Random.value)].transform;
			GameObject playerObject = (GameObject)Network.Instantiate(this.m_PlayerPrefab,spawn.position,spawn.rotation,0);
			if(Network.isServer) //Le serveur fait apparaitre les objets de départ à des fins de test.
			{
				int spnOffset = 0;
				foreach(GameObject g in this.m_TestSpawn)
				{
					Network.Instantiate(g,new Vector3(0,1+spnOffset,0),Quaternion.identity,1);
					spnOffset+=3;
				}
			}
			//On assigne les références possibles grâce à la création de ce nouveau joueur.
			this.m_MyPlayer=playerObject;
			//Signifie l'apparition de ce nouveau joueur.
			EV.gameManager.networkView.RPC("RPC_NewPlayer",RPCMode.All,playerObject.networkView.viewID,p_Player);
			//Synchonise les attributs visuels sur chaque machine.
			this.networkView.RPC("SetPlayerAttributes",RPCMode.AllBuffered,p_Player,playerObject.networkView.viewID,new Vector3(this.m_MyColor.x,this.m_MyColor.y,this.m_MyColor.z));
		}
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
			Debug.Log("New Player has connected");
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
	}
	
	void OnDisconnectedFromServer()
	{
		this.m_ConsoleMessage="Disconnected from Server.";
		this.EndGame();
	}
	
	void StartServer (string serverName)
	{
		Network.InitializeServer(3,25001,!Network.HavePublicAddress());
		MasterServer.RegisterHost(this.GAMENAME,serverName,"Private");
	}
	
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
		foreach(Renderer r in PlayerObject.GetComponentsInChildren(typeof(Renderer)))
		{
			r.material.SetColor("_Color",new Color(p_MainColor.x,p_MainColor.y,p_MainColor.z,1));
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
		this.m_MyColor = new Vector3(0.5f,0.5f,0.5f);
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
	}
	
	[RPC]
	public void RPC_NewPlayerHasDisconnected (NetworkPlayer p_NetworkPlayer)
	{
		this.m_PlayerInfos.Remove(p_NetworkPlayer);
	}


}
