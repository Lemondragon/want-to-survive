using UnityEngine;
using System.Collections;

public abstract class Weapon : Equippable 
{
	public GameObject m_ImpactPrefab; //Objet servant d'impact.(ex: Balle).
	public float m_Power = 30f;//Scalaire de la puissance.
	public float m_BleedRatio=1f;//Pourcentage (0 a 1) de bleed contenu dans m_power, le reste étant de la commotion.
	public Transform m_Tip = null; //Position à partir du pivot vers l'avant que le m_impactPrefab apparaitra.
	
	/// <summary>
	/// This event is triggered when the Impact_Prefab hits a victim.
	/// </summary>
	/// <param name='p_Victim'>
	/// P_ victim.
	/// </param>
	abstract public void onHit(GameObject p_Victim,float p_Power);

}


