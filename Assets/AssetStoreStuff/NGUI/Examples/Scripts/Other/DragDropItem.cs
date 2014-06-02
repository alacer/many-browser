//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

[AddComponentMenu("NGUI/Examples/Drag & Drop Item")]
public class DragDropItem : MonoBehaviour
{
	/// <summary>
	/// Prefab object that will be instantiated on the DragDropSurface if it receives the OnDrop event.
	/// </summary>

	public GameObject prefab;
	UIRoot mRoot;
	Transform mTrans;
	bool mIsDragging = false;
	Transform mParent;

	static float _minZ = float.MaxValue;

	/// <summary>
	/// Update the table, if there is one.
	/// </summary>



	void UpdateTable ()
	{
		UITable table = NGUITools.FindInParents<UITable>(gameObject);
		if (table != null) table.repositionNow = true;
	}

	/// <summary>
	/// Drop the dragged object.
	/// </summary>

	public void Drop ()
	{
		// Is there a droppable container?
//		Collider col = UICamera.lastHit.collider;
//		DragDropContainer container = (col != null) ? col.gameObject.GetComponent<DragDropContainer>() : null;

//		if (container != null)
//		{
//			// Container found -- parent this object to the container
//			mTrans.parent = container.transform;
//
//			Vector3 pos = mTrans.localPosition;
//			pos.z = 0f;
//			mTrans.localPosition = pos;
//		}
//		else
//		{
//			// No valid container under the mouse -- revert the item's parent
//			mTrans.parent = mParent;
//		}

		// Notify the table of this change
		UpdateTable();

		// Make all widgets update their parents
		BroadcastMessage("CheckParent", SendMessageOptions.DontRequireReceiver);
	}

	/// <summary>
	/// Cache the transform.
	/// </summary>

	void Awake () 
	{
		mRoot = NGUITools.FindInParents<UIRoot>(gameObject);
		mTrans = transform; 
		if (transform.position.z < _minZ)
			_minZ = transform.position.z;
	}

	/// <summary>
	/// Start the drag event and perform the dragging.
	/// </summary>

	void OnDrag (Vector2 delta)
	{
		if (SceneManager.Instance.GetScene() != Scene.Default)
			return;

		if (UICamera.currentTouchID > -2)
		{
//			if (!mIsDragging)
//			{
//				mIsDragging = true;
//				mParent = mTrans.parent;
//				mTrans.parent = DragDropRoot.root;
//				
//				Vector3 pos = mTrans.localPosition;
//				pos.z = 0f;
//				mTrans.localPosition = pos;
//				
//				mTrans.BroadcastMessage("CheckParent", SendMessageOptions.DontRequireReceiver);
//			}
//			else
//			{
			mTrans.localPosition += (Vector3)delta * (768.0f / Screen.height);


		}
	}

	/// <summary>
	/// Start or stop the drag operation.
	/// </summary>

	void OnPress (bool isPressed)
	{
		mIsDragging = false;
		_minZ -= .1f;
		mTrans.position = new Vector3(mTrans.position.x,mTrans.position.y,_minZ);
		Collider col = collider;
		if (col != null) col.enabled = !isPressed;
		if (!isPressed) Drop();
	}
}