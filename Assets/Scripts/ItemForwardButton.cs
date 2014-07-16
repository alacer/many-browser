using UnityEngine;
using System.Collections;

public class ItemForwardButton : MonoBehaviour {

	ImageObj _imageObj;


	// Use this for initialization
	void Start () {
		_imageObj = transform.parent.GetComponent<ImageObj>();

	
	}
	
	public void OnTap()
	{
		if (_imageObj.IsCommunity())
			CameraManager.Instance.DoForwardTransitionOnObj(_imageObj);

	}
}
