using UnityEngine;
using System.Collections;

public class Logo : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {

		yield return new WaitForSeconds(3);

		LeanTween.alpha(gameObject,0, 2);
	
	}
	

}
