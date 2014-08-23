using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public static Player Instance;

	public float constitution = 0f;

	public float score = 0F;


	void Awake()
	{
		Instance = this;
	}


	public void Eat()
	{
		Food food = GameController.Instance.activeFood;
		//float foodValue = GameController.Instance.activeFood.Value;
		float foodQuality = food.Quality;
		score += foodQuality;

		string commentary = "";
		if(foodQuality < -100f) {
			commentary = "You're going to be sick"; 
		} 
		else if(foodQuality >= -100f && foodQuality < -75f) {
			commentary = "God awful"; 
		} 
		else if(foodQuality >= -75 && foodQuality < -50f){
			commentary = "Absolutely disgusting";
		} 
		else if(foodQuality >= -50f && foodQuality < -25f) {
			commentary = "Really bad"; 
		} 
		else if(foodQuality >= -25f && foodQuality < -0f) {
			commentary = "Kinda gross"; 
		} 
		else if(foodQuality >= 0f && foodQuality < 25f){
			commentary = "You've had better"; 
		}
		else if(foodQuality >= 25f && foodQuality < 50f) {
			commentary = "Not bad."; 
		} 
		else if(foodQuality >= 50f && foodQuality < 75f) //A
		{
			commentary = "Quite good"; 
		}
		else if(foodQuality >= 75f && foodQuality < 100f) //A
		{
			commentary = "Mmm, delicious"; 
		} 
		else if(foodQuality >= 100) {
			commentary = "Amazing!"; 
		}
		InterfaceController.Instance.WriteToOutcome(
			//food.attributes + " * " + food.ingredient.multiplier + " * " + food.quality.multiplier + " = " + foodQuality
		     //                                       + "\n"+
			foodQuality + ": "+commentary );
		InterfaceController.Instance.WriteToScore(score);
		                                     
		GameController.Instance.NextPrompt();
	}

	public void DontEat()
	{
		InterfaceController.Instance.WriteToOutcome("Didn't eat");
		GameController.Instance.NextPrompt();
	}
}
