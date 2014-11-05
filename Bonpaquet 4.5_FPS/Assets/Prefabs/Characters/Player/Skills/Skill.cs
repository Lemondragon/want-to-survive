using UnityEngine;
using System.Collections;

public class Skill : MonoBehaviour 
{
	[HideInInspector]public PlayerMotor m_Master;
	public bool m_Locked = false;
	public string m_SkillName;
	public string m_Tooltip;
	public Texture m_Logo;
	public EV.ItemQuality m_Tier;
	private SkillGroup m_Group;
	public int m_Cost = 0;
	public int m_MaxLevel = 1;
	public Bonus.BonusType[] m_BonusType;
	public int[] m_BonusAmount;
	public int m_Level;
	private bool m_Known;
	
	public void Init()
	{
		this.m_Level=0;
		this.m_Known=false;
	}
	
	public SkillGroup getGroup()
	{
		return this.m_Group;
	}
	
	public bool isLocked()
	{
		return this.m_Locked;
	}
	
	public bool isKnown()
	{
		return this.m_Known;
	}
	
	public void learn()
	{
		this.m_Known=true;
	}
	
	public int getLevel()
	{
		return this.m_Level;
	}
	public EV.ItemQuality getTier()
	{
		return this.m_Tier;
	}
	public void setSkillGroup(SkillGroup p_SkillGroup)
	{
		this.m_Group=p_SkillGroup;
	}
	
	public string describe()
	{
		string description = this.m_SkillName+" Rank("+this.m_Level+"/"+this.m_MaxLevel+")\n"+this.m_Tooltip;
		for(int i = 0;i<this.m_BonusType.Length;i++)
		{
			int levelPrevision = 1;
		if(this.isLocked()){levelPrevision=0;}
			description+="\n"+Bonus.describe(this.m_BonusType[i],this.m_BonusAmount[i]*(this.m_Level+levelPrevision));
		}
		description+="\n\n";
		if(this.m_Level==this.m_MaxLevel)
		{
			description+="Maxed.";
		}
		else if(!this.m_Locked)
		{
			description+="Cost "+this.m_Cost+" experience points";
		}
		return description;
	}
	
	public void OnPurchase()
	{
		if(!this.isLocked())
		{
			this.m_Level++;
			if(this.m_Level>=this.m_MaxLevel)
			{
				this.m_Locked=true;
			}
			this.m_Group.addExpSpent(this.m_Cost);
			for(int i = 0;i<this.m_BonusType.Length;i++)
			{
				this.m_Master.statChange(this.m_BonusType[i],this.m_BonusAmount[i]);
			}
			this.OnPurchaseExtras();
		}
	}
	
	public virtual void OnPurchaseExtras(){}
	
	public void OnRespecialize()
	{
		for(int i = 0;i<this.m_BonusType.Length;i++)
		{
			//this.m_Master.m_Bonuses[(int)this.m_BonusType[i]]-=this.m_BonusAmount[i];
		}
		this.OnRespecializeExtras();
	}
	
	public virtual void OnRespecializeExtras(){}
}
