using UnityEngine;
using System.Collections;

public class ContactsButton : MonoBehaviour {

	public void GoToContacts()
	{

		if (Community.CurrentCommunity.Name != "Now")
			StartCoroutine(GoToKartuaAndSearchContacts());
		else
			SearchContacts();

	}

	IEnumerator GoToKartuaAndSearchContacts()
	{
		CameraManager.Instance.OnCommunityButtonTouch();

		while (Community.CurrentCommunity.Name != "Now" || LeanTween.isTweening(Camera.main.gameObject))
		{

			yield return new WaitForSeconds(.3f);
		}

		SearchContacts();
	}

	void SearchContacts()
	{

		ImageObj obj = Community.CurrentCommunity.FindObjWithName("Contacts");
		if (obj != null)
		{
			StartCoroutine( SelectionManager.Instance.OnObjectSearch(obj.transform) );
		}
	}
}
