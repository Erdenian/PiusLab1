using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(Mathf.Cos(Time.time * 2) * 0.25f, -0.17f, -Mathf.Cos(Time.time * 0.085f) * 0.25f);
	}
}
