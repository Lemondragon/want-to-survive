using UnityEngine;
using System.Collections;

public class ItemSpawner : MonoBehaviour 
{
	public int m_MedicalRating = 1; 
	public int m_AmmunitionRating = 1;
	public int m_WeaponRating = 1;
	public int m_BuildingRating = 1;
	
	public void Spawn()
	{
		MasterBluePrint master = (MasterBluePrint)this.transform.parent.GetComponent(typeof(MasterBluePrint));
		if(master.spawnRating<UnityEngine.Random.value)//Vérifie si il apparait, le pourcentage de chances provient du parent.
		{
			master.spawnRating-=0.1f;//lorsqu'il apparait, on réduit le % de chances de 10%.
			float refRating = m_MedicalRating+m_AmmunitionRating+m_WeaponRating+m_BuildingRating;//Faire la somme de tous les ratings (pour établir la relativité).
			float processedRating=m_MedicalRating;
			float refScore = UnityEngine.Random.value;//Définir la valeur du type d'objet obtenu.
			float refQuality = Mathf.Pow(UnityEngine.Random.value*10,2)/100; //Définir le pourcentage de qualité (un plus haut poucentage est plus rare).
			GameObject spawn; //Déclarer l'objet a faire apparaitre.
			if(processedRating/refRating>refScore) //Définir si la valeur de l'objet tombe sur la portion accordée au médical.
			{
				spawn = EV.gameManager.m_ILMedical[(int)Mathf.Floor(refQuality*EV.gameManager.m_ILMedical.Length)];//Définir l'index dans la liste par rapport au poucentage de qualité (0 est commun, plus ça monte, plus c'est rare);
			}
			else
			{
				processedRating+=m_AmmunitionRating;//On augmente la valeur de référence de la portion précédente.
				if(processedRating/refRating>refScore)
				{
					spawn = EV.gameManager.m_ILAmmunition[Mathf.FloorToInt(refQuality*EV.gameManager.m_ILAmmunition.Length)];
				}
				else
				{
					processedRating+=m_WeaponRating;
					if(m_WeaponRating/refRating>refScore)
					{
						spawn = EV.gameManager.m_ILWeapon[Mathf.FloorToInt(refQuality*EV.gameManager.m_ILWeapon.Length)];
					}
					else
					{
						spawn = EV.gameManager.m_ILBuilding[Mathf.FloorToInt(refQuality*EV.gameManager.m_ILBuilding.Length)];
					}
				}
			}
			Network.Instantiate(spawn,this.transform.position,this.transform.rotation,2);//Apparaitre.
		}
		else
		{
			master.spawnRating+=0.05f;//Si il n'apparait pas on augmente les chances d'apparaitre de 5%.
		}
		Network.Destroy(this.gameObject);//Supprimer le spawner.
	}
}
