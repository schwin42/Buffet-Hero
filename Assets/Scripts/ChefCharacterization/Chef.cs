using UnityEngine;
using System.Collections.Generic;
using TagId = System.String;
using AttributeId = System.String;

public class Chef : MonoBehaviour {

	public List<TagId> Liked;
	public List<TagId> Disliked;

	public List<AttributeId> ChosenAttributes;

	public void Init() {
		this.Liked = new List<TagId>();
		this.Disliked = new List<TagId>();
		this.ChosenAttributes = new List<AttributeId>();
	}

}
