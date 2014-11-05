using UnityEngine;
using System.Collections;

public static class Bonus
{
	static int BONUS_LENGTH = 31;
	public enum BonusType  
	{
		MaxStamina,//dt
		MaxCommotion,//dt
		MaxBleed,//dt
		CommotionRegen,//d
		BleedRegen,//d
		StaminaRegen,//d
		BleedRate,//d
		WalkSpeed,//d
		RunSpeed,//dt
		FearEffect,//d	
		FearGain,//d
		BuildSpeed,//d
		BuildIntegrity,//d
		CraftCrittical, //NEED SYSTEM
		CraftSpeed,//NEED SYSTEM
		ReloadClipSpeed,//d
		SortBulletsSpeed,//d
		FillClipSpeed,//d
		WalkThreat,//d
		RunThreat,//d
		MeleeSpeed,//d
		MeleeCommotionDamage,//d
		MeleeBleedDamage,//d
		BackStabDamage,//NEED SYSTEM
		ThreatReductionRate,//d
		PistolDamage,//d
		RifleDamage,//d
		ShotgunDamage,//d
		ArrowDamage,//d
		MeleeDamage,//d
		FireArmDamage,//d
		BothHandsMeleeDamage //d
	}
	public static int getLength()
	{
		return BONUS_LENGTH;
	}
	public static string describe(BonusType p_Type, float p_Amount)
	{
		string message;
		if(p_Amount>0)
		{
			message="Increase ";
		}
		else
		{
			p_Amount*=-1;
			message="Decrease ";
		}
		switch(p_Type)
		{
		case BonusType.MaxStamina:
			message+="your maximum stamina by ";break;
		case BonusType.MaxCommotion:
			message+="your maximum commotion by ";break;
		case BonusType.MaxBleed:
			message+="your maximum bleed by ";break;
		case BonusType.ArrowDamage:
			message+="damage with arrows by ";break;
		case BonusType.BackStabDamage:
			message+="damage at backstabbing by ";break;
		case BonusType.BleedRate:
			message+="commotion gained from tears by ";break;
		case BonusType.BleedRegen:
			message+="bleed regeneration by ";break;
		case BonusType.StaminaRegen:
			message+="stamina regeneration by ";break;
		case BonusType.BuildIntegrity:
			message+="built structures integrity by ";break;
		case BonusType.BuildSpeed:
			message+="build speed by ";break;
		case BonusType.CommotionRegen:
			message+="commotion regeneration by ";break;
		case BonusType.CraftCrittical:
			message+="craft crittical chance by ";break;
		case BonusType.CraftSpeed:
			message+="craft speed by ";break;
		case BonusType.FearEffect:
			message+="fear effect on you by ";break;
		case BonusType.FearGain:
			message+="amount of fear gained by ";break;
		case BonusType.FillClipSpeed:
			message+="filling clip speed by ";break;
		case BonusType.MeleeBleedDamage:
			message+="bleed damage done with melee weapons by ";break;
		case BonusType.MeleeCommotionDamage:
			message+="commotion damage done with melee weapons by ";break;
		case BonusType.MeleeSpeed:
			message+="attack speed with melee weapons";break;
		case BonusType.PistolDamage:
			message+="damage done with pistols by ";break;
		case BonusType.RifleDamage:
			message+="damage done with rifles by ";break;
		case BonusType.ShotgunDamage:
			message+="damage done with shotguns by ";break;
		case BonusType.ReloadClipSpeed:
			message+="reload speed of firearms with clips by ";break;
		case BonusType.RunSpeed:
			message+="run speed by ";break;
		case BonusType.RunThreat:
			message+="threat generated from running by ";break;
		case BonusType.SortBulletsSpeed:
			message+="sorting bullets speed by ";break;
		case BonusType.ThreatReductionRate:
			message+="threat reduction rate by ";break;
		case BonusType.WalkSpeed:
			message+="walk speed by ";break;
		case BonusType.WalkThreat:
			message+="threat generated from walking by ";break;
		case BonusType.MeleeDamage:
			message+="damage done with melee weapons by ";break;
		case BonusType.FireArmDamage:
			message+="damage done with firearms by ";break;
		case BonusType.BothHandsMeleeDamage:
			message+="damage done with two-handed melee weapons by ";break;
		default:
			return "Description unavailable : ";
		}
		return message+p_Amount+"%.";
			
	}
}
