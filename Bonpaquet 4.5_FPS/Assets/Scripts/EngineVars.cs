using UnityEngine;
using System.Collections;

public class EV 
{
	public static float guiPixel = Screen.width/160f;
	public static NetworkManager networkManager;
	public static GameManager gameManager;
	
	
	public enum ItemQuality:byte {Junk,Common,Uncommon,Rare,Unique};
	public enum AmmoType:byte {Pistol,Shotgun,Arrow,Rifle};
	
	/// <summary>
	/// Return the corresponding color for an ItemQuality.
	/// </summary>
	/// <returns>
	/// The color.
	/// </returns>
	/// <param name='p_Quality'>
	/// P_ quality.
	/// </param>
	public static Color QualityColor (ItemQuality p_Quality)
	{
		switch (p_Quality)
		{
			case ItemQuality.Junk:
				return new Color(0.5f,0.5f,0.5f);
			case ItemQuality.Common:
				return new Color(1f,1f,1f);
			case ItemQuality.Uncommon:
				return new Color(0.3f,1f,0.3f);
			case ItemQuality.Rare:
				return new Color(0.7f,0.7f,1f);
			case ItemQuality.Unique:
				return new Color(1f,0.7f,0.2f);
			default:
				return Color.red;
		}
	}
	/// <summary>
	/// Return the alternative(Bullets) name for an ItemQuality.
	/// </summary>
	/// <returns>
	/// The color.
	/// </returns>
	/// <param name='p_Quality'>
	/// P_ quality.
	/// </param>
	public static string QualityBulletName (ItemQuality p_Quality)
	{
		switch (p_Quality)
		{
			case ItemQuality.Junk:
				return "Unidentified";
			case ItemQuality.Common:
				return "Poor";
			case ItemQuality.Uncommon:
				return "Standard";
			case ItemQuality.Rare:
				return "High-Quality";
			case ItemQuality.Unique:
				return "Perfect";
			default:
				return "";
		}
	}
	
	public static Rect relativeRect(float p_left,float p_top,float p_width,float p_height)
	{
		return new Rect(p_left*guiPixel,p_top*guiPixel,p_width*guiPixel,p_height*guiPixel);
	}
}
