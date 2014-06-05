using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AOT;

public class Tutorial : MonoBehaviour {
	private GameObject mainObject;
	// Window Properties
	private float width = 620;
	private float height = 350;
	// Other
	private Rect windowRect;
	private Rect prevButtonRect;
	private Rect skipButtonRect;
	private Rect nextButtonRect;
	private Rect minimizeButtonRect;
	private Vector2 scrollViewVector;
	private string tutorialText;
	private int tutorialIndex;
	private bool tutorialComplete = false;
	private bool readyForNextTutorial = true;
	private bool objectiveCompleted = false;
	private string message = "";
	private Vector2 lastQuadrant;
	private Vector2 lastPosition;
	private bool isHidden;
	private bool nextButtonHidden = false;
	public static int score;
	public GUISkin skin;
	public GameObject congratPicture;
	public GameObject awesomePicture;
	private UnityEngine.Object congrat;
	private UnityEngine.Object awesome;
	private bool debugMode = true;

	private int[] speciesList;
	private bool initializedShop = false;

	readonly private int challengeStartIndex = 6;
	readonly private int challengeEndIndex = 9;

	private int challengeRequiredBiomass;
	private int challengeCreditsGiven;
	private int challengeRequiredEnvironmentScore;
	private int challengeTimeLimit;
	private int challengeMinimumSpecies;

	// Variables to keep track of days passed on a challenge.
	private int initialDay = 0;
	private int daysPassed = 0;
	private int daysLeft = 0; // Just used to display to the user.
	private bool initializedDay = false;
	private bool removedSpecies = false;

	private bool instantiatedPics = false;
	private Rect resetTutorial;

	// Represents the tutorial index the player is on.
	// This is retrieved through RequestTutorial protocol.
	private int currentTutorial; 

	private int currentChallenge;

	private bool isTutorialInitialized = false;

	private bool requestedChallengeInfo = false;
	private bool initializedChallengeInfo = false;

	private bool failedChallenge = false;
	private string failedChallengeMsg = "You have <color=#ffa500ff>failed</color> to complete the challenge in the allotted amount of time. Click <color=#ffa500ff>OK</color> to try again.";
	private string objectiveCompletedMsg = "You have <color=#00ff00ff>completed</color> the challenge! Click <color=#00ff00ff>OK</color> to continue.";

	private int biomass;
	private int speciesNumber = 0;

	public int currentTutorialCredits { get; set;} 

	public bool currentlyOnChallenge { get; set; } // Used by tutorial shop.

	void Awake() {
		mainObject = GameObject.Find("MainObject");
		gameObject.AddComponent("TutorialShop");
		//mainObject.GetComponent<MessageQueue>().AddCallback(Constants.SMSG_CHAT, ResponseChat);
		mainObject.GetComponent<MessageQueue>().AddCallback(Constants.SMSG_TUTORIAL_CHALLENGE_SPECIES, ResponseChallengeShopSpecies);
		mainObject.GetComponent<MessageQueue>().AddCallback(Constants.SMSG_TUTORIAL_CHALLENGE, ResponseChallenge);
		mainObject.GetComponent<MessageQueue>().AddCallback(Constants.SMSG_TUTORIAL_REMOVE_SPECIES, ResponseTutorialRemoveSpecies);
		mainObject.GetComponent<MessageQueue>().AddCallback(Constants.SMSG_TUTORIAL_DATA, ResponseTutorial);
		mainObject.GetComponent<MessageQueue>().AddCallback(Constants.SMSG_TUTORIAL_UPDATE_CUR_TUT, ResponseUpdateCurTut);
	}

	void Start () {
		currentTutorialCredits = 0;
		currentlyOnChallenge = false;

		if (!isHidden) {
			windowRect = new Rect((Screen.width - width)/2, Screen.height - height * 1.5f, width, height);
		}

		scrollViewVector = Vector2.zero;

		ConnectionManager cManager = mainObject.GetComponent<ConnectionManager>();
		if (cManager) {
			cManager.Send(RequestTutorial(Constants.USER_ID));
		}

	}
	
