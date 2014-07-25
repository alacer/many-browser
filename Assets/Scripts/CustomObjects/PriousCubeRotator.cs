using UnityEngine;
using System.Collections;

public class PriousCubeRotator : CubeRotator {

	public GameObject Guests;


	void OnBookTableTap()
	{
		Debug.Log("appointment: " + GetForward());

		if (GetForward() == Vector3.forward)
		{


		//	transform.parent.GetComponent<CustomImageObj>().AppointmentSet = true;
			
			RotateBox(1,.3f);
		}
	}

	void OnPersonTap()
	{


	}

	void OnInviteTap()
	{
		Guests.SetActive(true);

	}


}
