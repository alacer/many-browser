using UnityEngine;
using System.Collections;

public class WasherCubeRotator : CubeRotator {

	public GameObject RepairCompanies;

	public Texture2D WasherAppointmentTex;


	void OnSetAppointmentTap()
	{
		Debug.Log("appointment: " + GetForward());

		if (GetForward() == Vector3.left)
		{
			Debug.Log("in");

			CubeTransform.renderer.material.mainTexture = WasherAppointmentTex;

			transform.parent.GetComponent<CustomImageObj>().AppointmentSet = true;
			
			RotateBox(1,.3f);
		}
	}

	void OnRepairCompanyTap()
	{
		if (GetForward() == Vector3.forward && RepairCompanies.transform.localScale == Vector3.one)
		{
			TweenScale.Begin(RepairCompanies,.3f,Vector3.zero);
		
			RotateBox(1,.3f);
		}
	}
	
	void OnRepairTap()
	{
		if (GetForward() != Vector3.forward)
			return;

		if (RepairCompanies.transform.localScale != Vector3.one)
			TweenScale.Begin(RepairCompanies,.3f,Vector3.one);
		else
			TweenScale.Begin(RepairCompanies,.3f,Vector3.zero);
		
	}

	protected override void OnChange ()
	{
		if (RepairCompanies.transform.localScale != Vector3.zero)
		{
			TweenScale.Begin(RepairCompanies,.3f,Vector3.zero);
		}
	}

}
