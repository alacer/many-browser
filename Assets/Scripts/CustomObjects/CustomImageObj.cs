using UnityEngine;
using System.Collections;

public class CustomImageObj : ImageObj {

	public Texture2D BrokenImage;
	public Texture2D FixedImage;
	public Vector3 SmallPosition;

	public bool AppointmentSet = false;

	protected override void OnSelected()
	{
		base.OnSelected();


		ImageRenderer.material.mainTexture = BrokenImage;
	}

	protected override void OnUnselected()
	{
		base.OnUnselected();

		ImageRenderer.material.mainTexture = FixedImage;

		if (AppointmentSet && transform.localPosition.z != SmallPosition.z)
			StartCoroutine(MoveToSmallPos());

	}

	IEnumerator MoveToSmallPos()
	{
		yield return new WaitForSeconds(.5f);

		if (LeanTween.isTweening(gameObject) == false)
			LeanTween.moveLocal(gameObject,SmallPosition,.3f);
	}

	public override void UpdateFavoritesButton(string url)
	{
		// do nothing

	}
}
