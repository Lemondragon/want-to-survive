using UnityEngine;
using System.Collections;

/// <summary>
/// Classe fille de Health destinée aux entitées mécaniques et structurelles.
/// </summary>
public class Integrity : Health 
{
	/// <summary>
	/// Instructions à être effectués à chaque frame.
	/// </summary>
	public override void OnLive ()
	{
		if(this.Bleed>0)
		{
			this.Commotion+=this.Bleed/5;
			this.Bleed=0;
		}
	}
	
	/// <summary>
	///Vérifie si la quantité de commotion actuelle pourrait avoir des conséquences.
	/// </summary>
	public override void CheckCommotion()
	{
		if(this.Commotion>=this.m_CommotionThresold)
		{
			m_Dead=true;
		}
		base.CheckCommotion();
	}
}