	void Update () 
	{

		if(isTutorialInitialized) {
			score = GetComponent<EnvironmentScore>().GetScore();
			biomass = gameObject.GetComponent<GameState>().getTotalBiomass();

			if (tutorialComplete || tutorialIndex == 10 && !instantiatedPics) 
			{
				instantiatedPics = true;
				showVictoryPics();
			}

			if (tutorialIndex < challengeStartIndex || tutorialIndex > challengeEndIndex) {
				if(!initializedShop) {
					initializeShopAll();
				}
			}

			//Challenges
			if(tutorialIndex >= challengeStartIndex && tutorialIndex <= challengeEndIndex)
			{

				if(!currentlyOnChallenge) {
					currentlyOnChallenge = true;
				}

				if(initializedChallengeInfo && !failedChallenge) {

					if(!initializedDay) {
						currentChallenge = 1 + (tutorialIndex - challengeStartIndex);
						initializeDay();
					}

					if(!removedSpecies) {
						resetSpecies();
						removedSpecies = true;
					}

					if (!objectiveCompleted) {
						daysPassed = getCurrentDay() - initialDay;
					}

					// If daysPassed reaches the days allowed for the challege,
					// all species and env. score are reset for the zone
					// NOTE: NOT SURE THAT THE challengeTimeLimit IS MEASURED IN DAYS
					// YOU MAY HAVE TO ADJUST FOR THIS TO MAKE THIS CODE WORK
					if (daysPassed == challengeTimeLimit) {
						daysPassed = 0;
						initializeDay();
						resetSpecies();
						failedChallenge = true;
						tutorialText = failedChallengeMsg;
					}

					daysLeft = challengeTimeLimit - daysPassed;

					if(!initializedShop) {
						ConnectionManager sManager = mainObject.GetComponent<ConnectionManager>();
						if (sManager) {
							//Not sure if the coorect value here is tutorialIndex or (tutorialIndex -tutorialStartIndex)
							sManager.Send(RequestChallengeShopSpecies((short)tutorialIndex));
							initializedShop = true;
						}
					}
					/*
					try {
						speciesNumber = (new List<Species>(GetComponent<GameState>().speciesList.Values)).Count;
					} catch (NullReferenceException e) {
						Debug.Log(e.ToString());
					}*/
					speciesNumber = gameObject.GetComponent<GameState>().getSpeciesCount();

					if(score >= challengeRequiredEnvironmentScore && biomass >= challengeRequiredBiomass && speciesNumber >= challengeMinimumSpecies)
					{
						objectiveCompleted = true;
						readyForNextTutorial = true;
						tutorialText = objectiveCompletedMsg;
					}
				}

				else if (!initializedChallengeInfo && !requestedChallengeInfo) {
					ConnectionManager cManager = mainObject.GetComponent<ConnectionManager>();
					cManager.Send(RequestChallenge((short)tutorialIndex));
					requestedChallengeInfo = true;
				}

			}
		}
	
	}
	
