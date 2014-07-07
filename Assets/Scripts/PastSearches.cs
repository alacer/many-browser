using UnityEngine;
using System.Collections;

public class PastSearches : MonoBehaviour {

	public PastSearchObj[] AllObjs;
	public static int MaxSearches = 10;
	public static PastSearches Instance;
	

	// Use this for initialization
	void Start () {

		Instance = this;

		UpdateSearchObjs();
	}

	void UpdateSearchObjs()
	{

		for (int i=0; i < AllObjs.Length; i++)
		{
			string key = "PastSearch" + i;
			
			if (PlayerPrefs.HasKey(key))
			{
				AllObjs[i].gameObject.SetActive(true);
			}
			else
				break;
			
		}
	}

	public static int GetPastSearchesCount()
	{
		int count = 0;

		for (int i=0; i < MaxSearches; i++)
		{
			string key = "PastSearch" + i;
			
			if (PlayerPrefs.HasKey(key))
			{
				count++;
			}
			else
				break;
			
		}

		return count;
	}

	public static void OnDidSearch(string search)
	{
		PlayerPrefs.SetString("PastSearch" + GetPastSearchesCount() % MaxSearches, search);

		if (Instance != null)
			Instance.UpdateSearchObjs();

	}

}
