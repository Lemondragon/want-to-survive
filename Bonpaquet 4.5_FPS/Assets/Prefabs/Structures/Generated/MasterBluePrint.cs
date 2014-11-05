using UnityEngine;
using System.Collections;

public class MasterBluePrint : MonoBehaviour 
{
	public BlockSet defaultBlockSet;
	public BluePrint m_DefaultBuilderBluePrint;
	public GameObject m_DefaultFloor;
	public GameObject m_DefaultRoof;
	public Color[] m_PossiblePresetRoofColors;
	
	[HideInInspector] public float spawnRating = 0.3f;
	void Start()
	{
		if(Network.isServer)
		{
			this.BuildAll();
		}
	}
	public void BuildAll()
	{
		//Active tous les spawners d'objets.
		foreach (ItemSpawner s in this.GetComponentsInChildren(typeof(ItemSpawner)))
		{
			s.Spawn();
		}
		//Active la construction de tous les blueprints.
		foreach (BluePrint b in this.GetComponentsInChildren(typeof(BluePrint)))
		{
			b.Build();
		}
		
		//Recolore tous les Toits
		Color col = Color.white;
		if(this.m_PossiblePresetRoofColors.Length>0)
		{
			col=this.m_PossiblePresetRoofColors[Mathf.FloorToInt(this.m_PossiblePresetRoofColors.Length*Random.value)];
		}
		else
		{
			col = new Color(1-(Random.value/2),1-(Random.value/2),1-(Random.value/2));
		}
		foreach (Ceilling c in this.GetComponentsInChildren(typeof(Ceilling)))
		{
			c.setColor(col);
		}
		this.networkView.RPC("DestroyMe",RPCMode.All);
	}
	
	[RPC]
	public void DestroyMe()
	{
		//Déparente tous les enfants directs.
		foreach(Transform c in this.GetComponentsInChildren(typeof(Transform)))
		{
			if(c.transform.parent=this.transform)
			{
				c.transform.parent=null;
			}
		}
		//Détruit cet objet.
		Destroy(this.gameObject);
	}
}
