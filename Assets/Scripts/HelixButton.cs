using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HelixButton : MonoBehaviour {


	UIButton _button;
	static List<UIButton> _allButtons = new List<UIButton>();

	void Awake()
	{

		_button = GetComponent<UIButton>();
		_allButtons.Add(_button);
	}

	public static void UnselectAll()
	{
		foreach(UIButton button in _allButtons)
		{

			button.normalSprite = button.hoverSprite;

		}

	}

	void SortBy(string sort, SortOrder order)
	{

		_button.normalSprite = _button.pressedSprite;
		
		foreach(UIButton button in _allButtons)
		{
			if (button != _button)
			{
				button.normalSprite = button.hoverSprite;
			
//				button.target.spriteName = button.normalSprite;
				
			}
		}

		HelixManager.Instance.FormHelix(sort,order);
	}


	public void OnTapPopularity()
	{
		SortBy("Popularity",SortOrder.Asending);
	}

	public void OnTapPrice()
	{
		SortBy("Price",SortOrder.Desending);
	}

	public void OnTapExpertRating()
	{
		SortBy("ExpertRating",SortOrder.Asending);
	}

	public void OnTapBuyerRating()
	{
		SortBy("BuyerRating",SortOrder.Asending);
	}

	public void OnTapAvailability()
	{
		SortBy("Availability",SortOrder.Asending);
	}

	IEnumerator UpdateColors()
	{
		yield return new WaitForSeconds(.5f);

		foreach (UIButton button in _allButtons)
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



}

