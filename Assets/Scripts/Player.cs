using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public static Player Instance;

	public float score = 0F;

	void Awake()
	{
		Instance = this;
	}


	public void Eat()
	{
		Food food = GameController.Instance.activeFood;
		//float foodValue = GameController.Instance.activeFood.Value;
		//score += food.Value;

//		string commentary = "";
//		if(food.Value < -100f)
//		{
//			commentary = "That was god awful.";
//		} else if(food.Value >= -100f && food.Value < -50f)
//		{
//			commentary = "So disgusting.";
//		} else if(food.Value >= -50f && food.Value < 0f)
//		{
//			commentary = "Kinda gross.";
//		} else if(food.Value >= 0f && food.Value < 50f)
//		{
//			commentary = "You've had better";
//		} else if(food.Value >= 50f && food.Value < 100f)
//		{
//			commentary = "Not bad.";
//		} else if(food.Value >= 100f && food.Value < 200f)
//		{
//			commentary = "Mmm, delicious!";
//		} else if(food.Value >= 200)
//		{
//			commentary = "That was way better than sex.";
//		}
//		InterfaceController.Instance.WriteToOutcome(food.form.modifier + " * " + food.ingredient.multiplier + " * " + food.quality.multiplier + " = " + food.Value
//		                                            + "\n"+commentary );
//		InterfaceController.Instance.WriteToScore(score);
		                                     
		GameController.Instance.NextPrompt();
	}

	public void DontEat()
	{
		InterfaceController.Instance.WriteToOutcome("Didn't eat");
		GameController.Instance.NextPrompt();
	}
}