	void OnGUI() {
		GUI.skin = skin;

		GUI.BeginGroup(new Rect(Screen.width / 2 - 100, 145, 200, 100));
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.font = (Font)Resources.Load("coopbl", typeof(Font)); 
		style.fontSize = 20;
		style.alignment = TextAnchor.UpperCenter;

		if(!currentlyOnChallenge) {		
			ExtraMethods.DrawOutline(new Rect(0, 0, 200, 50), "Current Biomass", style, Color.black, new Color(1.0f, 0.93f, 0.73f, 1.0f));

			style.font = (Font)Resources.Load("coopbl", typeof(Font)); 
			style.fontSize = 24;
			style.alignment = TextAnchor.UpperCenter;

			ExtraMethods.DrawOutline(new Rect(0, 25, 200, 50), biomass.ToString("n0"), style, Color.black, Color.white);
			
		}
		GUI.EndGroup();
		
		// Display challenge info
		if (tutorialIndex >= challengeStartIndex && tutorialIndex <= challengeEndIndex) {
			if (!objectiveCompleted && !failedChallenge) {
				Rect rect1 = new Rect(Screen.width - 250, 145, 240, 40);
				Rect rect2 = new Rect(Screen.width - 255, 175, 200, 40);
				Rect rect3 = new Rect(Screen.width - 145, 175, 200, 40);
				Rect rect4 = new Rect(Screen.width - 245, 205, 200, 40);
				Rect rect5 = new Rect(Screen.width - 135, 205, 200, 40);
				Rect rect6 = new Rect(Screen.width - 288, 235, 200, 40);
				Rect rect7 = new Rect(Screen.width - 145, 238, 200, 40);
				Rect rect8 = new Rect(Screen.width - 285, 265, 200, 40);
				Rect rect9 = new Rect(Screen.width - 145, 268, 200, 40);
				Rect rect10 = new Rect(Screen.width - 295, 295, 200, 40);
				Rect rect11 = new Rect(Screen.width - 155, 298, 200, 40);

				style.normal.textColor = Color.red;
				style.font = (Font)Resources.Load("coopbl", typeof(Font)); 
				style.fontSize = 21; 
				ExtraMethods.DrawOutline(rect1, "Current Challenge:  " + currentChallenge, style, Color.black, new Color(1.0f, 0.93f, 0.73f, 1.0f));
				style.fontSize = 17; 
				ExtraMethods.DrawOutline(rect2, "Days remaining: ", style, Color.black, Color.white);
				ExtraMethods.DrawOutline(rect3, daysLeft.ToString(), style, Color.white, Color.red);
				ExtraMethods.DrawOutline(rect4, "Credits Remaining: ", style, Color.black, Color.white);
				ExtraMethods.DrawOutline(rect5, currentTutorialCredits.ToString(), style, Color.black, Color.green);
				ExtraMethods.DrawOutline(rect6, "Species: ", style, Color.black, Color.white);

				if (speciesNumber >= challengeMinimumSpecies) {
					GUI.Label(rect7, "<color=#008000ff><b> " + speciesNumber.ToString()  + "/" + challengeMinimumSpecies.ToString() + "</b></color>");
				} else {
					GUI.Label(rect7, "<color=#000000><b> " + speciesNumber.ToString()  + "/" + challengeMinimumSpecies.ToString() + "</b></color>");
				}

				ExtraMethods.DrawOutline(rect8, "Biomass:", style, Color.black, Color.white);

				if (biomass >= challengeRequiredBiomass) {
					GUI.Label(rect9, "<color=#008000ff><b>   " + biomass.ToString("n0") + "/" + challengeRequiredBiomass.ToString("n0") + "</b></color>");
					} else {
						GUI.Label(rect9, "<color=#000000><b>   " + biomass.ToString("n0") + "/" + challengeRequiredBiomass.ToString("n0") + "</b></color>");
				}
				
				ExtraMethods.DrawOutline(rect10, "Score: ", style, Color.black, Color.white);

				if (score >= challengeRequiredEnvironmentScore) {
					GUI.Label(rect11, "<color=#008000ff><b> " + score.ToString("n0") + "/" + challengeRequiredEnvironmentScore.ToString("n0") + "</b></color>");					
					} else {
						GUI.Label(rect11, "<color=#000000><b> " + score.ToString("n0") + "/" + challengeRequiredEnvironmentScore.ToString("n0") + "</b></color>");
				}

				


			} else if (objectiveCompleted && !instantiatedPics) {
				showVictoryPics();
			}

		}

		Rect increaseScore = new Rect(Screen.width - 220, 360, 200, 35);
		if(GUI.Button(increaseScore, "Env Score += 1000 (Debug)")) {
			score += 1000;
			GetComponent<EnvironmentScore>().SetScore(score);
		}

		resetTutorial = new Rect(Screen.width - 220, 400, 200, 35);
		if(GUI.Button(resetTutorial, "Reset Tutorial (Debug)")) {
			tutorialIndex = 0;
			tutorialText = Strings.ecosystemTutorialStrings[0];
			tutorialComplete = false;
			objectiveCompleted = false;
			readyForNextTutorial = Strings.ecosystemTutorialStringIsNextable[0];
			initializedShop = false;
			currentlyOnChallenge = false;
			failedChallenge = false;
			removeVictoryPics();
			resetSpecies();
			initializeShopAll();
			ConnectionManager cManager = mainObject.GetComponent<ConnectionManager>();
			if (cManager) {
				cManager.Send(RequestUpdateCurTut(Constants.USER_ID, 0));
			}
		}

		Rect failTheChallenge = new Rect(Screen.width - 220, 440, 200, 35);
		if(currentlyOnChallenge && GUI.Button(failTheChallenge, "Fail The Challenge (Debug)")) {
			daysPassed = 0;
			initializeDay();
			resetSpecies();
			failedChallenge = true;
			tutorialText = failedChallengeMsg;
			if(instantiatedPics) {
				removeVictoryPics();
				instantiatedPics = false;
			}
		}

		Rect completeChallengeButtonRect = new Rect(Screen.width - 220, 480, 200, 35);
		if (tutorialIndex >= challengeStartIndex && tutorialIndex <= challengeEndIndex &&
			GUI.Button(completeChallengeButtonRect, "Complete Challenge(Debug)")) {
			objectiveCompleted = true;
			readyForNextTutorial = true;
			tutorialText = objectiveCompletedMsg;
		}

		if (isHidden && (failedChallenge || objectiveCompleted)) {
			isHidden = !isHidden;
		}

		GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
		btnStyle.font = (Font)Resources.Load("coopbl", typeof(Font));

		if (!isHidden) 
		{
			GUI.backgroundColor = new Color(0.0f , 0.0f , 0.0f , 1.0f); // black color
			Color color = GUI.color;
			color.a =1f; // transparency
			GUI.color = color;

			windowRect = GUI.Window((int)Constants.GUI_ID.Tutorial, windowRect, MakeWindow, Strings.tutorialTitles[tutorialIndex]);
		}

		else if (GUI.Button(new Rect(Screen.width - 260, Screen.height - 40, 120, 30), "Tutorial", btnStyle)) {
			isHidden = !isHidden;
		}
		/*if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return) {
			SendMessage();
		}*/
	}

