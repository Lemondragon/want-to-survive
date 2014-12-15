using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor (typeof(MasterBluePrint))]
class HouseEditor : Editor 
{
	private const int AREA_SIZE = 10; 
	private const float CURSOR_SIZE = 0.5f;
	private GameObject m_Obj ;
	private Vector2 m_CursorPos;
	private Vector2 m_HeldCursorPos;
	private bool m_SelectionMode = false;
	private bool m_ShowRoofs = true;
	private GameObject m_BluePrint = null;
	private GameObject m_Floor = null;
	private GameObject m_Roof = null;
	private bool m_AutoBuild = false;
	private bool m_AutoDelete = false;
	private Stack<GameObject> m_BuildHistory = new Stack<GameObject>();

    public void OnSceneGUI () 
	{
		if(this.m_Obj!=null)
		{
			Vector3 center = m_Obj.transform.position;
			Vector2 offset = new Vector2 (center.x-AREA_SIZE-CURSOR_SIZE,center.z-AREA_SIZE-CURSOR_SIZE);
			Handles.DrawPolyLine(this.createRectangle(offset,AREA_SIZE*2,AREA_SIZE*2));
			if(!this.m_SelectionMode)
			{
				Handles.DrawPolyLine(this.createRectangle(m_CursorPos+offset,CURSOR_SIZE*2,CURSOR_SIZE*2));
			}
			else
			{
				Handles.DrawPolyLine(this.createRectangle(
					m_CursorPos+offset+new Vector2(CURSOR_SIZE,CURSOR_SIZE),
					m_HeldCursorPos.x-m_CursorPos.x,
					m_HeldCursorPos.y-m_CursorPos.y));
			}
			Handles.DrawSolidRectangleWithOutline(this.createRectangle(m_CursorPos+offset+new Vector2(CURSOR_SIZE/2,CURSOR_SIZE/2),CURSOR_SIZE,CURSOR_SIZE),Color.red,Color.white);
			
		}
		else
		{
			this.m_Obj=(this.target as MasterBluePrint).gameObject;
			if(this.m_Obj!=null)
			{
				GameObject fl = (this.target as MasterBluePrint).m_DefaultFloor;
				if(fl!=null)
				{
					this.m_Floor = fl;
				}
				else
				{
					Debug.LogWarning("No default floor is assigned, this is recommended to assign one.");
				}
				GameObject rf = (this.target as MasterBluePrint).m_DefaultRoof;
				if(rf!=null)
				{
					this.m_Roof = rf;
				}
				else
				{
					Debug.LogWarning("No default roof is assigned, this is recommended to assign one.");
				}
				BluePrint bp = (this.target as MasterBluePrint).m_DefaultBuilderBluePrint;
				if(bp!=null)
				{
					this.m_BluePrint = bp.gameObject;
				}
				else
				{
					Debug.LogWarning("No default blueprint is assigned, this is recommended to assign one.");
				}
				this.m_CursorPos = new Vector2(AREA_SIZE,AREA_SIZE);
				this.ShowRoofs(true);
			}
		}
		
	}
	
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		BluePrint bp =EditorGUILayout.ObjectField("Used Blue Print",this.m_BluePrint,typeof(BluePrint)) as BluePrint;
		this.m_Floor =EditorGUILayout.ObjectField("Used Floor",this.m_Floor,typeof(GameObject)) as GameObject;
		this.m_Roof =EditorGUILayout.ObjectField("Used Roof",this.m_Roof,typeof(GameObject)) as GameObject;
		if(bp!=null)
		{
			this.m_BluePrint=bp.gameObject;
		}
		
