using UnityEngine;
using System.Collections;


public class SidePanels : MonoBehaviour {

	public GameObject[] Panels;

	GameObject _currentPanel;
	Vector3 _outPos = new Vector3(-120,0,0);
	CommunityType _lastCommunityType = CommunityType.Generic;

	GameObject _touchBlocker;

	void Start()
	{
		_touchBlocker = GameObject.Find( "TouchBlockerSide" );

		_touchBlocker.SetActive(false);

	}
	void OnCommunityChange()
	{
		Debug.Log("old community: " + _lastCommunityType + " new: " + Community.CurrentCommunity.Type);

		if (_lastCommunityType != Community.CurrentCommunity.Type)
			ShowPanel(Community.CurrentCommunity.Type);

	}

	public void ShowPanel(CommunityType type)
	{

		if (_currentPanel == null) 
		{
			_currentPanel = Panels[(int)type];

			if (type != CommunityType.Kartua)
			{
				LeanTween.moveLocal(_currentPanel,Vector3.zero,1);
				_touchBlocker.SetActive(true);
			}
				
		}
		else // open new
		{
			if (_currentPanel.transform.localPosition == Vector3.zero)
			{
				Debug.Log ("panel is showing");
				_touchBlocker.SetActive(false);

				LeanTween.moveLocal(_currentPanel,_outPos,1).setOnComplete( () => {
					_currentPanel = Panels[(int)type];

					if (type != CommunityType.Kartua)
					{
						LeanTween.moveLocal(_currentPanel,Vector3.zero,1);
						_touchBlocker.SetActive(true);
					}
				});
			}
			else
			{
				Debug.Log("panel isnt showing ");

				_currentPanel = Panels[(int)type];
				if (type != CommunityType.Kartua)
				{
					LeanTween.moveLocal(_currentPanel,Vector3.zero,1);
					_touchBlocker.SetActive(true);
				}
			}

		}

		_lastCommunityType = type;
	}

}
