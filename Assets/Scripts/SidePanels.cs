using UnityEngine;
using System.Collections;


public class SidePanels : MonoBehaviour {

	public GameObject[] Panels;

	GameObject _currentPanel;
	Vector3 _outPos = new Vector3(-120,0,0);
	CommunityType _lastCommunityType = CommunityType.Generic;

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
			LeanTween.moveLocal(_currentPanel,Vector3.zero,1);

		}
		else
		{
			LeanTween.moveLocal(_currentPanel,_outPos,1).setOnComplete( () => {
				_currentPanel = Panels[(int)type];
				LeanTween.moveLocal(_currentPanel,Vector3.zero,1);
			});

		}

		_lastCommunityType = type;
	}

}
