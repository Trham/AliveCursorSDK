using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Threeyes.Editor;
#endif
/// <summary>
/// Custom Behaviour when CommonSetting changed
/// </summary>
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_AC_Cursor_Behaviour_Setting + "AC_CommonSettingBehaviour", 0)]
public class AC_CommonSettingBehaviour : AC_BehaviourBase,
	IAC_CommonSetting_IsAliveCursorActiveHandler
{
	#region Property & Field
	public BoolEvent onAliveCursorActiveDeactive;
	#endregion


	#region Callback
	public void OnIsAliveCursorActiveChanged(bool isActive)
	{
		onAliveCursorActiveDeactive.Invoke(isActive);
	}
	#endregion

	#region Editor Method
#if UNITY_EDITOR

	//——MenuItem——
	static string instName = "CSetB ";
	[UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Cursor_Behaviour_Setting + "CommonSettingBehaviour", false, 0)]
	public static void CreateActionEventPlayer_CursorStateBehaviour()
	{
		EditorTool.CreateGameObjectAsChild<AC_CommonSettingBehaviour>(instName);
	}

	//——Hierarchy GUI——
	public override string ShortTypeName { get { return "CSetB"; } }

	//——Inspector GUI——
	public override void SetInspectorGUIUnityEventProperty(GUIPropertyGroup group)
	{
		group.title = "Unity Event";
		group.listProperty.Add(new GUIProperty(nameof(onActiveDeactive)));

		group.listProperty.Add(new GUIProperty(nameof(onAliveCursorActiveDeactive)));
	}

#endif
	#endregion
}
