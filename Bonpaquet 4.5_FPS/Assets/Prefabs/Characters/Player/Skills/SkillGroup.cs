using UnityEngine;
using System.Collections;

public class SkillGroup 
{
	private EV.ItemQuality m_Tier;
	private int[] m_SkillsInGroup;
	private SkillGroup[] m_UnlockGroup;
	private SkillTree m_ParentSkillTree;
	private bool m_Locked = true;
	private int m_expSpent = 0;
	
	public SkillGroup (EV.ItemQuality p_Tier,int[] p_SkillsInGroup,SkillGroup p_UnlockGroup,SkillGroup p_UnlockGroup2,SkillTree p_ParentSkillTree)
	{
		this.m_Tier=p_Tier;
		this.m_SkillsInGroup=p_SkillsInGroup;
		this.m_UnlockGroup= new SkillGroup[2];
		this.m_UnlockGroup[0]=p_UnlockGroup;
		this.m_UnlockGroup[1]=p_UnlockGroup2;
		this.m_ParentSkillTree=p_ParentSkillTree;
	}
	public SkillGroup (EV.ItemQuality p_Tier,int[] p_SkillsInGroup,SkillGroup p_UnlockGroup,SkillTree p_ParentSkillTree)
	{
		this.m_Tier=p_Tier;
		this.m_SkillsInGroup=p_SkillsInGroup;
		this.m_UnlockGroup= new SkillGroup[1];
		this.m_UnlockGroup[0]=p_UnlockGroup;
		this.m_ParentSkillTree=p_ParentSkillTree;
	}
	
	public Color getGroupColor()
	{
		return EV.QualityColor(this.m_Tier);
	}
	
	public EV.ItemQuality getTier()
	{
		return this.m_Tier;
	}
	
	private void unlockNext()
	{
		if(this.m_UnlockGroup!=null)
		{
			foreach(SkillGroup sg in this.m_UnlockGroup)
			{
				sg.unlock();
			}
		}
	}
	
	public void addExpSpent(int p_XP)
	{
		this.m_expSpent+=p_XP;
		if(this.m_UnlockGroup[0]!=null)
		{
			if(this.m_UnlockGroup[0].m_Locked)
			{
				if(this.m_expSpent>=Mathf.Pow(2,(1+(int)this.m_Tier)))
				{
					this.unlockNext();
				}
			}
		}
	}
	
	private void unlock()
	{
		if(this.m_Locked)
		{
			Skill[] unlockedSkills = this.m_ParentSkillTree.extractSkills(this.m_SkillsInGroup.Length,this.m_Tier);
			for(int i = 0; i<this.m_SkillsInGroup.Length;i++)
			{
				unlockedSkills[i].setSkillGroup(this);
				this.m_ParentSkillTree.setSkill(this.m_SkillsInGroup[i],unlockedSkills[i]);
			}
			this.m_Locked=false;
		}
	}
	
}