	/* Kenneth Roberton, 4/30/2014
	 * With the text size increased I think the tutorial box needs to be increased in size, particularly in height.
	 * Also If possible I would like it if the tutorial box defaulted to being on top of other windows,
	 * so that, by default, the tutorial box would display infront of the shop box.
	 * Making the tutorial button in the bottom left side of the screen permenant, so that when the box is open
	 * the tutorial box minimizes and then when clicked again reappears in its original starting location
	 * might help make it easier for the player to get a hold of it to.
	*/
	
	public void MakeWindow(int id) {

		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.font = (Font)Resources.Load("coopbl", typeof(Font)); // changing the font style and must be "Resources" folder!
		style.fontSize = 16; // Changing for size of letters
		GUI.Label(new Rect (30, 30, 555, 350), tutorialText, style); // more parameter "style" added

		GUI.contentColor = Color.white;

		
		
		if (!readyForNextTutorial && !objectiveCompleted) 
			GUI.contentColor = Color.grey;
		//Would Like a way to grey out(disable) Next button when a task is required.
		nextButtonRect = new Rect(width - 45, height - 25, 40, 20);

		if (!failedChallenge && !objectiveCompleted && tutorialIndex != Strings.ecosystemTutorialStrings.Length - 1) {
			if (GUI.Button(nextButtonRect, "Next")) {
				if (readyForNextTutorial) nextTutorial();
			}
			GUI.contentColor = Color.white; // might be string of color
		} else if (failedChallenge) {
			GUI.contentColor = Color.white;
			if (GUI.Button(nextButtonRect, "OK")) {
				failedChallenge = false;
				objectiveCompleted = false;
				initializedShop = false;
				initializedDay = false;
				removedSpecies = false;
				ConnectionManager cManager = mainObject.GetComponent<ConnectionManager>();
				cManager.Send(RequestChallenge((short)tutorialIndex));

				tutorialText = Strings.ecosystemTutorialStrings[tutorialIndex];
			}
		} else if (objectiveCompleted) {
			if (GUI.Button(nextButtonRect, "OK")) {
				failedChallenge = false;
				objectiveCompleted = false;
				initializedShop = false;
				initializedDay = false;
				removedSpecies = false;
				nextTutorial();
			}
		}

		Rect prevButtonRect = new Rect(width - 135, height - 25, 80, 20);
		if ((tutorialIndex > 0 && tutorialIndex < challengeStartIndex) &&
			GUI.Button(prevButtonRect, "Previous")) {
			previousTutorial();
		}

		skipButtonRect = new Rect(width - 255, height - 25, 115, 20);
		if (tutorialIndex != Strings.ecosystemTutorialStrings.Length - 1 && GUI.Button(skipButtonRect, "Skip(Debug Only)")) {
			skipTutorial();
		}
		
		minimizeButtonRect = new Rect(width - 23, 3, 20, 20);
		if (GUI.Button(minimizeButtonRect, "-")) {
			isHidden = !isHidden;
		}

		GUI.DragWindow();
	}

