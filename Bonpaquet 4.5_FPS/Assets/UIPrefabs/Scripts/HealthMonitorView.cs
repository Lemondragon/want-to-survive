using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthMonitorView : MonoBehaviour,IObserver 
{
	[SerializeField]
	private Image[] m_ThreatIcons;
	[SerializeField]
	private Color[] m_ThreatColors;
	[SerializeField]
	private float m_TheatTopReference;
	[SerializeField]
	private float m_BarChangeSpeed = 10;
	[SerializeField]
	private Image[] m_TearIcons;
	[SerializeField]
	private Image m_BleedSpiral;
	[SerializeField]
	private Image m_CommotionDot;
	[SerializeField]
	private AudioClip m_GlassBreakSound;

	private  float m_BleedTarget;
	private  float m_CommotionTarget;
	private float m_ActualBleedProportion = 0;
	private float m_ActualCommotionProportion = 0;

	private  float m_BleedThresold= 30;
	private  float m_CommotionThresold= 50;

	public void update(ObserverMessages p_Message, object p_Argument)
	{
		switch (p_Message)
		{
		case ObserverMessages.BleedChanged:
			this.m_BleedTarget=(float) p_Argument;
			break;
		case ObserverMessages.BleedThresoldChanged:
			this.m_BleedThresold=(float) p_Argument;
			break;
		case ObserverMessages.CommotionChanged:
			this.m_CommotionTarget=(float) p_Argument;
			break;
		case ObserverMessages.CommotionThresoldChanged:
			this.m_CommotionThresold=(float) p_Argument;
			break;
		case ObserverMessages.FearChanged:
			break;
		case ObserverMessages.ThreatChanged:
			float threatStep = this.m_TheatTopReference/this.m_ThreatIcons.Length;
			float threat = (float) p_Argument;
			Color threatColor = this.m_ThreatColors[Mathf.FloorToInt(Mathf.Clamp(threat/threatStep,0,this.m_ThreatColors.Length-1))];
			for(int i = 0;i<this.m_ThreatIcons.Length;i++)
			{
				this.m_ThreatIcons[i].color=threatColor;
				this.m_ThreatIcons[i].enabled=i*threatStep<threat-1;
			}
			break;
		case ObserverMessages.TearsChanged:
			for(int i = 0;i<this.m_TearIcons.Length;i++)
			{
				if(i<(int) p_Argument)//Si on a cette Tear
				{
					if(this.m_TearIcons[i].enabled==false)
					{
						this.audio.PlayOneShot(this.m_GlassBreakSound);
						this.m_TearIcons[i].enabled=true;
					}
				}
				else
				{
					this.m_TearIcons[i].enabled=false;
				}
			}
			break;
		}
	}

	public void Update()
	{
		float increaseAmount = this.m_BarChangeSpeed * Time.deltaTime;
		if (this.m_ActualBleedProportion != this.m_BleedTarget) 
		{
			if(this.m_ActualBleedProportion+increaseAmount>this.m_BleedTarget)
			{
				this.m_ActualBleedProportion=this.m_BleedTarget;
			}
			else
			{
				this.m_ActualBleedProportion+=increaseAmount;
			}
			this.m_BleedSpiral.materialForRendering.SetFloat("_CutOff", Mathf.Lerp(1, 0.02f,this.m_ActualBleedProportion/this.m_BleedThresold));
		}
		if (this.m_ActualCommotionProportion != this.m_CommotionTarget) 
		{
			if(this.m_ActualCommotionProportion+increaseAmount>this.m_CommotionTarget)
			{
				this.m_ActualCommotionProportion = this.m_CommotionTarget;
			}
			else
			{
				this.m_ActualCommotionProportion+=increaseAmount;
			}
			this.m_CommotionDot.materialForRendering.SetFloat("_CutOff", Mathf.Lerp(1 ,0.02f ,this.m_ActualCommotionProportion/this.m_CommotionThresold));
		}
	}
}
