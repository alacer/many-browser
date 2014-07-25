using UnityEngine;
using System.Collections;

public class RestaurantsCubeRotator : CubeRotator {

	GameObject RestaurantPeopleObj;

	public Texture2D InvitedTexture;

	void Awake()
	{
		RestaurantPeopleObj = GameObject.Find("RestaurantPeople");

	}

	void OnBookTableTap()
	{

		if (GetForward() == Vector3.right && RestaurantPeopleObj.GetComponent<RestaurantPeople>().IsEnabled == false)
		{
			//	transform.parent.GetComponent<CustomImageObj>().AppointmentSet = true;
			
			RotateBox(1,.3f);
		}
	}

	protected override void OnChange ()
	{
		if (RestaurantPeopleObj.activeSelf)
			RestaurantPeopleObj.GetComponent<RestaurantPeople>().Disable();
	}

	void OnInviteTap()
	{
		Debug.Log("inviting");
		RestaurantPeopleObj.GetComponent<RestaurantPeople>().Enable();

	}


	public void OnPersonTap()
	{
		StartCoroutine(OnPersonTapRoutine());
	}

	IEnumerator OnPersonTapRoutine()
	{
		yield return new WaitForSeconds(1.5f);

		transform.parent.GetComponent<ImageObj>().ImageRenderer.materials[5].mainTexture = InvitedTexture;

	}

}
