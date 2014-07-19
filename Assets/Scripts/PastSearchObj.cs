using UnityEngine;
using System.Collections;

public class PastSearchObj : ImageObj {

	public TextMesh SearchTextMesh;
	public float ZOffset = 0;

	Vector3 startPos;

	public void SetSearchText(string search)
	{
		SearchTextMesh.text = search;
		startPos = SearchTextMesh.transform.localPosition;
	}

	protected override void Update()
	{
		base.Update();

		SearchTextMesh.transform.localPosition = startPos + Vector3.forward * ZOffset;

		Debug.Log("setting pos: " + SearchTextMesh.transform.localPosition);
	}

//	public override GoInto()
//	{
//
//	}

	public void DoSearch()
	{
		ImageSearch.Instance.Search(SearchTextMesh.text,true);
	}

	protected override void SetAlphaOnText(float alpha)
	{
		Color c = SearchTextMesh.color;

		SearchTextMesh.color = new Color(c.r,c.g,c.b,alpha);
	}

	public override bool CanGoThrough()
	{
		return true;
	}

	protected override void OnSelected()
	{
		base.OnSelected();

		CubeRotator.SetActive(false);


	}

	public override void ScaleToCube()
	{
		// do nothing
	}

	public override void ScaleToBox(float animTime)
	{
		// do nothing
	}
	
	public override void ScaleToBook(float animTime)
	{
		// do nothing
	}

}
