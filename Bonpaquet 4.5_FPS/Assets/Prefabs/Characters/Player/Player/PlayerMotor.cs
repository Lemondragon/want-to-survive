using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerMotor : Life 
{
	const float MOVE_THREAT_TICK_TIME = 2f;//Intervall between sent threat for moving. Caution, this multiplies the amout sent initially after not moving.
	const float RUN_FOCUS_GAIN = 4;//Per second
	const float WALK_FOCUS_GAIN = 0.3f;//Per second
	const float RUN_THREAT_GAIN = 20;//Per second
	const float WALK_THREAT_GAIN = 5;//Per second
	const float COWARD_THREAT_THRESOLD = 50; //Maximum threat to gain Coward Exp;
	const float COWARD_FOCUS_GAIN = 2; //Coward focus gain at 0 Threat per second.
	const float INTERACT_RANGE = 1.5f; //Range from privot where you can inteacte with objects.
	
	public RaycastHit m_CursorHit;
	[HideInInspector]public bool m_CursorHasHit;
	[HideInInspector]public NetworkPlayer m_AssociatedPlayer; //Joueur du réseau se servent de l'objet comme avatar.
	[HideInInspector]public Camera m_Cam; //Camera observant cet objet.
	public GameObject m_Body; //Objet visuel contenant les animations.
	public Transform m_Cursor;//Objet qui détermine ou le crusor est.
	public SkillTree m_SkillTreePrefab;
	[HideInInspector]public Inventory m_Inventory;
	public Canvas m_UI;
	public GameObject m_PlayerMenuView;
	public GameObject m_InGameOnlyUI;
	public InventoryView m_InventoryView;
	public ActionsView m_ActionSlotView;
	public HealthMonitorView m_HealthMonitorView;
	public int[] m_InventoryGUI; //Mesures pour l'affichage de l'inventaire.
	[HideInInspector]public bool m_ShowInventory = false;//Indicateur d'affichage de l'inventaire (Affiché/caché).
	[HideInInspector]private bool m_ShowBulletSupplies = true;
	[HideInInspector]private bool m_ShowSkillTree = true;
	[HideInInspector]public bool m_IsBuilding = false; //Indicateur du mode contruction (Construit/Ne construit pas).
	[HideInInspector]public Builder m_Builder;

	//Fear
	[HideInInspector]private float m_Fear = 0f; //Quantitée de peur affectant le joueur.
	private float m_BaseFear = 0f; //Quantité de peur cummulée.
	[HideInInspector]public float m_FearModifier = 0f; //Quantité de peur obtenue par seconde.
	

	[HideInInspector]public Equippable[] m_HeldInHands = new Equippable[2]; //Objets tenus dans les mains, 0 = gauche, 1 = droite.
	public Transform[] m_HandPositions; //Points où les objets tenus doivent se situer, 0 = gauche, 1 = droite.
	
	public Texture m_TextureEquipCadre;//Texture de cadre entourant un objet équippé dans l'enventaire.
	public Texture m_TextureBar; //Texture de barre.
	public Texture m_Vignette;
	
	//Speed
	public float m_WalkSpeed = 1.0f; //Vitesse de marche.
	public float m_RunSpeed = 1.0f; //Vitesse de course.
	
	//Stamina
	[HideInInspector]float m_Stamina; //Quantité de Stamina actuelle.
	private float m_MaxStamina = 30f; //Quantité de Stamina maximale (de BASE).
	public float m_StaminaRegeneration = 1f; //Régénération de Stamina par 2 secondes.
	private float m_NextBreath = 0f; //Temps à atteindre pour regagner de la Stamina.
	private float m_NextMoveSoundTick = 0f;
	
	public Dictionary<EV.AmmoType,BulletSupplies> m_BulletSupplies;//Dictionnaire de BulletSupplies pour chaque AmmoType.
	
	//Actions
	[HideInInspector]public Action m_CurrentAction; //Délégué appellé lors de la completion de l'action.
	[HideInInspector]private bool m_ActionRestrictMovement; //Indicateur pour savoir si le mouvement interrompt l'action (interrompt/n'interrompt pas)
	[HideInInspector]public float m_ActionIncrementer; //Progression/seconde en pourcent de l'action.
	[HideInInspector]private float m_ActionCompletion; //Poucentage d'action complété.
	[HideInInspector]public string m_ActionName; //Nom de l'action.
	[HideInInspector]public bool m_RepeatAction; //Indicateur si l'action se répète automatiquement lors de la complétion.
	
	private QuickSlotManager m_QuickSlotManager;
	[HideInInspector]public SkillTree m_SkillTree;

	
	//Stats
	[HideInInspector]public int[] m_Bonuses = new int[Bonus.getLength()];
	[HideInInspector]public float m_ThreatReductionSpeed = 5f;
	
	//Threat
	private float m_Threat = 1;
	private float m_UnsentThreat = 0;
	
	//Util
	[HideInInspector]public bool m_MouseClick = false;
	public float maxHeight = 1; //Hauteur maximale du pivot du joueur.
	

	
	/// <summary>
	/// Classe pour classer les différentes qualités de munitions pour le même ammoType.
	/// </summary>
	public class BulletSupplies
	{
		public Dictionary<EV.ItemQuality,Stack> supplies;//Dictionnaire de Stacks de munitions clasés selon les Qualités.
		public BulletSupplies()
		{
			this.supplies= new Dictionary<EV.ItemQuality, Stack>();
			this.supplies.Add(EV.ItemQuality.Junk,new Stack());
			this.supplies.Add(EV.ItemQuality.Common,new Stack());
			this.supplies.Add(EV.ItemQuality.Uncommon,new Stack());
			this.supplies.Add(EV.ItemQuality.Rare,new Stack());
		}
	}
	/// <summary>
	/// Classe une balle non-identifiée à la bonne place.
	/// </summary>
	public bool Sort(EV.AmmoType p_Type)
	{
		bool answer = false;
		BulletSupplies concernedBullets = (BulletSupplies)this.m_BulletSupplies[p_Type];
		if(concernedBullets.supplies[EV.ItemQuality.Junk].Count>0)
		{
			answer = true;
			byte bullet = (byte)((Stack)concernedBullets.supplies[EV.ItemQuality.Junk]).Pop();
			if(bullet<50)
			{
				EV.gameManager.GUIMessage("Identified a "+EV.QualityBulletName(EV.ItemQuality.Common)+" "+p_Type.ToString()+" bullet.",EV.QualityColor(EV.ItemQuality.Common));
				((Stack)concernedBullets.supplies[EV.ItemQuality.Common]).Push(bullet);
			}
			else if (bullet<80)
			{
				EV.gameManager.GUIMessage("Identified a "+EV.QualityBulletName(EV.ItemQuality.Uncommon)+" "+p_Type.ToString()+" bullet.",EV.QualityColor(EV.ItemQuality.Uncommon));
				((Stack)concernedBullets.supplies[EV.ItemQuality.Uncommon]).Push(bullet);
			}
			else
			{
				EV.gameManager.GUIMessage("Identified a "+EV.QualityBulletName(EV.ItemQuality.Rare)+" "+p_Type.ToString()+" bullet!",EV.QualityColor(EV.ItemQuality.Rare));
				((Stack)concernedBullets.supplies[EV.ItemQuality.Rare]).Push(bullet);
			}
			if(concernedBullets.supplies[EV.ItemQuality.Junk].Count<=0)
			{
				this.InteruptAction();
			}
		}
		else
		{
			EV.gameManager.GUIMessage("You have no more "+p_Type.ToString()+" bullets to sort.",Color.red);
			this.InteruptAction();
		}
		return answer;
	}




	//GETS / SETS / PROPERTIES

	public float getMaxStamina()
	{
		return this.m_MaxStamina*this.getBonusMultiplier(Bonus.BonusType.MaxStamina);
	}
	public float getStaminaRegen()
	{
		return this.m_StaminaRegeneration*this.getBonusMultiplier(Bonus.BonusType.StaminaRegen);
	}
	public float getWalkSpeed()
	{
		return this.m_WalkSpeed*this.getBonusMultiplier(Bonus.BonusType.WalkSpeed);
	}
	public float getRunSpeed()
	{
		return this.m_RunSpeed*this.getBonusMultiplier(Bonus.BonusType.RunSpeed);
	}
	public float Fear
	{
		get
		{
			return this.m_Fear;
		}
		set
		{
			this.m_Fear=value;
			this.getObservable().notify(ObserverMessages.FearChanged,value);
		}
	}

	public float ActionCompletion
	{
		get
		{
			return this.m_ActionCompletion;
		}
		set
		{
			this.m_ActionCompletion=value;
			this.getObservable().notify(ObserverMessages.ActionCompletionChanged,value);
		}
	}
	
	/// <summary>
	/// Gets or sets the threat.
	/// </summary>
	/// <value>
	/// The threat.
	/// </value>
	public float Threat
	{
		get
		{
			return this.m_Threat;
		}
		set
		{
			this.m_Threat=value;
			if(this.m_Threat<1)
			{
				this.m_Threat=1;
			}
			this.getObservable().notify(ObserverMessages.ThreatChanged,this.m_Threat);
		}
	}
		







	//PUBLIC

	/// <summary>
	/// Initialize this PlayerMotor
	/// </summary>
	public void Init()
	{
		if(this.networkView.isMine)
		{
			this.m_QuickSlotManager = this.gameObject.AddComponent<QuickSlotManager>();
			this.m_QuickSlotManager.getObservable().subscribe(this.m_ActionSlotView);
			this.getObservable().subscribe(this.m_QuickSlotManager);
			this.getObservable().subscribe(this.m_ActionSlotView);
			this.getObservable().subscribe(this.m_HealthMonitorView);
			PlayerUI.m_QuickSlotManager= this.m_QuickSlotManager;
			PlayerUI.m_PlayerMenuView= this.m_PlayerMenuView;
			PlayerUI.m_InventoryView= this.m_InventoryView;
			this.m_PlayerMenuView.SetActive(false);
			this.m_Inventory = new Inventory(this);
			this.m_Inventory.getObservable().subscribe(this.m_QuickSlotManager);
			this.m_InventoryView.setDisplayedInventory(this.m_Inventory);
			this.m_SkillTree=Instantiate(this.m_SkillTreePrefab) as SkillTree;
			this.m_SkillTree.Init(this);
			this.m_Stamina=this.getMaxStamina();//Débute avec la stamina au maximum.
			this.m_BulletSupplies=new Dictionary<EV.AmmoType, BulletSupplies>();
			this.m_BulletSupplies.Add(EV.AmmoType.Pistol,new BulletSupplies());
			this.m_BulletSupplies.Add(EV.AmmoType.Rifle,new BulletSupplies());
			this.m_BulletSupplies.Add(EV.AmmoType.Shotgun,new BulletSupplies());
			this.m_BulletSupplies.Add(EV.AmmoType.Arrow,new BulletSupplies());
			
			this.InteruptAction();
			//##TEMP
			for(int i=0;i<20f;i++)
			{
				this.TakeBullet(EV.AmmoType.Arrow,(byte)(UnityEngine.Random.value*100));
			}
			Screen.showCursor=false;
			this.Commotion=0;
			this.Bleed=0;
		}
		else
		{
			this.DestroyUI();
		}
	}

	/// <summary>
	/// Destroys the Player's UI
	/// </summary>
	public void DestroyUI()
	{
		Destroy (this.GetComponentInChildren<Camera> ().gameObject);
		Destroy (this.GetComponent<MouseLook> ());
		Destroy(this.m_PlayerMenuView);
		Destroy(this.m_UI.gameObject);
	}

	// Update is called once per frame
	public override void OnLive ()
	{
		if(this.networkView.isMine)
		{
			this.LockCursor();
			this.CorrectHeight();
			this.Move();
			this.CurrentAction();
			this.CursorInteraction();
			this.CheckInventoryButton();
			this.Regenerate();
			this.Frighten();
		}
		this.SimulateThreat();
		base.OnLive();
	}

	public void TakeItem (Item p_Item)
	{
		this.m_Inventory.addItem(p_Item);
	}
	
	//Menu Called
	public void Equip (Equippable p_Equippable)
	{
		int hand = 0;
		if(p_Equippable.m_IsTwoHanded)
		{//Equip as TwoHanded
			if(this.m_HeldInHands[0]!=null)
			{
				this.Unequip(this.m_HeldInHands[0]);
			}
			if(this.m_HeldInHands[1]!=null)
			{
				this.Unequip(this.m_HeldInHands[1]);
			}
			//hand=0;
		}
		else
		{//Equip as One handed
			if(this.m_HeldInHands[0]==null)
			{
				hand=0;
			}
			else if (this.m_HeldInHands[0].m_IsTwoHanded)
			{
				this.Unequip(this.m_HeldInHands[0]);
				//hand=0;
			}
			else if(this.m_HeldInHands[1]==null)
			{
				hand=1;
			}
			else
			{
				this.Unequip(this.m_HeldInHands[0]);
				//hand=0;
			}
		}
		p_Equippable.m_HeldByHand=hand;
		this.m_HeldInHands[hand]=p_Equippable;
		if(hand==0)
		{
			EV.gameManager.GUIMessage("Equipped :"+p_Equippable.FullName+" in main",Color.white);
		}
		else
		{
			EV.gameManager.GUIMessage("Equipped :"+p_Equippable.FullName+" in off hand",Color.white);
		}
		Transform temp_hand =this.m_HandPositions[p_Equippable.m_HeldByHand];
		p_Equippable.networkView.RPC("RPC_Equip",RPCMode.AllBuffered,temp_hand.networkView.viewID);
	}
	//Menu Called
	public void Unequip(Equippable p_Equippable)
	{
		if (p_Equippable.m_HeldByHand != -1) 
		{
			this.networkView.RPC ("PlayHandAction", RPCMode.All, p_Equippable.m_HeldByHand, "UnGrab");
			if (p_Equippable.m_IsTwoHanded) 
			{
					this.networkView.RPC ("PlayHandAction", RPCMode.All, 1, "UnGrab");
			}
			EV.gameManager.GUIMessage ("Unequipped :" + p_Equippable.FullName + ".", Color.white);
			this.m_HeldInHands [p_Equippable.m_HeldByHand] = null;
			p_Equippable.m_HeldByHand = -1;
			p_Equippable.Unequip();
			p_Equippable.networkView.RPC ("RPC_Unequip", RPCMode.AllBuffered);
		}
	}
	
	private void CheckStamina()
	{
		if(this.m_Stamina<0)
		{
			this.m_Stamina=0;
		}
		else if(this.m_Stamina>this.getMaxStamina())
		{
			this.m_Stamina=this.getMaxStamina();
		}
	}
	
	public void TakeBullet(EV.AmmoType p_AmmoType,byte p_Quality)
	{
		((Stack)((BulletSupplies)this.m_BulletSupplies[p_AmmoType]).supplies[EV.ItemQuality.Junk]).Push(p_Quality);
	}
	
	public void StartAction (float p_duration, string p_name, Action p_action,bool p_restrictMovement)
	{
		if(this.ActionCompletion==-1)
		{
			this.m_RepeatAction=false;
			this.m_ActionIncrementer=100/p_duration;
			this.ActionCompletion=0;
			this.m_ActionName=p_name;
			this.m_ActionRestrictMovement=p_restrictMovement;
			this.m_CurrentAction=p_action;
			if(p_restrictMovement)
			{
				this.rigidbody.velocity=Vector3.zero;
			}
			this.getObservable().notify(ObserverMessages.ActionNameChange,p_name);
		}
		else
		{
			EV.gameManager.GUIMessage("Action aborted: "+p_name+", Your current action is not completed",Color.red);
		}
	}
	public void InteruptAction()
	{
		this.m_RepeatAction=false;
		this.ActionCompletion=-1;
		this.m_CurrentAction=null;
		this.m_ActionName="";
		this.m_ActionIncrementer=0;
		if(this.m_IsBuilding)
		{
			this.m_Builder.m_LockBuildPos=false;
		}
	}
	

	
	public override void OnDeath ()
	{
		EV.gameManager.networkView.RPC("MessageBroadcast",RPCMode.All,"A Survivor has died.");
		EV.gameManager.networkView.RPC("PlayerOut",RPCMode.All,Network.player);
	}

	/// <summary>
	/// Generates threat at set intervalls (MOVE_THREAT_TICK_TIME).
	/// </summary>
	/// <param name='p_Threat'>
	/// Amount of threat generated per second.
	/// </param>
	private void GenerateMoveThreat (float p_Threat)
	{
		this.m_UnsentThreat+=p_Threat*Time.deltaTime;
		if(this.m_NextMoveSoundTick<=Time.time&&m_UnsentThreat>1)
		{
			this.m_NextMoveSoundTick=Time.time+MOVE_THREAT_TICK_TIME;
			this.networkView.RPC("RPC_GenerateThreat",RPCMode.All,this.m_UnsentThreat);
			this.m_UnsentThreat=0;
		}
	}
	private void CursorInteraction()
	{
//		this.m_ContextualActionName = "";
		this.m_CursorHasHit = Physics.Raycast(this.m_Cursor.position,this.m_Cursor.TransformDirection(Vector3.forward),out this.m_CursorHit,INTERACT_RANGE);
		if(this.m_CursorHasHit)
		{
			Item item = this.m_CursorHit.transform.GetComponent<Item>();
			if(item!=null)
			{
				//On affiche l'option de le prendre.
				//this.m_ContextualActionName = "Take "+item.m_ItemName;
				//this.m_ContextualActionColor= EV.QualityColor(item.m_ItemQuality);
				if(Input.GetButtonDown("Action"))
				{
					this.TakeItem(item);
				}
			}
		}
		
	}
	
	private void drawBar(Vector2 p_Anchor,float p_Thickness, float p_Max,float p_Actual,Color p_Color)
	{
		float WIDPAD = 1f;
		float HEIPAD = 0.2f;
		GUI.color = p_Color;	
		GUI.Box(EV.relativeRect(p_Anchor.x,p_Anchor.y,p_Max+(WIDPAD*2),p_Thickness),"");
		GUI.DrawTexture(EV.relativeRect(p_Anchor.x+WIDPAD,p_Anchor.y+HEIPAD,p_Actual,p_Thickness-(HEIPAD*2)),this.m_TextureBar,ScaleMode.StretchToFill);
	}
	private void drawBar(Vector2 p_Anchor,float p_Thickness, float p_Max,float p_Actual,float p_Overcharge,Color p_Color,Color p_OverColor)
	{
		float WIDPAD = 1f;
		float HEIPAD = 0.2f;
		this.drawBar(p_Anchor,p_Thickness,p_Max,p_Actual,p_Color);
		GUI.color=p_OverColor;
		GUI.DrawTexture(EV.relativeRect(p_Anchor.x+WIDPAD+p_Actual,p_Anchor.y+HEIPAD,p_Overcharge,p_Thickness-(HEIPAD*2)),this.m_TextureBar,ScaleMode.StretchToFill);
	}
	
	public float getBonusMultiplier(Bonus.BonusType p_Bonus)
	{
		return 1+(this.m_Bonuses[(int)p_Bonus]/100f);
	}
	
	public void statChange(Bonus.BonusType p_Bonus,float p_Amount)
	{
		this.m_Bonuses[(int)p_Bonus]+=(int)p_Amount;
		switch(p_Bonus)
		{
		case Bonus.BonusType.MaxBleed:
			this.BleedThresold=30*this.getBonusMultiplier(Bonus.BonusType.MaxBleed);
			break;
		case Bonus.BonusType.MaxCommotion:
			this.CommotionThresold=50*this.getBonusMultiplier(Bonus.BonusType.MaxCommotion);
			break;
		case Bonus.BonusType.BleedRate:
			this.bleedingRate=0.16f*this.getBonusMultiplier(Bonus.BonusType.BleedRate);
			break;
		}
	}

	private void LockCursor()
	{
		if(!this.m_ShowInventory)
		{
			Screen.lockCursor=true;
			Screen.showCursor=false;
		}
		else
		{
			Screen.showCursor=true;
			Screen.lockCursor=false;
		}
	}
	
	/// <summary>
	/// Raccourci vers Skilltree.gainFocusExp().
	/// </summary>
	/// <param name='p_Focus'>
	/// P_ focus.
	/// </param>
	/// <param name='p_Amount'>
	/// P_ amount.
	/// </param>
	public void gainFocusExp(Focus p_Focus,float p_Amount)
	{
		if(this.m_SkillTree!=null)
		{
			this.m_SkillTree.gainFocusExp(p_Focus,p_Amount);
		}
	}

	/// <summary>
	/// Regenerate pools (Bleed and Commotion) for a frame on this Player.
	/// </summary>
	private void Regenerate()
	{
		if(this.getBonusMultiplier(Bonus.BonusType.BleedRegen)!=0)
		{
			this.Bleed-=(this.getBonusMultiplier(Bonus.BonusType.BleedRegen)-1)*Time.deltaTime;
		}
		if(this.getBonusMultiplier(Bonus.BonusType.CommotionRegen)!=0)
		{
			this.Commotion-=(this.getBonusMultiplier(Bonus.BonusType.CommotionRegen)-1)*Time.deltaTime;
		}
	}

	/// <summary>
	/// Corrects the player's height
	/// </summary>
	private void CorrectHeight()
	{
		if(this.transform.position.y>this.maxHeight)
		{
			this.transform.position = new Vector3(this.transform.position.x,this.maxHeight-0.01f,this.transform.position.z);
		}
	}

	private void Move()
	{
		//Obtiens les commandes directionelles.
		float  i_horizontal = Input.GetAxis("Horizontal");
		float i_vertical = Input.GetAxis("Vertical");
		Vector3 moveDirection = new Vector3 (-i_horizontal,0,-i_vertical).normalized;
		moveDirection = this.transform.TransformDirection(moveDirection);
		moveDirection.y=0;
		
		//Gestion de la course
		float speed = this.getWalkSpeed();
		if(Input.GetButton("Run")&&this.m_Stamina>0)//Si on veut et peut courrir.
		{
			speed=this.getRunSpeed();
			float multiplier = moveDirection.magnitude;
			this.m_Stamina-=multiplier*Time.deltaTime;
			this.gainFocusExp(Focus.Athletics,RUN_FOCUS_GAIN*Time.deltaTime);
			this.m_NextBreath=Time.time+2;
			this.GenerateMoveThreat(RUN_THREAT_GAIN*multiplier*this.getBonusMultiplier(Bonus.BonusType.RunThreat));
		}
		else
		{
			if(this.m_NextBreath<Time.time)
			{
				this.m_NextBreath=Time.time+2;
				this.m_Stamina+=this.getStaminaRegen();
			}
			this.GenerateMoveThreat(WALK_THREAT_GAIN*moveDirection.normalized.magnitude*this.getBonusMultiplier(Bonus.BonusType.WalkThreat));
			this.gainFocusExp(Focus.Athletics,WALK_FOCUS_GAIN*Time.deltaTime);
		}
		
		this.rigidbody.velocity=moveDirection*speed;
		this.CheckStamina();
	}
	/// <summary>
	/// Simulate Fear for one frame
	/// </summary>
	private void Frighten()
	{
		this.m_BaseFear+=(this.m_FearModifier*Time.deltaTime)*this.getBonusMultiplier(Bonus.BonusType.FearGain);
		this.Fear=(0.1f*((this.Bleed*2)+this.Commotion+(120*this.Tears))+this.m_BaseFear)*this.getBonusMultiplier(Bonus.BonusType.FearEffect);
		if (this.Fear<0){this.Fear=0;}
	}

	/// <summary>
	/// Simulate the current ongoing action.
	/// </summary>
	private void CurrentAction()
	{
		if(this.ActionCompletion!=-1)
		{
			if(this.m_ActionRestrictMovement&&(Input.GetAxis("Horizontal")!=0||Input.GetAxis("Vertical")!=0))
			{
				this.InteruptAction();
			}
			this.ActionCompletion+=this.m_ActionIncrementer*Time.deltaTime;
			if(this.ActionCompletion>=100)
			{
				this.m_CurrentAction();
				if(this.m_RepeatAction)
				{
					this.ActionCompletion=0;
				}
				else
				{
					this.InteruptAction();
				}
			}
		}
	}
	/// <summary>
	/// Checks the inventory button, if pressed, change the inventory's state.
	/// </summary>
	private void CheckInventoryButton()
	{
		if (Input.GetButtonDown("Inventory"))
		{
			this.m_ShowInventory=!this.m_ShowInventory;
			this.m_PlayerMenuView.SetActive(this.m_ShowInventory);
			this.m_InGameOnlyUI.SetActive(!this.m_ShowInventory);
			this.GetComponent<MouseLook>().enabled=!this.m_ShowInventory;
			this.getObservable().notify(ObserverMessages.InventoryStateChanged,this.m_ShowInventory);
		}
	}

	/// <summary>
	/// Simulates the threat for one frame.
	/// </summary>
	private void SimulateThreat()
	{
		this.Threat-=Time.deltaTime*this.m_ThreatReductionSpeed*this.getBonusMultiplier(Bonus.BonusType.ThreatReductionRate);
		if(this.Threat<COWARD_THREAT_THRESOLD)
		{
			this.gainFocusExp(Focus.Coward,(COWARD_THREAT_THRESOLD-this.Threat)/COWARD_THREAT_THRESOLD*Time.deltaTime);
		}
	}

	[RPC]
	void PlayHandAction (int p_hand,string p_Action)
	{
		//this.PlayHandActionScaled(p_hand,p_Action,1);
	}
	
	[RPC]
	void PlayHandActionScaled (int p_hand,string p_Action,float p_Scale)
	{
		/*Animation anim = this.m_HandPositions[p_hand].transform.parent.parent.animation;
		anim[p_Action].speed=p_Scale;
		anim.Stop();
		anim.Play(p_Action);*/
	}
	[RPC]
	void RPC_GenerateThreat (float p_Threat)
	{
		this.Threat+=p_Threat;
		if(Network.isServer)
		{
			EV.gameManager.addThreat(p_Threat);
		}
	}
}