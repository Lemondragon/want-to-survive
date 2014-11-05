using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SkillTree : MonoBehaviour
{
	private Skill[] m_Skills;
	private PlayerMotor m_Master;
	public float m_ExpGainInterval = 30;
	private float m_nextExpGain = 0;
	public float[] m_SkillsX;
	public float[] m_SkillsY;
	public Texture m_Background;
	public Texture m_PurchasedSkill;
	public Texture m_UnpurchasedSkill;
	public int m_ExperiencePoints ;
	private float[] m_FocusExp ;
	public bool m_UseExtendedSkills = true;
	public Skill[] m_ZombieSkills;
	public Skill[] m_ExtendedZombieSkills;
	public Skill[] m_MadnessSkills;
	public Skill[] m_ExtendedMadnessSkills;
	public Skill[] m_CowardSkills;
	public Skill[] m_ExtendedCowardSkills;
	public Skill[] m_AthleticsSkills;
	public Skill[] m_ExtendedAthleticsSkills;
	public Skill[] m_MeleeSkills;
	public Skill[] m_ExtendedMeleeSkills;
	public Skill[] m_FireArmsSkills;
	public Skill[] m_ExtendedFireArmsSkills;
	public Skill[] m_CraftSkills;
	public Skill[] m_ExtendedCraftSkills;
	public Skill m_StarterSkill;
	public Skill m_UnKnownSkill;
	
	private Skill[][] m_SkillPools = new Skill[][] {new Skill[0],new Skill[0],new Skill[0],new Skill[0],new Skill[0],new Skill[0],new Skill[0]};
	
	public void Init(PlayerMotor p_Master)
	{
		this.m_Master=p_Master;
		this.m_nextExpGain=Time.time+this.m_ExpGainInterval;
		this.m_FocusExp = new float[7];
		
		this.m_FocusExp[(int)Focus.Zombie]=5;
		
		this.m_Skills=new Skill[15];
		for(int i = 0;i<this.m_Skills.Length;i++)
		{
			this.m_Skills[i]=this.m_UnKnownSkill;
		}
		//Transfer Inspector Arrays into private Arrays.
		this.transferSkillsFromInspector(Focus.Zombie,m_ZombieSkills,m_ExtendedZombieSkills);
		this.transferSkillsFromInspector(Focus.Madness,m_MadnessSkills,m_ExtendedMadnessSkills);
		this.transferSkillsFromInspector(Focus.Coward,m_CowardSkills,m_ExtendedCowardSkills);
		this.transferSkillsFromInspector(Focus.Athletics,m_AthleticsSkills,m_ExtendedAthleticsSkills);
		this.transferSkillsFromInspector(Focus.Melee,m_MeleeSkills,m_ExtendedMeleeSkills);
		this.transferSkillsFromInspector(Focus.Firearms,m_FireArmsSkills,m_ExtendedFireArmsSkills);
		this.transferSkillsFromInspector(Focus.Craft,m_CraftSkills,m_ExtendedCraftSkills);	

		for(int i = 0;i<this.m_SkillPools.Length;i++)
		{
			for(int j = 0;j<this.m_SkillPools[i].Length;j++)
			{
				this.m_SkillPools[i][j].Init();
			}
		}
		//Creating group hierarchy
		SkillGroup[] groups = new SkillGroup[9];
		groups[0]=new SkillGroup(EV.ItemQuality.Unique,  new int[]{14,13},   null,     this);
		groups[1]=new SkillGroup(EV.ItemQuality.Rare,    new int[]{12,11,10},groups[0],this);
		groups[2]=new SkillGroup(EV.ItemQuality.Rare,    new int[]{9,8},     groups[0],this);
		groups[3]=new SkillGroup(EV.ItemQuality.Uncommon,new int[]{7,6},     groups[1],this);
		groups[4]=new SkillGroup(EV.ItemQuality.Uncommon,new int[]{5,4,3},   groups[2],this);
		groups[5]=new SkillGroup(EV.ItemQuality.Common,  new int[]{2},       groups[3],this);
		groups[6]=new SkillGroup(EV.ItemQuality.Common,  new int[]{1},       groups[4],this);
		groups[7]=new SkillGroup(EV.ItemQuality.Junk,    new int[]{0},       groups[5],groups[6],this);
		
		this.m_UnKnownSkill.setSkillGroup(groups[7]);
		this.m_Skills[0]=Instantiate(this.m_StarterSkill) as Skill;
		this.m_Skills[0].setSkillGroup(groups[7]);
		this.m_Skills[0].m_Master=this.m_Master;
	}
	
	private void transferSkillsFromInspector(Focus p_Focus, Skill[] p_BaseSkills,Skill[] p_ExtendedSkills)
	{
		if(this.m_UseExtendedSkills)
		{
			Skill[] resultSkillPool = new Skill[p_BaseSkills.Length+p_ExtendedSkills.Length];
			p_BaseSkills.CopyTo(resultSkillPool,0);
			p_ExtendedSkills.CopyTo(resultSkillPool,p_BaseSkills.Length);
			this.m_SkillPools[(int)p_Focus]=resultSkillPool;
		}
		else
		{
			this.m_SkillPools[(int)p_Focus]=p_BaseSkills;
		}
	}
	
	public void Update()
	{
		if(Time.time>=this.m_nextExpGain)
		{
			EV.gameManager.GUIMessage("Gained Experience (Now :"+this.m_ExperiencePoints+" )",Color.green);
			this.m_ExperiencePoints++;
			this.m_nextExpGain=Time.time+this.m_ExpGainInterval;
		}
	}
	
	public Skill[] extractSkills(int p_Quantity,EV.ItemQuality p_Tier)
	{
		Skill[] extractedSkills = new Skill[p_Quantity];
		Focus[] extractedFocus = this.truncateFocus(p_Quantity);
		for(int i = 0;i<extractedSkills.Length;i++)
		{
			Skill[] skillpool = this.m_SkillPools[(int)extractedFocus[i]];
			int index = Mathf.FloorToInt(Random.value*skillpool.Length);
			int maxSeaches = 20;
			do
			{
				if(skillpool[index].getTier()==p_Tier  &&  !skillpool[index].isKnown())
				{
					extractedSkills[i]=skillpool[index];
					skillpool[index].learn();
				}
				index++;
				maxSeaches--;
				if(index==skillpool.Length)
				{
					index=0;
				}
			}while(extractedSkills[i]==null&&maxSeaches>0);
			if(maxSeaches<=0)
			{
				Debug.LogError("No Appropriate Skill found");
			}
		}
		Skill[] returnedSkills = new Skill[p_Quantity];
		for(int i = 0;i<extractedFocus.Length;i++)
		{
			returnedSkills[i]=Instantiate(extractedSkills[i]) as Skill;
			returnedSkills[i].m_Master=this.m_Master;
		}
		return returnedSkills;
	}
	
	public void setSkill(int p_Index,Skill p_Skill)
	{
		this.m_Skills[p_Index]=p_Skill;
	}
	
	public void gainFocusExp(Focus p_Focus,float p_Amount)
	{
		this.m_FocusExp[(int)p_Focus]+=p_Amount;
	}
	
	public void display(PlayerMotor p_Parent)
	{
		Event e = Event.current;
		GUI.color= Color.white;
		string infoToShow = "";
		GUI.DrawTexture(EV.relativeRect(50,0,75,90),m_Background,ScaleMode.StretchToFill);
		for(int i = 0;i<this.m_Skills.Length;i++)
		{
			Texture bgTexture = this.m_PurchasedSkill;
			Skill skill = this.m_Skills[i];
			Rect skillRect = EV.relativeRect(this.m_SkillsX[i],this.m_SkillsY[i],10,10);
			if(skillRect.Contains(e.mousePosition))
			{
				if(!skill.isLocked())
				{
					if(p_Parent.m_MouseClick)
					{
						p_Parent.m_MouseClick=false;
						if(this.m_ExperiencePoints>=skill.m_Cost)
						{
							if(skill.getLevel()==0)
							{
								EV.gameManager.networkView.RPC("MessageBroadcast",RPCMode.All,EV.networkManager.m_MyName+" purchased "+skill.m_SkillName+"!");
							}
							else
							{
								EV.gameManager.networkView.RPC("MessageBroadcast",RPCMode.All,EV.networkManager.m_MyName+" upgraded "+skill.m_SkillName+" at rank "+(skill.getLevel()+1)+"!");
							}
							this.m_ExperiencePoints-=skill.m_Cost;
							skill.OnPurchase();
						}
						else
						{
							EV.gameManager.GUIMessage("Not enough Experience points to gain "+skill.m_SkillName+".",Color.red);
						}
					}
				}
				infoToShow = skill.describe();
				
			}
			else if(skill.getLevel()==0)
			{
				bgTexture = this.m_UnpurchasedSkill;
			}
			GUI.color=skill.getGroup().getGroupColor();
			GUI.DrawTexture(skillRect,bgTexture);
			GUI.color=Color.white;
			GUI.DrawTexture(skillRect,skill.m_Logo);
		}
		if(infoToShow!="")
		{
			GUI.Box(new Rect(e.mousePosition.x,e.mousePosition.y,300,120),infoToShow);	
		}
	}
	
	private Focus[] truncateFocus(int p_Quantity)
	{
		Focus[] selectedFocus = new Focus[p_Quantity+1];
		Focus[] returnedFocus = new Focus[p_Quantity];
		Focus[] orderedFocus = this.getOrderedFocus();
		for(int i = 0;i<selectedFocus.Length;i++)
		{//On fabrique l'arborescence des meilleurs focus.
			selectedFocus[i]=orderedFocus[i];
		}
		for(int i = 0;i<returnedFocus.Length;i++)
		{//On rend l'exp de tous les focus sélectionnés égal au dernier-0.1.
			this.m_FocusExp[(int)selectedFocus[i]]=this.m_FocusExp[(int)selectedFocus[selectedFocus.Length-1]]-0.1f;
			returnedFocus[i]=selectedFocus[i];
		}
		return returnedFocus;
	}
	
	/// <summary>
	/// Gets a list of focus in the order of their EXP score.
	/// </summary>
	/// <returns>
	/// La list of all focus, in order.
	/// </returns>
	private Focus[] getOrderedFocus()
	{
		LinkedList<FocusPair> sortedPairs = new LinkedList<FocusPair>();
		for(int i =0;i<this.m_FocusExp.Length;i++)
		{
			FocusPair fp = new FocusPair((Focus)i,this.m_FocusExp[i]);
			LinkedListNode<FocusPair> comparedNode = sortedPairs.First;
			while(comparedNode!=null)
			{
				if(fp.compareTo(comparedNode.Value)!=-1)
				{
					sortedPairs.AddBefore(comparedNode,fp);
					break;
				}
				comparedNode=comparedNode.Next;
			}
			if(comparedNode==null)
			{
				sortedPairs.AddLast(fp);
			}
		}
		Focus[] orderedFocus = new Focus[this.m_FocusExp.Length];
		int index = 0;
		foreach (FocusPair fp in sortedPairs)
		{
			orderedFocus[index]=fp.getFocus();
			index++;
		}
		return orderedFocus;
	}
	
	private class FocusPair
	{
		private Focus m_Focus;
		private float m_amount;
		
		public FocusPair (Focus p_Focus,float p_Amount)
		{
			this.m_Focus=p_Focus;
			this.m_amount=p_Amount;
		}
		
		public Focus getFocus()
		{
			return this.m_Focus;
		}
		public int compareTo (FocusPair p_FocusPair)
		{
			if(this.m_amount<p_FocusPair.m_amount)
			{
				return -1;
			}
			else if(this.m_amount==p_FocusPair.m_amount)
			{
				return 0;
			}
			else
			{
				return 1;
			}
	}
}
}
