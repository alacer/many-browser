using UnityEngine;
using System.Collections;

public class DetailedDescription : MonoBehaviour {

	public UITexture BookImage;
	public UILabel Title;
	public UILabel Author;
	public UILabel StarRating;
	public UILabel FormatAndPrice;

	public UILabel[] BottomLabels;
	public UILabel Publisher;

	public void SetData(ImageObj obj)
	{
		
		
		if (obj.HasData("Title"))
			Title.text =  obj.GetData<string>("Title");

		BookImage.mainTexture = obj.ImageRenderer.material.mainTexture;
		if (obj.HasData("Author"))
			Author.text = "By " + obj.GetData<string>("Author");

		if (obj.HasData("ExpertRating"))
			StarRating.text = SortText.GetStarRating(obj.GetData<float>("ExpertRating"));

		if (obj.HasData("Format") && obj.HasData("Price"))
			FormatAndPrice.text =  "- " + obj.GetData<string>("Format") + ": " + SortText.GetPrice(obj.GetData<float>("Price"));


		int i=0;


		if (obj.HasData("Publisher") && obj.GetData<string>("Publisher") != string.Empty)
		{
			BottomLabels[i].text = "- Publisher:";
			Publisher.text = obj.GetData<string>("Publisher");
			i++;
		}

		if (obj.HasData("Edition") && obj.GetData<string>("Edition") != string.Empty)
		{
			BottomLabels[i].text = "- Edition: " + obj.GetData<string>("Edition");
			i++;
		}

		if (obj.HasData("PublicationDate") && obj.GetData<string>("PublicationDate") != string.Empty)
		{
			BottomLabels[i].text = "- Publication Date: " + obj.GetData<string>("PublicationDate");
			i++;
		}


		if (obj.HasData("ReleaseDate") && obj.GetData<string>("ReleaseDate") != string.Empty)
		{
			BottomLabels[i].text = "- ReleaseDate: " + obj.GetData<string>("ReleaseDate");
			i++;
		}

		for ( ; i < BottomLabels.Length; i++)
		{
			BottomLabels[i].text = "";
		}


		

	}


}
