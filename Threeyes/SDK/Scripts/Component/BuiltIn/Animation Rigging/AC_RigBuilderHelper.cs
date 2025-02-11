using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Threeyes.Coroutine;
using NaughtyAttributes;
/// <summary>
/// Make sure joints relation don't break when cursor size changed
/// 
/// How to save joints' Initialization info:
/// 1. Setup joints
/// 2. Open ContextMenu and invoke SaveJointInfo before game played 
/// 
/// PS：更改光标大小/光标显隐后，RigBuilder的joints会出现错位（因为性能优化，更改缩放不会更新Joint： https://forum.unity.com/threads/how-can-i-override-scale-using-animation-rigging.770219/#post-7277947）
/// </summary>
public class AC_RigBuilderHelper : ComponentHelperBase<RigBuilder>,
	IAC_CommonSetting_IsAliveCursorActiveHandler,
	IAC_CommonSetting_CursorSizeHandler,
	IAC_SystemWindow_ChangedHandler
{
	[Header("Cache")]
	[SerializeField] protected List<Transform> listBone = new List<Transform>();//缓存每个Bone的初始位置
	[SerializeField] protected List<Vector3> listDefaultBonePosition = new List<Vector3>();//缓存每个Bone的初始局部位置
	[SerializeField] protected List<Vector3> listDefaultBoneRotation = new List<Vector3>();//缓存每个Bone的初始局部旋转

	#region CallBack

	public void OnCursorSizeChanged(float value)
	{
		RebuildJoint();
	}
	public void OnIsAliveCursorActiveChanged(bool isShow)
	{
		if (isShow)
			RebuildJoint();
	}
	public void OnWindowChanged(AC_WindowEventExtArgs e)
	{
		//切换屏幕后/更改分辨率后，需要重新BuildJoint
		if (e.stateChange == AC_WindowEventExtArgs.StateChange.After)
			RebuildJoint();
	}

	#endregion

	void Start()
	{
		RebuildJoint();
	}

	protected Coroutine cacheEnum;
	public void RebuildJoint()
	{
		TryStopCoroutine();
		cacheEnum = CoroutineManager.StartCoroutineEx(IERebuildJoint());
	}
	protected virtual void TryStopCoroutine()
	{
		if (cacheEnum != null)
			CoroutineManager.StopCoroutineEx(cacheEnum);
	}

	IEnumerator IERebuildJoint()
	{
		//重建Rig (Warning: Setting改变大小不能用Tween，否则会出现偏移的Bug)
		if (!Comp)
			yield break;

		Comp.Clear();//停止旧的Job
		yield return null;//Wait for process completed

		for (int i = 0; i != listBone.Count; i++)
		{
			Transform tfChild = listBone[i];
			tfChild.localPosition = listDefaultBonePosition[i];
			tfChild.localEulerAngles = listDefaultBoneRotation[i];
		}
		Comp.Build();
	}


#if UNITY_EDITOR

	//PS:在编辑器中记录所有子关节的信息，然后再实时还原（PS:因为BoneRenderer是编辑器脚本，因此只能先记录所有的子节点）
	[ContextMenu("SaveJointInfo")]
	[Button("SaveJointInfo")]
	public void SaveJointInfo()
	{
		listBone.Clear();
		listDefaultBonePosition.Clear();
		listDefaultBoneRotation.Clear();
		for (int i = 0; i != GetComponent<BoneRenderer>().transforms.Length; i++)
		{
			Transform tfChild = GetComponent<BoneRenderer>().transforms[i];
			listBone.Add(tfChild);
			listDefaultBonePosition.Add(tfChild.localPosition);
			listDefaultBoneRotation.Add(tfChild.localEulerAngles);
		}
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);// mark as dirty, so the change will be save into scene file
#endif
	}

	//PS:非运行时调用
	[ContextMenu("ResetJoint")]
	public void ResetJoint()
	{
		List<RigLayer> layers = Comp.layers;
		foreach (RigLayer rigLayer in layers)
		{
			for (int i = 0; i != rigLayer.jobs.Length; i++)
			{
				DampedTransformJob job = (DampedTransformJob)rigLayer.jobs[i];
				DampedTransform dampedTransform = (DampedTransform)rigLayer.constraints[i];

				dampedTransform.data.constrainedObject.localPosition = job.localBindTx.translation;
				dampedTransform.data.constrainedObject.localEulerAngles = job.localBindTx.rotation.eulerAngles;
			}
		}
	}

#endif
}
