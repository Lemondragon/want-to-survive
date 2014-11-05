using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour 
{
	public BlockSet blockSet;
	public bool ManualLink = false;
	
	/// <summary>
	/// Remplace ce bloc par la bonne version en fonction de ses voisins.
	/// </summary>
	public void VisualUpdate()
	{
		int x = (int)this.transform.position.x;
		int y = (int)this.transform.position.z;
		GameObject newBlock = blockSet.SpawnAppropriateBlock(EV.gameManager.getInnerZoneNeigboringMatrix(x,y),new Vector2(x,y));
		newBlock.networkView.RPC("AddToInnerZones",RPCMode.All);
		Network.Destroy(this.gameObject);
	}
	[RPC]
	/// <summary>
	/// Appelle VisualUpdate sur les blocs adjacents.
	/// </summary>
	public void Link()
	{
		if(Network.isServer)
		{
			Vector2 direction = new Vector2(1,0);
			for (int i = 0;i<4;i++)//On fait 4 fois la manoeuvre suivante (une fois par coté).
			{
				//Détermine la position vérifiée a partie du vecteur directionnel et la position actuelle.
				int x = Mathf.RoundToInt(this.transform.position.x+direction.x);
				int y = Mathf.RoundToInt(this.transform.position.z+direction.y);
				//Va chercher l'objet contenu a la position vérifiée selon le gamemanager.
				GameObject selectedInnerZone = EV.gameManager.getInnerZone(x,y);
				if(selectedInnerZone!=null)// Si il y a quelque chose.
				{
					Block selectedBlock = (Block)selectedInnerZone.GetComponent(typeof(Block));
					if(selectedBlock!=null)//Si c'est un block.
					{
						selectedBlock.VisualUpdate();//On le met a jour visuellement.
					}
				}
				//Tourne le vecteur de 90 degrés.
				direction = new Vector2(direction.y,-direction.x);
			}
		}
	}
	void Update()
	{
		if(ManualLink)
		{
			this.networkView.RPC("Link",RPCMode.All);
			this.ManualLink=false;
		}
	}
	
	[RPC]
	public void AddToInnerZones()
	{
		EV.gameManager.setInnerZone(Mathf.RoundToInt(this.transform.position.x),Mathf.RoundToInt(this.transform.position.z),this.gameObject);
	}
}
