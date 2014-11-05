using UnityEngine;
using System.Collections;

public class BlockSet : MonoBehaviour 
{
	public GameObject column;
	public GameObject deadEnd;
	public GameObject wall;
	public GameObject corner;
	public GameObject junction;
	public GameObject cross;
	
	public GameObject SpawnAppropriateBlock (int p_neighboringMatrix,Vector2 p_Pos)
	{
		GameObject appropriateBlock = null;
		int angle = 0;
		switch(p_neighboringMatrix)
		{
		case 0:
			appropriateBlock = this.column;
			break;
		case 1:
			appropriateBlock = this.deadEnd;
			angle=90;
			break;
		case 2:
			appropriateBlock = this.deadEnd;
			angle=180;
			break;
		case 4:
			appropriateBlock = this.deadEnd;
			angle=-90;
			break;
		case 8:
			appropriateBlock = this.deadEnd;
			break;
		case 3:
			appropriateBlock = this.corner;
			break;
		case 6:
			appropriateBlock = this.corner;
			angle=90;
			break;	
		case 12:
			appropriateBlock = this.corner;
			angle=180;
			break;
		case 9:
			appropriateBlock = this.corner;
			angle=-90;
			break;	
		case 5:
			appropriateBlock = this.wall;
			break;
		case 10:
			appropriateBlock=this.wall;
			angle=90;
			break;
		case 15:
			appropriateBlock=this.cross;
			break;
		case 14:
			appropriateBlock=this.junction;
			angle=180;
			break;
		case 13:
			appropriateBlock=this.junction;
			angle=-90;
			break;
		case 11:
			appropriateBlock=this.junction;
			break;
		case 7:
			appropriateBlock=this.junction;
			angle=90;
			break;
		}
		GameObject myNewObject =(GameObject)Network.Instantiate(appropriateBlock,new Vector3(p_Pos.x,0,p_Pos.y),Quaternion.Euler(0,angle,0),2);
		return myNewObject;
	}
}
