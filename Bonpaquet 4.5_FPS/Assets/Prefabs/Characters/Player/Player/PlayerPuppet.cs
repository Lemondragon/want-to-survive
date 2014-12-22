using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Transform))]
public class PlayerPuppet : Puppet
{
	[SerializeField]
	private MeshRenderer m_Head;
	[SerializeField]
	private float m_TransitionSpeed = 5;
	[SerializeField]
	private float m_MaxWait = 2;
	[SerializeField]
	private float m_DistanceThresold = 0.2f;
	[SerializeField]
	private float m_RotationThresold = 5;
	private Quaternion m_LastRot;
	private Vector3 m_LastPos;
	private float m_LastPosUpdate;
	private float m_LastRotUpdate;

	private int updates = 0;

	protected override void OnStart()
	{
		this.m_LastRot=this.transform.rotation;
		this.m_LastPos=this.transform.position;
		this.m_LastPosUpdate=Time.time;
		this.m_LastRotUpdate=Time.time;
		if(!this.networkView.isMine)
		{
			this.rigidbody.isKinematic=true;
		}
		else
		{
			this.m_Head.enabled=false;
		}
	}
	protected override void AsPuppet()
	{
		if(this.m_LastPos!=this.transform.position)
		{
			this.transform.position=Vector3.Lerp(this.transform.position,this.m_LastPos,Time.deltaTime*this.m_TransitionSpeed);
		}
		if(this.m_LastRot!=this.transform.rotation)
		{
			this.transform.rotation=Quaternion.Lerp(this.transform.rotation,this.m_LastRot,Time.deltaTime*this.m_TransitionSpeed);
		}
	}
	protected override void AsMaster()
	{
		if(this.m_LastPos!=this.transform.position)
		{
			if(Time.time>=this.m_LastPosUpdate+(this.m_MaxWait/(Vector3.Distance(this.m_LastPos,this.transform.position)/this.m_DistanceThresold)))
			{
				this.updates++;
				networkView.RPC("RPC_SetPos",RPCMode.All,this.transform.position);
				this.m_LastPosUpdate=Time.time;
			}
		}

		if(this.m_LastRot!=this.transform.rotation)
		{
			if(Time.time>=this.m_LastRotUpdate+(this.m_MaxWait/(Quaternion.Angle(this.m_LastRot,this.transform.rotation)/this.m_RotationThresold)))
			{
				this.updates++;
				networkView.RPC("RPC_SetRot",RPCMode.All,this.transform.rotation);
				this.m_LastRotUpdate=Time.time;
			}
		}
	}

	[RPC]
	public void RPC_SetPos(Vector3 p_Pos)
	{
		this.m_LastPos=p_Pos;
	}

	[RPC]
	public void RPC_SetRot(Quaternion p_Rot)
	{
		this.m_LastRot=p_Rot;
	}

}
