using UnityEngine;
using System.Collections;

public class ForwardTap : Tappable {

	public GameObject ForwardObject;
	public string MethodToCall;

	public void OnTap()
	{
		ForwardObject.SendMessage(MethodToCall);

	}
}