	void previousTutorial() {
		tutorialIndex--;
		tutorialText = Strings.ecosystemTutorialStrings[tutorialIndex];
		readyForNextTutorial = Strings.ecosystemTutorialStringIsNextable[tutorialIndex];

		ConnectionManager cManager = mainObject.GetComponent<ConnectionManager>();
		if (cManager) {
			cManager.Send(RequestUpdateCurTut(Constants.USER_ID, tutorialIndex));
		}
	}

	void nextTutorial()
	{
		objectiveCompleted = false;
		initializedShop = false;
		initializedDay = false;
		removedSpecies = false;
		instantiatedPics = false;
		currentlyOnChallenge = false;
		removeVictoryPics();

		if (readyForNextTutorial) 
		{
			tutorialIndex++;
			readyForNextTutorial = Strings.ecosystemTutorialStringIsNextable[tutorialIndex];
			currentTutorial = tutorialIndex;

			if(tutorialIndex == Strings.ecosystemTutorialStrings.Length) 
			{
				tutorialComplete = true;

				RequestTutorialComplete(Constants.CMSG_ECOSYSTEM_TUTORIAL_COMPLETE);
			} 
			else
			tutorialText = Strings.ecosystemTutorialStrings[tutorialIndex];

			ConnectionManager cManager = mainObject.GetComponent<ConnectionManager>();
			if (cManager) {
				cManager.Send(RequestUpdateCurTut(Constants.USER_ID, currentTutorial));

				//This is where the challengeData gets updated for the next challenge tutorial
				if(tutorialIndex >= challengeStartIndex && tutorialIndex <= challengeEndIndex)
				{
					cManager.Send(RequestChallenge((short)tutorialIndex));
				}
			}

		}
	}

		void skipTutorial()
		{
			readyForNextTutorial = true;
			nextTutorial();
		}

		public RequestChallengeShopSpecies RequestChallengeShopSpecies(short challenge) 
		{

			RequestChallengeShopSpecies request = new RequestChallengeShopSpecies();
			request.Send(challenge);
			
			return request;
		}
		
		public void ResponseChallengeShopSpecies(ExtendedEventArgs eventArgs) {
			ResponseChallengeShopSpeciesEventArgs args = eventArgs as ResponseChallengeShopSpeciesEventArgs;
			
			speciesList = args.speciesList;
			
			if (speciesList == null || speciesList.Length <= 0) {
				Debug.Log("Failed to load Challenge Shop Species");
			}
			else 
			{
				for (int i = 0; i < speciesList.Length; i++) 
					Debug.Log("species index " + i + ": speciesID = " + speciesList[i]);

				initializeShop();
			}
		}

		public RequestChallenge RequestChallenge(short challenge) 
		{

			RequestChallenge request = new RequestChallenge();
			request.Send(challenge);
			
			return request;
		}
		
		public void ResponseChallenge(ExtendedEventArgs eventArgs) {
			ResponseChallengeEventArgs args = eventArgs as ResponseChallengeEventArgs;
			
			challengeRequiredBiomass = args.requiredBiomass;
			challengeCreditsGiven = args.creditsGiven;
			challengeRequiredEnvironmentScore = args.requiredEnvironmentScore;
			challengeTimeLimit = args.timeLimit;
			challengeMinimumSpecies = args.minSpecies;

			currentTutorialCredits = challengeCreditsGiven;

			gameObject.GetComponent<TutorialShopCartPanel>().tutorialCreditsBeforePurchase = currentTutorialCredits;

			initializedChallengeInfo = true;
		}

