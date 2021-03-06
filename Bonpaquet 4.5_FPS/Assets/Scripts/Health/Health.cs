using UnityEngine;
using System.Collections;

/// <summary>
/// Classe abstraite pouvant recevoir du bleed et de la commotion.
/// </summary>
public abstract class Health : MonoBehaviour 
{

	public AudioClip m_HitSound;
	[SerializeField]
	private GameObject m_ImpactEffect;
	private Observable m_Observable = new Observable();
	private float m_Bleed = 0f; //Quantité de Bleed.
	private float m_Commotion = 0f; //Quantité de Commotion.
	private float m_CommotionThresold = 50f; //Commotion maximale avant la mort.
	[HideInInspector]public bool m_Dead = false; //Indicateur d'état (vie/mort).

	protected float m_LastImpactPower=0;
	protected Vector3 m_LastImpactPosition;

	void Update () 
	{
		if(m_Dead)//N'update pas si il est mort.
		{
			if(this.networkView!=null && networkView.isMine)
			{
				this.networkView.RPC("RPC_Death",RPCMode.Others);
				this.RPC_Death();
			}
			else
			{
				this.RPC_Death();
			}
		}
		else
		{
			this.OnLive();
			this.m_LastImpactPower=0;
		}
	}

	public Observable getObservable()
	{
		return this.m_Observable;
	}

	public GameObject getImpactEffect()
	{
		return this.m_ImpactEffect;
	}
	
	/// <summary>
	/// //Interface pour des instructions dans l'update.
	/// </summary>
	public virtual void OnLive(){}
	public virtual void OnDeath(){}
	
	/// <summary>
	///Vérifie si la quantité de commotion actuelle pourrait avoir des conséquences.
	/// </summary>
	public virtual void CheckCommotion()
	{
		if(this.m_Commotion<0)
		{
			this.m_Commotion=0;
		}
	}
	
	public virtual void CheckBleed()
	{
		if(this.m_Bleed<0)
		{
			this.m_Bleed=0;
		}
	}
	
	public float Bleed
	{
		get
		{
			return this.m_Bleed;
		}
		set
		{
			this.m_Bleed=value;
			this.CheckBleed();
			this.getObservable().notify(ObserverMessages.BleedChanged,value);
		}
	}

	public float Commotion
	{
		get
		{
			return this.m_Commotion;
		}
		set
		{
			this.m_Commotion=value;
			this.CheckCommotion();
			this.getObservable().notify(ObserverMessages.CommotionChanged,value);
		}
	}

	public float CommotionThresold
	{
		get
		{
			return this.m_CommotionThresold;
		}
		set
		{
			this.m_CommotionThresold=value;
			this.CheckCommotion();
			this.getObservable().notify(ObserverMessages.CommotionThresoldChanged,value);
		}
	}
	public virtual void OnHit(bool p_isBleed,float p_amount,GameObject p_Source){}
	
	/// <summary>
	/// Ajoute/Retire du bleed.
	/// </summary>
	/// <param name='p_Bleed'>
	/// Quantité de bleed (une quantité négative soigne).
	/// </param>
	[RPC]
	public void AddBleed(float p_Bleed,NetworkViewID p_SourceID)
	{
		this.audio.PlayOneShot(this.m_HitSound);
		this.Bleed+=p_Bleed;
		this.OnHit(true,p_Bleed,NetworkView.Find(p_SourceID).gameObject);
	}
	/// <summary>
	/// Ajoute/Retire de la commotion.
	/// </summary>
	/// <param name='p_Commotion'>
	/// Commotion à ajouter (un nombre négatif soigne).
	/// </param>
	[RPC]
	public void AddCommotion(float p_Commotion,NetworkViewID p_SourceID)
	{
		this.audio.PlayOneShot(this.m_HitSound);
		this.Commotion+=p_Commotion;
		this.OnHit(false,p_Commotion,NetworkView.Find(p_SourceID).gameObject);
	}
	[RPC]
	public void SetImpact(float p_Power,Vector3 p_Position)
	{
		this.m_LastImpactPower = p_Power;
		this.m_LastImpactPosition = p_Position;
	}
	[RPC]
	public void RPC_Death()
	{
		this.OnDeath();
	}
}
