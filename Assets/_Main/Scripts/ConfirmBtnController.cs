using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mainly animation controller for the confirm button
public class ConfirmBtnController : MonoBehaviour
{
	const string show_flag = "isShown";

	private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
		anim = GetComponent<Animator>();
    }

	public void ShowButton(bool val) {
		anim.SetBool(show_flag, val);
	}
}
