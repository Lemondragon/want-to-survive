using UnityEngine;
using System.Collections;

public class Zombie : Life {
	
	const float PERCEPTION_THRESOLD = 1f;
	const int RETARGET_TICK_TIMER = 1;
	const float MAX_ATTACK_POWER = 10;
	const float ZOMBIE_FOCUS_GAIN_PER_POWER = 10;
	const float THREAT_GAIN_PER_MELEE_DAMAGE = 2;
	
	public GameObject m_Body;
	Transform m_Target;
	Vector3 m_AltTarget = new Vector3(0,0,0);
	float m_RetargetTimer = 0;
	float m_Speed = 1f;
	float m_NextHitTick = 0;
	public Animation[] m_Hands;
	
	// Update is called once per frame
	void Start()
	{
		if(Network.isServer)
		{
			this.Retarget();
		}
	}
	
	public override void OnLive () 
	{
		base.OnLive();
		
		if(!this.m_Hands[0].isPlaying)
		{
			this.playHandAnim("hand_ZomIdle");
		}
	//	m_Body.animation["Body_Walk"].speed= this.rigidbody.velocity.magnitude/3;
		Quaternion LastRot = this.transform.rotation;
		if(m_Target!=null)
		{
			this.transform.LookAt(m_Target);
			this.transform.rotation = Quaternion.Lerp(LastRot,this.transform.rotation,3*Time.deltaTime);
			this.rigidbody.velocity=transform.forward*m_Speed;
		}
		else
		{
			if((this.transform.position-this.m_AltTarget).sqrMagnitude<10)
			{
				if(Network.isServer)
				{
					this.setNewRandomAltTarget();
				}
			}
			else
			{
				this.transform.LookAt(m_AltTarget);
				this.transform.rotation = Quaternion.Lerp(LastRot,this.transform.rotation,5*Time.deltaTime);
				this.rigidbody.velocity=transform.forward*m_Speed/2;
			}
		}
		if(Network.isServer)
		{
			if(Time.time>m_RetargetTimer)
			{
				this.Retarget();
				this.m_RetargetTimer=Time.time+RETARGET_TICK_TIMER;
			}
		}
	}
	
	public override void OnDeath()
	{
		if(Network.isServer && this.m_Dead)
		{
			EV.gameManager.signalZombieDeath();
			Network.Destroy(this.gameObject);
		}
	}
	/// <summary>
	/// Tries to select a new target within threat range.
	/// </summary>
	void Retarget()
	{
		NetworkPlayer closerPlayer = Network.player;
		
		float maxThreat = float.MinValue;
		foreach(NetworkManager.PlayerInfos pi in EV.networkManager.m_PlayerInfos.Values)
		{
			PlayerMotor p = pi.m_PlayerMotor;
			float detectedThreat = p.Threat/((p.transform.position-this.transform.position).magnitude);
			if(detectedThreat>=PERCEPTION_THRESOLD)
			{
				closerPlayer=pi.m_AssociatedPlayer;
				maxThreat=detectedThreat;
			}
		}
		if(maxThreat>=PERCEPTION_THRESOLD)
		{
			this.networkView.RPC("RPC_NewTarget",RPCMode.All,(EV.networkManager.m_PlayerInfos[closerPlayer].m_PlayerMotor.networkView.viewID));
		}
		else
		{
			Vector3 altTarget;
			if(this.m_Target!=null)
			{
				altTarget = this.m_Target.transform.position;
				this.networkView.RPC("RPC_NewAltTarget",RPCMode.All,altTarget);
			}
			else if(this.m_AltTarget == Vector3.zero)
			{
				this.setNewRandomAltTarget();
			}
		}
	}
	
	private void setNewRandomAltTarget()
	{
		this.networkView.RPC("RPC_NewAltTarget",RPCMode.All,new Vector3(Random.value*100,0,Random.value*100));
	}
	
	void OnCollisionStay(Collision p_collider)
	{
		if(!p_collider.gameObject.CompareTag("Zombie")&&Time.time>this.m_NextHitTick)
		{
			bool canDamageTarget = true;
			if(p_collider.gameObject.CompareTag("Player"))
			{
				if(this.m_Target!=p_collider.transform)
				{
					this.networkView.RPC("RPC_NewTarget",RPCMode.All,p_collider.gameObject.networkView.viewID);
				}
				if(p_collider.gameObject!=EV.networkManager.m_MyPlayer)
				{
					canDamageTarget=false;
				}
			}
			else if(!Network.isServer)
			{
				canDamageTarget=false;
			}
			if(canDamageTarget)
			{
				Health colHealth = p_collider.gameObject.GetComponent<Health>() as Health;
				if(colHealth!=null)
				{
					this.playHandAnim("hand_ZomSlam");
					float attackPower = Random.value*MAX_ATTACK_POWER;
					colHealth.networkView.RPC("AddBleed",RPCMode.AllBuffered,attackPower,this.networkView.viewID);
					this.m_NextHitTick=Time.time+1+Random.value;
					PlayerMotor colPlayer = colHealth as PlayerMotor;
					if(colPlayer!=null)
					{
						colPlayer.gainFocusExp(Focus.Zombie,attackPower*ZOMBIE_FOCUS_GAIN_PER_POWER);
					}
				}
			}
		}
	}
	
	public override void OnHit (bool p_isBleed, float p_amount, GameObject p_Source)
	{
		Weapon sourceWeapon = p_Source.GetComponent<Weapon>() as Weapon;
		if(sourceWeapon!=null)
		{
			Melee sourceMelee = sourceWeapon as Melee;
			if(sourceMelee!=null)
			{
				sourceMelee.m_Master.networkView.RPC("RPC_GenerateThreat",RPCMode.All,p_amount*THREAT_GAIN_PER_MELEE_DAMAGE);
			}
		}
	}
	
	private void playHandAnim(string p_Animation)
	{
		foreach(Animation a in this.m_Hands)
		{
//			a.Play(p_Animation);
		}
	}
	
	[RPC]
	void RPC_NewTarget (NetworkViewID viewID)
	{
		this.m_Target=NetworkView.Find(viewID).transform;
	}
	[RPC]
	void RPC_NewAltTarget (Vector3 target)
	{
		this.m_Target=null;
		this.m_AltTarget=target;
	}
}
