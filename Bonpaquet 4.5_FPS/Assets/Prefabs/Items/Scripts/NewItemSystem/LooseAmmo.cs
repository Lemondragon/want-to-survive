using UnityEngine;
using System.Collections;

public class LooseAmmo : Item 
{
	public EV.AmmoType m_AmmoType;
	public Stack m_Bullets;
	
	public override void Pickup (PlayerMotor p_Master)
	{
		foreach(byte b in this.m_Bullets)
		{
			p_Master.m_BulletSupplies[m_AmmoType].supplies[EV.ItemQuality.Junk].Push(b);
		}
		Network.Destroy(this.gameObject);
	}
}
