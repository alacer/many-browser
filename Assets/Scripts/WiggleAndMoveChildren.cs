using UnityEngine;
using System.Collections;

public class WiggleAndMoveChildren : MonoBehaviour {

	public float MaxTime;
	public float MinTime;

	// Use this for initialization
	void Start () {
			
		StartCoroutine( DoRandomMove() );
	}
	

	GameObject GetRandomVisibleChild()
	{
		for (int i=0; i < 200; i++)
		{

			GameObject obj = transform.GetChild(Random.Range(0, transform.childCount) ).gameObject;
			Vector3 vPos = Camera.main.WorldToViewportPoint(obj.transform.position);

			if (vPos.x > 0 && vPos.x < 1 && vPos.y > 0 && vPos.y < 1)
				return obj;

		}

		return null;
	}

	IEnumerator DoRandomMove()
	{
		GameObject obj = GetRandomVisibleChild();

		if (obj != null && SceneManager.Instance.GetScene() == Scene.Browse)
		{

			if (Random.Range(0, 2) == 1)
			{
				LeanTween.move(obj, obj.transform.position + Vector3.right * .2f, .3f).setOnComplete( () => {
					LeanTween.move(obj, obj.transform.position - Vector3.right * .2f, .3f);
				});

			}
			else
			{
				LeanTween.rotate(obj,new Vector3(0,0,10), .3f).setOnComplete( () => {
					LeanTween.rotate(obj,Vector3.zero, .3f);
				});


				
			}
		}

		yield return new WaitForSeconds(Random.Range(MinTime,MaxTime));

		StartCoroutine( DoRandomMove() );
	}
}
