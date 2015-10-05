using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Console : MonoBehaviour, IPointerClickHandler {

	bool expanded = false;
	float expandPosition = 0;
	float collapsePosition = -725;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	#region IPointerClickHandler implementation

	public void OnPointerClick (PointerEventData eventData)
	{
		if (expanded) {
			expanded = false;
			Collapse();
		} else {
			expanded = true;
			Expand ();
		}
	}

	public void Expand() {
		transform.localPosition = new Vector3(expandPosition, transform.localPosition.y, transform.localPosition.z);
	}

	public void Collapse() {
		transform.localPosition = new Vector3(collapsePosition, transform.localPosition.y, transform.localPosition.z);
	}

	#endregion
}
