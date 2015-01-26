using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
	
	const int ZOMBIE_POS_TRIES = 10;
	const int ZOMBIE_SPAWN_MIN_DISTANCE = 5;
	const int ZOMBIE_SPAWN_COST = 120;
	const int PLAYER_VIEW_ANGLE = 40;//Amount of degrees from the center of the player views that will prevent zombie spawns.
	
	ArrayList m_MessageList;
	float m_MessageExpiration = 0f;
	int m_LastLevelPrefix=0;
	bool m_LoadingLevel = false;
	public GameObject m_StandardZombiePrefab;
	public bool m_KeepAllMessages=false;
	
	public float m_DayLightCycleTime = 30f;
	public float [] m_DayTimes;//TOTAL SHOULD NOT EXCEED 1.0f
	public Color [] m_DayColors; //Refers to m_DayTimes
	private Light m_Sun;
	private float m_StartTime = 0f;
	public Texture m_TextureDebug;
	public GameObject[] m_ThrowingPrefabs;
	
	//ItemLists
	public GameObject[] m_ILMedical;
	public GameObject[] m_ILAmmunition;
	public GameObject[] m_ILWeapon;
	public GameObject[] m_ILBuilding;
	
	//World Data
	public Zone[,] m_MapZones;
	private float m_Threat = 0;
	public int m_MaxZombies = 20;
	private int m_ZombieCount = 0;
	
	public class Zone
	{
		public enum ZoneType{Street,House,Lawn,Out};
		public ZoneType m_ZoneType;
		public GameObject[,] m_InnerZones;
		
		public Zone(ZoneType p_ZoneType)
		{
			this.m_ZoneType=p_ZoneType;
			this.m_InnerZones = new GameObject[20,20];
		}
		/// <summary>
		/// Remove the relation between the actual gameobject and the inner zone	
		/// </summary>
		public void DeleteInnerZone(Vector2 localPos)
		{
			this.m_InnerZones[(int)localPos.x,(int)localPos.y]=null;
		}
		/// <summary>
		/// Set the object of the an inner zone to p_GameObject	
		/// </summary>
		public void AffectInnerZone(GameObject p_GameObject,Vector2 localPos)
		{
			this.m_InnerZones[(int)localPos.x,(int)localPos.y]=p_GameObject;
		}
	}
	
	private class Message
	{
		public string m_Message;
		public Color m_Color;
		public Message(string p_message, Color p_color)
		{
			this.m_Message=p_message;
			this.m_Color=p_color;
		}
	}
	
	/// <summary>
	/// Adds global threat and spawn zombies when reaching ZOMBIE_SPAWN_COST
	/// </summary>
	/// <param name='p_Threat'>
	/// Threat to add.
	/// </param>
	public void addThreat(float p_Threat)
	{
		if(Network.isServer)
		{
			this.m_Threat+=p_Threat;
			if(this.ZombieCanSpawn())
			{
				this.SpawnZombie();
			}
		}
	}
	
	void Start()
	{
		this.m_MapZones=new Zone[5,5];
		this.m_MessageList=new ArrayList();
		EV.gameManager=this;
		DontDestroyOnLoad (this.gameObject);
	}
	
	void OnGUI()
	{
		//Show Messages
		int offset = 1;
		foreach(Message m in this.m_MessageList)
		{
			GUI.color=m.m_Color;
			GUI.Label(new Rect(120*EV.guiPixel,10*EV.guiPixel+(20*offset),400,20),m.m_Message);
			offset++;
		}
	}
	void Update()
	{
		this.FlushValidConsoleMessages ();
		this.DayLightCycle ();
	}

	//EVENTS
	
	void OnLevelWasLoaded(int level)
	{
		if (m_LoadingLevel)
		{
			m_LoadingLevel=false;
			Network.isMessageQueueRunning = true;
	        Network.SetSendingEnabled(0, true);
			GameObject[] gameObjects = (GameObject[])FindObjectsOfType(gameObject.GetType());
	        foreach (GameObject go in gameObjects)
			{
		       go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver); 
			}
		}
		if(level!=0)
		{
			EV.networkManager.InitializePlayer(Network.player);
			EV.networkManager.m_IsInGame=true;
			if(Network.isClient)
			{
				this.networkView.RPC("RPC_AskTime",RPCMode.All,Network.player);
			}
			else
			{
				this.m_StartTime=Time.time;
			}
		}
		
		if(Network.isServer)
		{
			TerrainGenerator Tg = (TerrainGenerator)this.GetComponent(typeof(TerrainGenerator));
			Tg.GenerateRoad();
			Tg.GenerateRoad();		
			for (int i = 0;i<5;i++)
			{
				for (int j = 0;j<5;j++)
				{
					if(this.m_MapZones[i,j]==null)
					{
						this.m_MapZones[i,j]=new Zone(Zone.ZoneType.Lawn);
					}
				}
			}
			Tg.FillWithHouses();
			foreach (GameObject g in EV.networkManager.m_TestSpawn)
			{
				Network.Instantiate(g,new Vector3(50,10,50),Quaternion.identity,0);
			}
		}
		this.m_Sun=GameObject.Find("Sun").light;
	}






	//PUBLIC

	public void InitializeNetworkLevelLoad(string level)
	{
		networkView.RPC( "LoadLevel", RPCMode.AllBuffered, level, m_LastLevelPrefix + 1);
	}

	/// <summary>
	/// Shows the message on the GUI.
	/// </summary>
	/// <param name='p_Message'>
	/// Message to be shown.
	/// </param>
	/// <param name='p_color'>
	/// Color of the message.
	/// </param>
	public void GUIMessage(string p_Message,Color p_color)
	{
		this.m_MessageExpiration = Time.time+3;
		this.m_MessageList.Add(new Message(p_Message,p_color));
	}
	
	public void signalZombieDeath()
	{
		this.m_ZombieCount--;
		if(this.m_ZombieCount<0)
		{
			Debug.LogError("Zombie Count is negative, More zombies died than the number spawned, something is wrong.");
		}
	}
	
	public int getInnerZoneNeigboringMatrix(int p_x,int p_y)
	{
		Vector2 direction = new Vector2(1,0);
		int matrix = 0;
		for (int i = 1;i<=8;i*=2)//On fait 4 fois la manoeuvre suivante (une fois par coté). le i est la valeur binaire de la position dans la matrice.
		{
			//Détermine la position vérifiée a partie du vecteur directionnel et la position actuelle.
			int x = (int)(p_x+direction.x);
			int y = (int)(p_y+direction.y);
			//Va chercher l'objet contenu a la position vérifiée selon le gamemanager.
			GameObject selectedInnerZone = this.getInnerZone(x,y);
			if(selectedInnerZone!=null)// Si il y a quelque chose.
			{
				Block selectedBlock = (Block)selectedInnerZone.GetComponent(typeof(Block));
				if(selectedBlock!=null)//Si c'est un block.
				{
					//On ajoute la valeur bineaire de la position à la matrice.
					matrix+=i;
				}
			}
			//Tourne le vecteur de 90 degrés.
			direction = new Vector2(direction.y,-direction.x);
		}
		return matrix;
	}

	/// <summary>
	/// Gets the inner zone with global world coordinates.
	/// </summary>
	/// <returns>
	/// The inner zone.
	/// </returns>
	/// <param name='p_WorldX'>
	/// P_ world x.
	/// </param>
	/// <param name='p_WorldY'>
	/// P_ world y.
	/// </param>
	public GameObject getInnerZone(int p_WorldX,int p_WorldY)
	{
		if(p_WorldX<0||p_WorldX>99||p_WorldY<0||p_WorldX>99)
		{
			return null;
		}
		else
		{
			return this.m_MapZones[Mathf.FloorToInt(p_WorldX/20),Mathf.FloorToInt(p_WorldY/20)].m_InnerZones[p_WorldX%20,p_WorldY%20];
		}
	}

	public void setInnerZone(int p_WorldX,int p_WorldY,GameObject p_Object)
	{
		this.m_MapZones[Mathf.FloorToInt(p_WorldX/20),Mathf.FloorToInt(p_WorldY/20)].m_InnerZones[p_WorldX%20,p_WorldY%20]=p_Object;
	}

	//Private

	/// <summary>
	/// Spawn a new Zombie
	/// </summary>
	private void SpawnZombie()
	{
			this.m_Threat-=ZOMBIE_SPAWN_COST;
			Vector3 pos; 
			bool valid;
			int tries = 0;
			do
			{//Tries to find a valid location away from players (10 tries).
				pos = new Vector3 (Random.value*100,0.66f,Random.value*100);
				valid = true;
				foreach(NetworkManager.PlayerInfos pi in EV.networkManager.m_PlayerInfos.Values)
				{
					//valid&=(pi.m_PlayerMotor.transform.position-pos).magnitude>ZOMBIE_SPAWN_PERIMETER;
					Transform playTrans = pi.m_PlayerMotor.transform;
					float angle =Mathf.Abs(Vector3.Angle(playTrans.TransformDirection(Vector3.forward),playTrans.position-pos));
					valid&=angle>PLAYER_VIEW_ANGLE;
					valid&=ZOMBIE_SPAWN_MIN_DISTANCE<=Vector3.Distance(playTrans.position,pos);
				}
				tries++;
			}while(!valid&&tries<ZOMBIE_POS_TRIES);
			
			if(tries<=ZOMBIE_POS_TRIES)
			{
				Network.Instantiate(m_StandardZombiePrefab,pos,Quaternion.identity,0);
				this.m_ZombieCount++;
			}
			else
			{
				Debug.LogWarning("Zombie Spawn Failed after "+ZOMBIE_POS_TRIES+" retries.");
			}
	}

	/// <summary>
	/// Simulate the Day-Light Cycle for a frame
	/// </summary>
	private void DayLightCycle()
	{
		if(this.m_Sun!=null)
		{
			//DayLight 
			float dayTime = ((Time.time-this.m_StartTime)%this.m_DayLightCycleTime)/this.m_DayLightCycleTime;
			this.m_Sun.transform.rotation=Quaternion.Euler(360*dayTime,0f,0f);
			for(int i = 0;i<this.m_DayTimes.Length;i++)
			{
				if(dayTime<this.m_DayTimes[i])
				{
					Color To;
					if(i==this.m_DayTimes.Length-1){To=this.m_DayColors[0];}else{To=this.m_DayColors[i+1];}
					Color result = Color.Lerp(this.m_DayColors[i],To,dayTime/this.m_DayTimes[i]);
					this.m_Sun.color = result;
					this.m_Sun.intensity=result.grayscale/3;
					RenderSettings.ambientLight=Color.Lerp(result,Color.black,0.5f);
					RenderSettings.fogColor=Color.Lerp(result,Color.black,0.8f);
					RenderSettings.skybox.SetColor("_Tint",Color.Lerp(result,Color.black,0.2f));
					i=this.m_DayTimes.Length;
				}
				else
				{
					dayTime-=this.m_DayTimes[i];
				}
			}
		}
	}

	/// <summary>
	/// Test if condidition for a new zombie are met	/// </summary>
	/// <returns><c>true</c>, if they are met, <c>false</c> otherwise.</returns>
	private bool ZombieCanSpawn()
	{
		return this.m_Threat >= ZOMBIE_SPAWN_COST && this.m_ZombieCount < this.m_MaxZombies;
	}

	/// <summary>
	/// Remove messages from console where requirements are met.
	/// </summary>
	private void FlushValidConsoleMessages()
	{
		//Delete Messages at expiration
		if(!this.m_KeepAllMessages)
		{
			if(Time.time>this.m_MessageExpiration&&this.m_MessageList.Count>0)
			{
				this.m_MessageList.RemoveAt(0);
				this.m_MessageExpiration=Time.time+(3/(this.m_MessageList.Count+1));
			}
		}
	}





	//Remote Procedure Calls
	[RPC]
	void LoadLevel (string level, int levelPrefix)
	{
	    m_LastLevelPrefix = levelPrefix;
        Network.SetSendingEnabled(0, false);    
        Network.isMessageQueueRunning = false;
        Network.SetLevelPrefix(levelPrefix);
        Application.LoadLevel(level);
		m_LoadingLevel=true;
	}
	
	[RPC]
	public void RPC_NewPlayer(NetworkViewID p_playerID,NetworkPlayer p_Player)
	{
		NetworkView nv = NetworkView.Find(p_playerID);
		PlayerMotor ps = (PlayerMotor)nv.GetComponent(typeof(PlayerMotor));
		EV.networkManager.m_PlayerInfos[p_Player].m_PlayerMotor=ps;
	}
	
	[RPC]
	public void PlayerOut(NetworkPlayer player)
	{
		if(Network.isServer)
		{
			Network.Destroy(EV.networkManager.m_PlayerInfos[player].m_PlayerMotor.gameObject);
			EV.networkManager.m_PlayerInfos.Remove(player);
			if(EV.networkManager.m_PlayerInfos.Count==0)
			{
				this.networkView.RPC("MessageBroadcast",RPCMode.All,"No Survivors Remaining, Game Over.");
				EV.networkManager.networkView.RPC("EndGame",RPCMode.All);
			}
		}
	}
	
	[RPC]
	void MessageBroadcast (string p_Message)
	{
		this.GUIMessage(p_Message,Color.magenta);
	}
	
	[RPC]
	public void RPC_SendTime(float p_Time)
	{
		if(Network.isClient)
		{//Transform his start Time to be the same day and time than the recieved value based on this own time and DayLightCycle(The daylightCycle should be the same).
			this.m_StartTime=Time.time-(p_Time*this.m_DayLightCycleTime);
		}
	}

	[RPC]
	public void RPC_AskTime(NetworkPlayer p_Player)
	{
		if(Network.isServer)
		{//Send the current dayTime value (Between 0 and 1)
			this.networkView.RPC("RPC_SendTime",p_Player,(Time.time-this.m_StartTime)/this.m_DayLightCycleTime);
		}
	}

}
