using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChefCharacterization : MonoBehaviour
{

	private static ChefCharacterization _instance;
	public static ChefCharacterization Instance {
		get {
			if(_instance == null) {
				_instance = FindObjectOfType<ChefCharacterization>();
			}
			return _instance;
		}
	}

	public int NumberOfRounds = 3;
	public int CurrentRound = 0;

	private const int GOOD_ANSWER_COUNT = 2;
	private const int NEUTRAL_ANSWER_COUNT = 8;
	private const int BAD_ANSWER_COUNT = 2;
	public Chef chef;
	public string Question; //User prompt

	public List<string> AnswerCriteria; //Text to search against tag types
	public List<FoodAttribute> Answers; //Populated from AnswerCriteria against tag data
	public List<FoodAttribute> GoodAnswers;
	public List<FoodAttribute> NeutralAnswers;
	public List<FoodAttribute> BadAnswers;

	public void Awake() {
		_instance = this;
	}

	public void Start ()
	{
		//Test RNG
//		List<int> hundredList = new List<int>();
//		for(int i = 0; i < 100; i++) {
//			hundredList.Add (i);
//		}
//		Dictionary<int, int> testDict = new Dictionary<int, int>();
//		for(int i = 0; i < 10000; i++) {
//			int hundredMember = Database.GetRandomCollectionMember(hundredList);
//			//print ("hundred list, hundred member: " + hundredList.Count + ", " + hundredMember);
//			if(testDict.ContainsKey(hundredMember)) {
//				testDict[hundredMember] ++;
//			} else {
//				testDict.Add (hundredMember, 1);
//			}
//		}
//		var sortedDict = testDict.OrderBy(pair => pair.Key);
//		foreach(KeyValuePair<int, int> pair in sortedDict) {
//			//print ("Id, count" + pair.Key + ", " + pair.Value);
//		}

		StartGame();




//		string uiString = "";
//		foreach (FoodAttribute answer in this.Answers) {
//				uiString += answer.Id + "\n";
//		}
		//ChefUi.Instance.WriteToText0 (uiString);
	}

	private void GenerateAnswers ()
	{
		this.Answers = new List<FoodAttribute> ();
		this.GoodAnswers = new List<FoodAttribute>();
		this.NeutralAnswers = new List<FoodAttribute>();
		this.BadAnswers = new List<FoodAttribute>();

		for (int i = 0; i < GOOD_ANSWER_COUNT; i++) {
			FoodAttribute assignableAttribute = GetUniqueAttribute (this.Answers, AttributeType.Form, this.chef.Liked [0]);
			//print ("Attribute id = " + assignableAttribute.Id);
			this.Answers.Add (assignableAttribute);
			this.GoodAnswers.Add (assignableAttribute);
		}
		for (int i = 0; i < NEUTRAL_ANSWER_COUNT; i++) {
			FoodAttribute assignableAttribute = GetUniqueAttribute (this.Answers, AttributeType.Form);
			this.Answers.Add (assignableAttribute);
			this.NeutralAnswers.Add (assignableAttribute);
		}
		for (int i = 0; i < BAD_ANSWER_COUNT; i++) {
			FoodAttribute assignableAttribute = GetUniqueAttribute (this.Answers, AttributeType.Form, this.chef.Disliked[0]);
			this.Answers.Add (assignableAttribute);
			this.BadAnswers.Add (assignableAttribute);
		}

		Database.Shuffle (this.Answers);
	}

	private FoodAttribute GetUniqueAttribute (List<FoodAttribute> destinationList, AttributeType targetType, string targetTagId = null)
	{
		FoodAttribute prospectiveAttribute = null;
		int i = 0;
		while (prospectiveAttribute == null || destinationList.Contains(prospectiveAttribute)) {
			if (targetTagId != null) {
				print ("i, target tag" + i + ", " + targetTagId);
				prospectiveAttribute = Database.GetRandomAttributeFromData (targetType, targetTagId);
			} else {
				prospectiveAttribute = Database.GetRandomAttributeFromData (targetType);
			}
			if (i > 100) {
				Debug.LogError ("Stack overflow");
				return null;
			}
			i++;
		}
		return prospectiveAttribute;
	}
	public void StartGame () {
		this.chef.Init();

		//Get possible nationalities
		List<Tag> nationalities = new List<Tag>();
		foreach(Tag tag in Database.Instance.TagData) {
			if(Database.TagListContainsId(nationalities, tag.Id) || tag.TagType != "nationality") {
				continue;
			}
			nationalities.Add(tag);
		}
		Database.Shuffle(nationalities);

		//Assign nationalities to chef
		this.chef.Liked.Add(nationalities[0].Id);
		this.chef.Disliked.Add(nationalities[1].Id);

		this.Next ();
	}

	public void Next () {
		//Check trial count and progress to next scene if applicable
		if(CurrentRound < NumberOfRounds) {
			//Progress to next round
			this.CurrentRound ++;
			GenerateAnswers ();

			print ("Displaying answers");
			ChefUi.Instance.DisplayAnswers(this.Answers.Select(a => a.Id).ToList());

		} else {
			EndGame();
		}
	}

	public void SubmitAttribute (string attributeId) {
		if(this.Answers.Where (a => a.Id == attributeId).Count () == 0) {
		//if(!this.Answers.Contains(attributeId)) {
			Debug.LogError("Attribute not contained in Answers");
		}
		//Save answer to chef's chosen list
		this.chef.ChosenAttributes.Add(attributeId);

		Next ();
	}

	public void EndGame() {

	}

}

