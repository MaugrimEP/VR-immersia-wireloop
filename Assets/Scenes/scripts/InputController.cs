using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    private Interaction _interactionController = null;

	// Use this for initialization
	void Start () {
        _interactionController = GetComponent<Interaction>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.A))
            _interactionController.Grab();

        if (Input.GetKeyDown(KeyCode.Z))
            _interactionController.Drop();

    }
}
