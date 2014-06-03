using UnityEngine;
using System.Collections;

public class HelixButton : MonoBehaviour {



	void SortBy(string sort)
	{

		GridManager.Instance.FormHelix(sort,SortOrder.Desending);
	}

	void SortAsending(string sort)
	{
		GridManager.Instance.FormHelix(sort,SortOrder.Asending);
;
	}

}