		public RequestTutorialRemoveSpecies RequestTutorialRemoveSpecies() {
			RequestTutorialRemoveSpecies request = new RequestTutorialRemoveSpecies();
			request.Send();

			return request;
		}

		public void ResponseTutorialRemoveSpecies(ExtendedEventArgs eventArgs) {
			ResponseTutorialRemoveSpeciesEventArgs args = eventArgs as ResponseTutorialRemoveSpeciesEventArgs;
			gameObject.GetComponent<GameState>().resetSpecies();
		}

		public RequestTutorial RequestTutorial(int id) {
				RequestTutorial request = new RequestTutorial();
				request.Send(id);

				return request;
		}

		public void ResponseTutorial(ExtendedEventArgs eventArgs) {
			ResponseTutorialEventArgs args = eventArgs as ResponseTutorialEventArgs;

			currentTutorial = args.currentTutorial;
			tutorialIndex = currentTutorial;
			tutorialText = Strings.ecosystemTutorialStrings[tutorialIndex];
			readyForNextTutorial = Strings.ecosystemTutorialStringIsNextable[tutorialIndex];
			initializeShopAll();
			objectiveCompleted = false;
			isTutorialInitialized = true;
		}

		public RequestUpdateCurTut RequestUpdateCurTut(int id, int tutorialIndex) {

			RequestUpdateCurTut request = new RequestUpdateCurTut();
			request.Send(id, tutorialIndex);

			return request;
		}

		public void ResponseUpdateCurTut(ExtendedEventArgs eventArgs) {
			ResponseUpdateCurTutEventArgs args = eventArgs as ResponseUpdateCurTutEventArgs;
		}

		public void initializeShop() {
			gameObject.GetComponent<TutorialShop>().selectedSpecies = null;
			gameObject.GetComponent<TutorialShopCartPanel>().ResetCartList();
			gameObject.GetComponent<TutorialShop>().Initialize(null, speciesList);
		}

		public void initializeShopAll() {
			gameObject.GetComponent<TutorialShop>().selectedSpecies = null;
			gameObject.GetComponent<TutorialShopCartPanel>().ResetCartList();
			gameObject.GetComponent<TutorialShop>().InitializeAll();
			if (gameObject.GetComponent<TutorialShop>().entireSpeciesList.Count == 0) {
				initializedShop = false;
			} else {
				initializedShop = true;
			}
		}

		private void initializeDay() {
			initialDay = (gameObject.GetComponent<Clock>().second / (Constants.MONTH_DURATION / 30));	
			if (initialDay == 0) {
				initializedDay = false;
			}	else {
				initializedDay = true;
			}
		}

		private int getCurrentDay() {
			return (gameObject.GetComponent<Clock>().second / (Constants.MONTH_DURATION / 30));
		}

		public void resetSpecies() {
			gameObject.GetComponent<GameState>().resetSpecies();
			gameObject.GetComponent<TutorialShopCartPanel>().resetSpecies = true;
			ConnectionManager cManager = mainObject.GetComponent<ConnectionManager>();
			if (cManager) {
				cManager.Send(RequestTutorialRemoveSpecies());
			}
		}

		public void RequestTutorialComplete(short tutorialRequest)
		{
		}
		public void ResponseTutorialComplete(ExtendedEventArgs eventArgs)
		{
		}

		void showVictoryPics() {
			instantiatedPics = true;
			congrat = Instantiate(congratPicture,congratPicture.transform.position , congratPicture.transform.rotation);// when user sucessful
			awesome = Instantiate(awesomePicture,awesomePicture.transform.position , awesomePicture.transform.rotation);// the images will display!
		}

		void removeVictoryPics() {
			Destroy(congrat);
			Destroy(awesome);
			instantiatedPics = false;
		}
}