using UnityEngine;
using System.Collections;

public class QuickSlotButton : MonoBehaviour 
{
	[SerializeField] private int m_Number;
	public void click()
	{
		PlayerUI.m_QuickSlotManager.useQuickSlot (this.m_Number);
	}
}
