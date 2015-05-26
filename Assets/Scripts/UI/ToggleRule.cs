using UnityEngine;
using System.Collections;

public class ToggleRule : MonoBehaviour {

	//Configurable
	private int _ruleValue = 1;
	public int ruleValue {
			get
			{
				return _ruleValue;
			}
			set
			{
				_ruleValue = value;
			ruleInput.value = value.ToString();
			}
	}
	public int minValue = 1;
	public int maxValue = 1;
	public int incrementConstant = 1;

	//Inspector
	public UIInput ruleInput;

	// Use this for initialization
	void Start () {
	
		ruleInput = GetComponent<UIInput>();
		//ruleValue = int.Parse(ruleInput.value);

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void IncrementRule()
	{
		if(ruleValue < maxValue)
		{
			ruleValue += incrementConstant;
		}

	}

	public void DecrementRule()
	{
		if(ruleValue > minValue)
		{
			ruleValue -= incrementConstant;
		}
	}
}
