using UnityEngine;
using System.Collections;



public class TerrainGenerator : MonoBehaviour
{
	public GameObject[] Streets;//0-Straight,1-Corner,2-Cross
	public GameObject[] Houses ;
	private void Generate(GameManager.Zone.ZoneType p_ZoneType,Vector2 worldPos)
	{
		switch(p_ZoneType)
		{
			case GameManager.Zone.ZoneType.Street://Générer une rue.
				this.VisualStreetUpdate(worldPos,false);
			if(worldPos.y<100)
			{
				this.VisualStreetUpdate(new Vector2(worldPos.x,worldPos.y+1),true);
			}
			if(worldPos.y>0)
			{
				this.VisualStreetUpdate(new Vector2(worldPos.x,worldPos.y-1),true);
			}
			if(worldPos.x<100)
			{
				this.VisualStreetUpdate(new Vector2(worldPos.x+1,worldPos.y),true);
			}
			if(worldPos.x>0)
			{
				this.VisualStreetUpdate(new Vector2(worldPos.x-1,worldPos.y),true);
			}
			break;
			case GameManager.Zone.ZoneType.House: //Générer une Maison.
				GameObject house = this.Houses[Mathf.FloorToInt(UnityEngine.Random.value*this.Houses.Length)];
				int angle = 0;
				int matrix = this.getNeigboringMatrix(worldPos,GameManager.Zone.ZoneType.Street);
				if(matrix!=0)
				{
					switch(matrix)
					{
						case 4:
						case 5:
						case 12:
						case 13:
							//West
							angle=90;
							break;
						case 1:
						case 9:
							//East
							angle = -90;
							break;
						case 8:
							//North
							angle = 180;
							break;				
					}
					Network.Instantiate(house,new Vector3(worldPos.x*20+10,0,worldPos.y*20+10),Quaternion.Euler(0,angle,0),2);
				}
			break;
		}
	}
	private void VisualStreetUpdate(Vector2 worldPos,bool UpdateOnly)
	{
		GameManager.Zone actualZone = this.whatZoneIsAt(worldPos);
		bool canRoadHere = true;
		if(!UpdateOnly||actualZone!=null)
		{
			if(actualZone!=null)
			{
				if(actualZone.m_ZoneType==GameManager.Zone.ZoneType.Out)
				{
					canRoadHere=false;
				}
				if(actualZone.m_ZoneType== GameManager.Zone.ZoneType.Street)
				{
					if (actualZone.m_InnerZones[0,0]!=null)
					{
						Network.Destroy(actualZone.m_InnerZones[0,0]);
					}
				}
			}
			if(canRoadHere)
			{
				int StreetType = 2;
				int StreetRotation = 0;
				switch(this.getNeigboringMatrix(worldPos,GameManager.Zone.ZoneType.Street))
				{
				case  0:
				case 1:
				case 4 :
				case 5 :
					//Horizontal
					StreetType=0;
					StreetRotation=90;
				break;
				case 2:
				case 8:
				case 10:
					//Vertical
					StreetType=0;
				break;
				case 3:
					//Right-Down Corner
					StreetType=1;
					StreetRotation=180;
				break;
				case 6:
					//Left-Down Corner
					StreetType=1;
					StreetRotation=-90;
				break;
				case 9:
					//Right-Up Corner
					StreetType=1;
					StreetRotation=90;
				break;
				case 12:
					//left-Up Corner
					StreetType=1;
				break;
				case 7:
					//South T
					StreetType=3;
					StreetRotation=90;
				break;
				case 11:
					//East T
					StreetType=3;
				break;
				case 13:
					//North T
					StreetType=3;
					StreetRotation=-90;
				break;
				case 14:
					//West T
					StreetType=3;
					StreetRotation=180;
				break;
				}
				if(actualZone==null)
				{
					this.gameObject.networkView.RPC("RPC_AssignZone",RPCMode.OthersBuffered,"Street",(int)worldPos.x,(int)worldPos.y);
					this.RPC_AssignZone("Street",(int)worldPos.x,(int)worldPos.y);
					actualZone=this.whatZoneIsAt(new Vector2(worldPos.x,worldPos.y));
				}
				actualZone.m_InnerZones[0,0]=(GameObject)Network.Instantiate(Streets[StreetType],new Vector3(worldPos.x*20+10,0,worldPos.y*20+10),Quaternion.Euler(0,StreetRotation,0),2);
			}
		}
	}
	public void GenerateRoad()
	{
		Vector2 Cursor = new Vector2(0,0);
		float CurveChance = 0;
		Vector2 Direction = new Vector2(0,0);
		//Initial CursorPosition
		if(Random.value>0.5f)
		{
			Cursor.x=Mathf.Floor(Random.value*3)+1;
			if(Random.value>0.5f)
			{//From North
				Cursor.y=4;
				Direction = new Vector2(0,-1);
			}
			else
			{//From South
				Direction = new Vector2(0,1);
			}
		}
		else
		{
			Cursor.y=Mathf.Floor(Random.value*3)+1;
			if(Random.value>0.5f)
			{//Form East
				Cursor.x=4;
				Direction = new Vector2(-1,0);
			}
			else
			{//From West
				Direction = new Vector2(1,0);
			}
		}
		//Generating...
		Rect WorldRect = new Rect(0,0,5,5);
		while(WorldRect.Contains(Cursor))
		{
			this.Generate(GameManager.Zone.ZoneType.Street,Cursor);
			if(Random.value<CurveChance)
			{
				CurveChance=0;
				if(Random.value>0.5)
				{
					Direction= new Vector2(Direction.y,-Direction.x);
				}
				else
				{
					Direction= new Vector2(-Direction.y,Direction.x);
				}
			}
			CurveChance+=(0.5f-CurveChance)/10;
			Cursor+=Direction;
		}
	}
	public void FillWithHouses()
	{
		for (int i = 0;i<5;i++)
		{
			for(int j = 0;j<5;j++)
			{
				if(EV.gameManager.m_MapZones[i,j].m_ZoneType==GameManager.Zone.ZoneType.Lawn)
				{
					this.Generate(GameManager.Zone.ZoneType.House,new Vector2(i,j));
					this.networkView.RPC("RPC_AssignZone",RPCMode.AllBuffered,"Lawn",i,j);
				}
			}
		}
	}
	public GameManager.Zone whatZoneIsAt(Vector2 worldPos)
	{
		Rect WorldRect = new Rect(0,0,5,5);
		if(!WorldRect.Contains(worldPos))
		{
			return new GameManager.Zone(GameManager.Zone.ZoneType.Out);
		}
		else
		{
			GameManager.Zone zoneToReturn = EV.gameManager.m_MapZones[(int)worldPos.x,(int)worldPos.y];
			if(zoneToReturn!=null)
			{
				return zoneToReturn;
			}
			else
			{
				return null;
			}
		}
	}
	/// <summary>
	/// Gets the neigboring matrix.
	/// this matrix's number means :
	/// 0-> No neighbors
	/// 1-> East Only
	/// 2-> South Only
	/// 3-> East and South
	/// 4-> West Only
	/// 5-> West and East
	/// 6-> West and South
	/// 7-> West, South and East
	/// 8-> North Only
	/// 9-> North and East
	/// 10->North and South
	/// 11->North, South and East
	/// 12->North and West
	/// 13->North, West and East
	/// 14->North,West and South
	/// 15->Every Direction.
	/// </summary>
	/// <returns>
	/// The neigboring matrix.
	/// </returns>
	/// <param name='worldPos'>
	/// World position.
	/// </param>
	/// <param name='p_ZoneTypeToFind'>
	/// P_ zone type to find.
	/// </param>
	private int getNeigboringMatrix(Vector2 worldPos,GameManager.Zone.ZoneType p_ZoneTypeToFind)
	{
		int matrixResult = 0;
		GameManager.Zone CheckingZone = whatZoneIsAt(new Vector2(worldPos.x,worldPos.y+1));
		if(CheckingZone!=null)
		{
			if(CheckingZone.m_ZoneType==p_ZoneTypeToFind)
			{
				matrixResult+=8;
			}
		}
		CheckingZone=whatZoneIsAt(new Vector2(worldPos.x-1,worldPos.y));
		if(CheckingZone!=null)
		{
			if(CheckingZone.m_ZoneType==p_ZoneTypeToFind)
			{
				matrixResult+=4;
			}
		}
		CheckingZone = whatZoneIsAt(new Vector2(worldPos.x,worldPos.y-1));
		if(CheckingZone!=null)
		{
			if(CheckingZone.m_ZoneType==p_ZoneTypeToFind)
			{
				matrixResult+=2;
			}
		}
		CheckingZone = whatZoneIsAt(new Vector2(worldPos.x+1,worldPos.y));
		if(CheckingZone!=null)
		{
			if(CheckingZone.m_ZoneType==p_ZoneTypeToFind)
			{
				matrixResult+=1;
			}
		}
		return matrixResult;
	}
	
	[RPC]
	public void RPC_Resize (NetworkViewID p_ObjectID, Vector3 Size)
	{
		Debug.Log("Resizing");
		GameObject obj = NetworkView.Find(p_ObjectID).gameObject;
		obj.transform.localScale=Size;
	}
	[RPC]
	public void RPC_AssignZone(string p_ZoneType,int worldPosX,int worldPosY)
	{
		GameManager.Zone.ZoneType zone = GameManager.Zone.ZoneType.Out;
		switch(p_ZoneType)
		{
		case "House":
			zone = GameManager.Zone.ZoneType.House;
			break;
		case "Lawn":
			zone = GameManager.Zone.ZoneType.Lawn;
			break;
		case "Street":
			zone= GameManager.Zone.ZoneType.Street;
			break;
		case "Out":
			zone = GameManager.Zone.ZoneType.Out;
			break;
		}
		EV.gameManager.m_MapZones[worldPosX,worldPosY]=new GameManager.Zone(zone);
	}
}
