using UnityEngine;
using System.Collections;

public class BluePrint : MonoBehaviour 
{
	public BlockSet customBlockSet= null;
	/// <summary>
	/// Remplace le BluePrint par un mur correspondant du BlockSet. Si aucun BlockSet n'est spécifié, il prend celui de son parent.
	/// </summary>
	public void Build()
	{
		int roundedX = Mathf.RoundToInt(this.transform.position.x);
		int roundedY = Mathf.RoundToInt(this.transform.position.z);
		this.transform.position=new Vector3(roundedX,this.transform.position.y,roundedY);
		BlockSet usedBlockSet = customBlockSet;
		if(usedBlockSet==null)
		{
			usedBlockSet = ((MasterBluePrint)this.transform.parent.GetComponent(typeof(MasterBluePrint))).defaultBlockSet;
		}
		GameObject newBlock = usedBlockSet.SpawnAppropriateBlock(EV.gameManager.getInnerZoneNeigboringMatrix(roundedX,roundedY),new Vector2(roundedX,roundedY));
		newBlock.networkView.RPC("AddToInnerZones",RPCMode.All);
		((Block)newBlock.GetComponent(typeof(Block))).Link();
		Network.Destroy(this.gameObject);
	}
}
