using UnityEditor;
using UnityEngine;

public class HouseBuilderWindow : EditorWindow
{

	int m_Step = 0;
	bool m_CursorGroup = false;
	public GameObject m_SelectionPrefab =  AssetDatabase.LoadAssetAtPath("Prefabs/Structures/Selection.prefab", typeof(GameObject)) as GameObject;
	public GameObject m_BluePrintPrefab =  AssetDatabase.LoadAssetAtPath("Prefabs/Structures/Generated/WallBluePrint.prefab", typeof(GameObject))as GameObject;
    
    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/House Builder")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(HouseBuilderWindow));
    }
    
    void OnGUI()
    {
		switch(m_Step)
		{
		case 0:
	        GUILayout.Label ("Creation", EditorStyles.boldLabel);
			if(GUILayout.Button("New"))
			{
				this.m_Step=1;
			}
			break;
		case 1:
			this.m_CursorGroup = EditorGUILayout.Foldout(this.m_CursorGroup,"Cursor");
			if(this.m_CursorGroup)
			{
				m_BluePrintPrefab = EditorGUILayout.ObjectField(this.m_BluePrintPrefab,typeof(GameObject),false) as GameObject;
				GUILayout.Button("UP");
				GUILayout.Button("DOWN");
				GUILayout.Button("LEFT");
				GUILayout.Button("RIGHT");
				Handles.DrawSolidRectangleWithOutline(new Vector3[]{new Vector3(0,0,0),new Vector3(100,0,0),new Vector3(100,0,100),new Vector3(0,0,100)},Color.blue,Color.white);
				HandleUtility.Repaint();
			}
			break;
		}
    }

}