using UnityEngine;
using System.Collections;

public class VisionChangeSkill : Skill {

	public Color[] m_DayColors;
	private Color[] m_OldColor;
	
	public override void OnPurchaseExtras ()
	{
		if(this.m_DayColors.Length==EV.gameManager.m_DayColors.Length)
		{
			this.m_OldColor=EV.gameManager.m_DayColors;
			EV.gameManager.m_DayColors=this.m_DayColors;
		}
		else
		{
			Debug.LogError("Tried to assign new DayColors but the size does not match");
		}
	}
	
	public override void OnRespecializeExtras ()
	{
		EV.gameManager.m_DayColors=this.m_OldColor;
	}
}
