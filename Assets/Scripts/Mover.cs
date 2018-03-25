using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(Mathf.Cos(Time.time), 0.5f, -Mathf.Cos(Time.time * 0.065f)) * 0.25f;
	}
}
