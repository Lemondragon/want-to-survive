using UnityEngine;
using System.Collections;

/// <summary>
/// Classe fille de Health destinée aux entitées de chair et de sang.
/// </summary>
public class Life : Health {

	[HideInInspector]public float permanentCommotion = 0f; //Quantité de commotion permanente.
	[HideInInspector]public int tears = 0; //Quantité de Tears.
	public float bleedThresold = 30f; //Quantité de bleed requise pour obtenir un tear.
	public float bleedingRate = 0.16f;//Quantité de commotion obtenue/seconde/tear.
	
	
	public override void OnLive ()
	{
		this.Commotion+=this.tears*Time.deltaTime*this.bleedingRate;
		
		if(this.Bleed>=this.bleedThresold)
		{
			this.Bleed-=this.bleedThresold;
			this.tears++;
			this.CheckTears();
		}
	}

	
	/// <summary>
	///Vérifie si la quantité de commotion actuelle pourrait avoir des conséquences.
	/// </summary
	public override void CheckCommotion()
	{
		if(this.permanentCommotion+this.Commotion>=this.m_CommotionThresold)
		{
			m_Dead=true;
		}
		base.CheckCommotion();
	}
	
	/// <summary>
	///Vérifie si la quantité de tears actuelle pourrait avoir des conséquences.
	/// </summary
	void CheckTears()
	{
		if(this.tears>=3)
		{
			this.m_Dead=true;
		}
	}
	
	/// <summary>
	/// Transforme toute la commotion actuelle en 20% de commotion permanente.
	/// </summary
	public void Rest()
	{
		this.permanentCommotion+=this.Commotion/5;
		this.Commotion=0;
	}
}