		Event e = Event.current;
		if(GUILayout.Button("UP (8)")||e.Equals (Event.KeyboardEvent ("[8]")))
		{
			this.MoveCursor(new Vector2(0,1));
		}
		if(GUILayout.Button("DOWN (5)")||e.Equals (Event.KeyboardEvent ("[5]")))
		{
			this.MoveCursor(new Vector2(0,-1));
		}
		if(GUILayout.Button("LEFT (4)")||e.Equals (Event.KeyboardEvent ("[4]")))
		{
			this.MoveCursor(new Vector2(-1,0));
		}
		if(GUILayout.Button("RIGHT (6)")||e.Equals (Event.KeyboardEvent ("[6]")))
		{
			this.MoveCursor(new Vector2(1,0));
		}
		

		
		if(GUILayout.Button("Change Mode (3)")||e.Equals (Event.KeyboardEvent ("[3]")))
		{
			this.m_SelectionMode=!this.m_SelectionMode;
			if(this.m_SelectionMode)
			{
				this.m_HeldCursorPos=this.m_CursorPos;
				this.m_AutoBuild=false;
				this.m_AutoDelete=false;
			}
		}
		if(this.m_SelectionMode)
		{
			if(GUILayout.Button("Build Floor (0)")||e.Equals (Event.KeyboardEvent ("[0]")))
			{
				if(this.m_Floor!=null)
				{
					Vector3 pos=this.getSelectionCenter();
					pos.y=0;
					GameObject newFloor = GameObject.Instantiate(this.m_Floor,pos,Quaternion.identity)as GameObject;
					newFloor.transform.parent=this.m_Obj.transform;
					newFloor.transform.localScale=new Vector3(Mathf.Abs(this.m_HeldCursorPos.x-this.m_CursorPos.x),0.1f,Mathf.Abs(this.m_HeldCursorPos.y-this.m_CursorPos.y));
					this.m_BuildHistory.Push(newFloor);
				}
				else
				{
					Debug.LogError("Unable to build floor, no floor is assigned.");
				}
			}
			if(this.m_ShowRoofs)
			{
				if(GUILayout.Button("Build Roof (.)")||e.Equals (Event.KeyboardEvent ("[.]")))
				{
					if(this.m_Roof!=null)
					{
						Vector3 pos=this.getSelectionCenter();
						pos.y=2;
						GameObject newRoof = GameObject.Instantiate(this.m_Roof,pos,Quaternion.Euler(-90,0,0))as GameObject;
						newRoof.transform.parent=this.m_Obj.transform;
						newRoof.transform.localScale=new Vector3(Mathf.Abs(this.m_HeldCursorPos.x-this.m_CursorPos.x)/2,Mathf.Abs(this.m_HeldCursorPos.y-this.m_CursorPos.y),3);
						this.m_BuildHistory.Push(newRoof);
					}
					else
					{
						Debug.LogError("Unable to build roof, no roof is assigned.");
					}
				}
			}
			this.m_AutoDelete=EditorGUILayout.Toggle("Show Roofs (7)",this.m_AutoDelete);
			if(e.Equals (Event.KeyboardEvent ("[7]")))
			{
				this.m_ShowRoofs=!this.m_ShowRoofs;
				this.ShowRoofs(this.m_ShowRoofs);
			}
			
		}
		else
		{
			if(GUILayout.Button("Build Here (0)")||e.Equals (Event.KeyboardEvent ("[0]")))
			{
				if(m_BluePrint!=null)
				{
					this.placeBluePrint();
				}
			}
			if(GUILayout.Button("Delete Here (.)")||e.Equals (Event.KeyboardEvent ("[.]")))
			{
				this.deleteBluePrint();
			}
			this.m_AutoBuild=EditorGUILayout.Toggle("Auto Build Mode (7)",this.m_AutoBuild);
			if(e.Equals (Event.KeyboardEvent ("[7]")))
			{
				this.m_AutoBuild=!this.m_AutoBuild;
				if(this.m_AutoBuild)
				{
					this.m_AutoDelete=false;
				}
			}
			this.m_AutoDelete=EditorGUILayout.Toggle("Auto Delete Mode (9)",this.m_AutoDelete);
			if(e.Equals (Event.KeyboardEvent ("[9]")))
			{
				this.m_AutoDelete=!this.m_AutoDelete;
				if(this.m_AutoDelete)
				{
					this.m_AutoBuild=false;
				}
			}
		}
		if(GUILayout.Button("UNDO LATEST BUILD (2)")||e.Equals (Event.KeyboardEvent ("[2]")))
			{
				if(this.m_BuildHistory.Count>0)
				{
					DestroyImmediate(this.m_BuildHistory.Pop() as GameObject);
				}
			}
		this.Repaint();
	}
	
	private void placeBluePrint()
	{
		if(m_BluePrint!=null)
		{
			Vector3 p_Pos = this.getCursorPos(this.m_CursorPos);
			p_Pos.y=0.7f;
			bool validSpace = true;
			foreach(Component child in this.m_Obj.GetComponentsInChildren(typeof(BluePrint)))
			{
				Transform trans = (child as BluePrint).transform;
				if(trans.position.Equals(p_Pos))
				{
					validSpace=false;
				}
			}
			if(validSpace)
			{
				GameObject newbp = GameObject.Instantiate(this.m_BluePrint,p_Pos,Quaternion.identity) as GameObject;
				newbp.transform.parent=this.m_Obj.transform;
				this.m_BuildHistory.Push(newbp);
			}
		}
	}
	private void deleteBluePrint()
	{
		Vector3 p_Pos = this.getCursorPos(this.m_CursorPos);
		foreach(Component child in this.m_Obj.GetComponentsInChildren(typeof(BluePrint)))
		{
			Transform trans = (child as BluePrint).transform;
			if(trans.position.x==p_Pos.x&&trans.position.z==p_Pos.z)
			{
				DestroyImmediate(trans.gameObject);
			}
		}
	}
	private Vector3 getCursorPos(Vector2 p_Cursor)
	{
		Vector3 center = m_Obj.transform.position;
		Vector2 offset = new Vector2 (center.x-AREA_SIZE-CURSOR_SIZE,center.z-AREA_SIZE-CURSOR_SIZE);
		return new Vector3(offset.x+p_Cursor.x+CURSOR_SIZE,0.5f,offset.y+p_Cursor.y+CURSOR_SIZE);
	}
	private Vector3 getSelectionCenter()
	{
		if(!this.m_SelectionMode)
		{
			Debug.LogWarning("You got the center of the selection while the user is not using selections. Strange behavior may occur.");
		}
		Vector3 pos = Vector3.Lerp(this.getCursorPos(this.m_CursorPos),this.getCursorPos(this.m_HeldCursorPos),0.5f);
		return pos;
	}
	
	private Vector3[] createRectangle(Vector2 p_Anchor,float p_Width,float p_Height)
	{
		return  new Vector3[]{new Vector3(p_Anchor.x,0,p_Anchor.y),
				new Vector3(p_Anchor.x+p_Width,0,p_Anchor.y),
				new Vector3(p_Anchor.x+p_Width,0,p_Anchor.y+p_Height),
				new Vector3(p_Anchor.x,0,p_Anchor.y+p_Height),new Vector3(p_Anchor.x,0,p_Anchor.y)};
	}
	private void MoveCursor(Vector2 p_Direction)
	{
		this.m_CursorPos+=p_Direction;
		if(new Rect(0,0,20,20).Contains(this.m_CursorPos))
		{
			if(this.m_AutoBuild)
			{
				this.placeBluePrint();
			}
			else if(this.m_AutoDelete)
			{
				this.deleteBluePrint();
			}
			SceneView.RepaintAll();
		}
		else
		{
			this.m_CursorPos-=p_Direction;
		}
	}
	private void ShowRoofs(bool p_Show)
	{
		foreach(Component child in this.m_Obj.GetComponentsInChildren(typeof(Ceilling)))
		{
			Renderer rend = (child as Ceilling).renderer;
			rend.enabled=p_Show;
			foreach(Renderer rendChild in (child as Ceilling).transform.GetComponentsInChildren<Renderer>())
			{
				rendChild.enabled=p_Show;
			}
		}
	}
}
