using UnityEngine;
using System.Collections;

public class Builder : Item {

	public GameObject m_SelectionCube; //Cube visuel pour montrer la sélection.
	public BlockSet[] m_BuiltSets; //Sets de cubes à fabriquer.
	public string[] m_BuiltSetsNames;
	public float[] m_BuildTime;
	private SelectionCube m_ShownCube;
	private bool m_ShowSelection = false;
	private int m_BuildType = 0;
	[HideInInspector]public bool m_LockBuildPos = false;
	
	public override void Start () 
	{
		for(int i = 0;i<this.m_BuiltSets.Length;i++)
		{
			this.m_PossibleActionNames.Add("Build "+this.m_BuiltSetsNames[i]);
			int newUsedInt = i;
			this.m_PossibleActions.Add(()=> this.Build(newUsedInt));
		}
		base.Start();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(this.m_ShowSelection)
		{
			if(!this.m_LockBuildPos)
			{
				Vector3 DirectPos =this.m_Master.transform.position-(this.m_Master.transform.forward*2);
				this.m_ShownCube.transform.position=new Vector3(Mathf.Round(DirectPos.x),DirectPos.y,Mathf.Round(DirectPos.z));
			}
			if(Input.GetMouseButtonDown(1))
			{
				if(this.m_ShownCube.ValidatePosition())
				{
					this.m_LockBuildPos=true;
					this.m_Master.StartAction(this.m_BuildTime[this.m_BuildType]/this.m_Master.getBonusMultiplier(Bonus.BonusType.BuildSpeed)
						,"Building "+this.m_BaseName,()=>this.Task_PlaceStructure(),true);
				}
				else
				{
					EV.gameManager.GUIMessage("You cannot build here.",Color.red);
				}
			}
		}
	}
	
	void Task_PlaceStructure()
	{
		this.m_LockBuildPos=false;
		if(this.m_ShownCube.ValidatePosition())
		{
			int roundedX =Mathf.RoundToInt(this.m_ShownCube.transform.position.x);
			int roundedY =Mathf.RoundToInt(this.m_ShownCube.transform.position.z);
			GameObject myNewCube=this.m_BuiltSets[this.m_BuildType].SpawnAppropriateBlock(EV.gameManager.getInnerZoneNeigboringMatrix(roundedX,roundedY),new Vector2(roundedX,roundedY));
			myNewCube.networkView.RPC("AddToInnerZones",RPCMode.All);
			myNewCube.GetComponent<Structure>().m_CommotionThresold*=this.m_Master.getBonusMultiplier(Bonus.BonusType.BuildIntegrity);
			((Block)myNewCube.GetComponent(typeof(Block))).networkView.RPC("Link",RPCMode.All);
			this.networkView.RPC("ChangeQuantity",RPCMode.AllBuffered,-1);
			this.m_ShownCube.ForceValidation();
			this.m_Master.gainFocusExp(Focus.Craft,10);
			if(this.m_ItemQuantity<=0)
			{
				this.EndBuild();
				Network.Destroy(this.gameObject);
			}
		}
		else
		{
			EV.gameManager.GUIMessage("You cannot build here.",Color.red);
		}
	}
	
	void Build(int p_BuildType)
	{
		if(this.m_Master.m_IsBuilding)
		{
			EV.gameManager.GUIMessage("You are already building something.",Color.red);
		}
		else
		{
			this.m_Master.m_ShowInventory=true;
			this.m_LockBuildPos=false;
			this.m_BuildType=p_BuildType;
			this.m_Master.m_IsBuilding=true;
			this.m_Master.m_Builder=this;
			this.m_ShowSelection=true;
			this.m_ShownCube=((SelectionCube)((GameObject)Instantiate(this.m_SelectionCube,Vector3.zero,Quaternion.identity)).GetComponent(typeof(SelectionCube)));
		}
	}
	public void EndBuild()
	{
		if(this.m_Master.m_IsBuilding)
		{
			this.m_Master.InteruptAction();
			this.m_Master.m_IsBuilding=false;
		}
		Destroy(this.m_ShownCube.gameObject);
		this.m_ShownCube=null;
		this.m_ShowSelection=false;
	}
	
	public override void Drop ()
	{
		this.EndBuild();
		base.Drop ();
	}
}
