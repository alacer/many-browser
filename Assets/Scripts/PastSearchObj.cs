using UnityEngine;
using System.Collections;

public class PastSearchObj : ImageObj {

	public TextMesh SearchTextMesh;

	public void SetSearchText(string search)
	{
		SearchTextMesh.text = search;
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

	protected override void ScaleToBox(float animTime)
	{
		// do nothing
	}
	
	protected override void ScaleToAspect(float animTime)
	{
		// do nothing
	}

}
