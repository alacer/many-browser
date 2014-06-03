using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HelixButton : MonoBehaviour {


	UIImageButton _button;
	static List<UIImageButton> _allButtons = new List<UIImageButton>();

	void Awake()
	{
		Debug.Log("adding button from: " + gameObject.name);
		_button = GetComponent<UIImageButton>();
		_allButtons.Add(_button);
	}

	void SortBy(string sort)
	{

		GridManager.Instance.FormHelix(sort,SortOrder.Desending);
	}

	void SortAsending(string sort)
	{
		GridManager.Instance.FormHelix(sort,SortOrder.Asending);

	}

	IEnumerator UpdateColors()
	{
		yield return new WaitForSeconds(.5f);

		foreach (UIImageButton button in _allButtons)
		{
			if (_button == button)
			{
				Debug.Log("setting button pressed: " + button.gameObject.name);
				_button.normalSprite = _button.hoverSprite;
			//	_button.target.spriteName = _button.normalSprite;
			}
			else
			{
				Debug.Log("setting button released: " + button.gameObject.name);
				_button.normalSprite = _button.pressedSprite;
			//	_button.target.spriteName = _button.normalSprite;
			}
			
		}
	}

	void OnPress(bool pressed)
	{

		_button.normalSprite = _button.pressedSprite;
	
		foreach(UIImageButton button in _allButtons)
		{
			if (button != _button)
			{
				button.normalSprite = button.hoverSprite;
				button.target.spriteName = button.normalSprite;

			}
		}


	

	}

}

