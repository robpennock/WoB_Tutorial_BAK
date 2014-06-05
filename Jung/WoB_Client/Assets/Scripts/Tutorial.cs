using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tutorial : MonoBehaviour {
	private GameObject mainObject;
	// Window Properties
	private float width = 453;
	private float height = 200;
	// Other
	private Rect windowRect;
	private Rect skipButtonRect;
	private Rect nextButtonRect;
	private Rect minimizeButtonRect;
	private Vector2 scrollViewVector;
	private string tutorialText = Strings.ecosystemTutorialStrings[0];
	private int tutorialIndex = 0;
	private bool tutorialComplete = false;
	private bool readyForNextTutorial = true;
	private bool objectiveCompleted = false;
	private string message = "";
	private Vector2 lastQuadrant;
	private Vector2 lastPosition;
	private bool isHidden;
	private bool nextButtonHidden = false;
	private bool debugmode = true;
	public static int score;
	public GUISkin skin;
	public GameObject congratPicture;
	public GameObject awesomePicture;
	public int numberOfFails = 0;
	
	void Awake() {
		mainObject = GameObject.Find("MainObject");
		//mainObject.GetComponent<MessageQueue>().AddCallback(Constants.SMSG_CHAT, ResponseChat);
	}

	// Use this for initialization
	void Start () {
		if (!isHidden) {
			windowRect = new Rect(200, Screen.height/2 - height - 10, width, height);
		}

		scrollViewVector = Vector2.zero;
	}
	
	// Update is called once per frame
	void Update () 
	{

		if(tutorialIndex == 8) //First challenge
		{
			int speciesNumber = (new List<Species>(GetComponent<GameState>().speciesList.Values)).Count;



			if(score >= 3000 && speciesNumber >= 3)
			{

				Instantiate(congratPicture,congratPicture.transform.position , congratPicture.transform.rotation);// when user sucessful
				Instantiate(awesomePicture,awesomePicture.transform.position , awesomePicture.transform.rotation);// the images will display!

				objectiveCompleted = true;
				readyForNextTutorial = true;
			}else{
				//Instantiate(failPicture,failPicture.transform.position, congratPicture.transform.rotation);
				//tutorialIndex==9; // to display the failing messages to the box
				//if (GUI.Button(nextButtonRect, "Try Again?")
					// 
					// back to previous tutorial page before challenges
				// 
			}

			// we will add more! whether user is failing or sucessing on learning of this game.

		}
	}
	
	void OnGUI() {
		GUI.skin = skin;
		if (!isHidden) 
		{

			GUI.backgroundColor = new Color(0.0f , 0.0f , 0.0f , 1.0f); // black color
			Color color = GUI.color;
			color.a =1f; // transparency
			GUI.color = color;


			windowRect = GUI.Window((int)Constants.GUI_ID.Tutorial, windowRect, MakeWindow, "Tutorial");

			//GUI.Label(windowRect,Color.black);
		}

		else if (GUI.Button(new Rect(Screen.width - 260, Screen.height - 40, 120, 30), "Tutorial")) {
			isHidden = !isHidden;
		}
		/*if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return) {
			SendMessage();
		}*/
	}
	
	public void MakeWindow(int id) {
		scrollViewVector = GUI.BeginScrollView(new Rect(30, 30, width - 40, height - 110), scrollViewVector, new Rect(0, 0, 300, 400));
		/*if (tutorialIndex < 7) {
				GUI.contentColor = Color.white;
		} else if(tutorialIndex == 7){ 
			    GUI.contentColor = Color.white;
			    
				
		}*/ // My personal Testing so DO NOT MIND ME!


		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.font = (Font)Resources.Load("coopbl", typeof(Font)); // changing the font style and must be "Resources" folder!
		style.fontSize = 20; // Changing for size of letters
		GUI.Label(new Rect (0, 0, 380, 400), tutorialText, style); // more parameter "style" added
		GUI.contentColor = Color.white;
		GUI.EndScrollView();


		if (!readyForNextTutorial && !objectiveCompleted) 
			GUI.contentColor = Color.grey;
		//Would Like a way to grey out(disable) Next button when a task is required.
		nextButtonRect = new Rect(width - 45, height - 25, 40, 20);
		if (GUI.Button(nextButtonRect, "Next")) {
			if (readyForNextTutorial || objectiveCompleted) nextTutorial();
		}
		GUI.contentColor = Color.white; // might be string of color


		
		skipButtonRect = new Rect(width - 165, height - 25, 115, 20);
		if (GUI.Button(skipButtonRect, "Skip(Debug Only)")) {
			skipTutorial();
		}

		minimizeButtonRect = new Rect(width - 23, 3, 20, 20);
		if (GUI.Button(minimizeButtonRect, "-")) {
			isHidden = !isHidden;
		}
		GUI.DragWindow();
	}

	void nextTutorial()
	{
		if (readyForNextTutorial) 
		{
			readyForNextTutorial = Strings.ecosystemTutorialStringIsNextable[tutorialIndex];

			tutorialIndex++;

			/*if(tutorialIndex+1 >= Strings.ecosystemTutorialStrings.Length)
				readyForNextTutorial = false;*/
			if(tutorialIndex == Strings.ecosystemTutorialStrings.Length) 
			{
				tutorialComplete = true;

				RequestTutorialComplete(Constants.CMSG_ECOSYSTEM_TUTORIAL_COMPLETE);

			} 
			else
				tutorialText = Strings.ecosystemTutorialStrings[tutorialIndex];
		}
	}

	void skipTutorial()
	{
		readyForNextTutorial = true;
		nextTutorial();
	}

	public void RequestTutorialComplete(short tutorialRequest)
	{
	}
	public void ResponseTutorialComplete(ExtendedEventArgs eventArgs)
	{
	}
	/*
	public void SendMessage() {
		ConnectionManager cManager = mainObject.GetComponent<ConnectionManager>();
		
		if (cManager) {
			cManager.Send(RequestChat(0, message));
		}
	}

	public void SetMessage(string message) {
		innerText += "\n" + message;
	}
	*/
	/*
	public RequestChat RequestChat(short type, string message) {
		RequestChat request = new RequestChat();
		request.Send(type, message);
		
		return request;
	}
	
	public void ResponseChat(ExtendedEventArgs eventArgs) {
		ResponseChatEventArgs args = eventArgs as ResponseChatEventArgs;
		
		if (args.status == 0) {
			string message = "";
			
			if (args.type == 0) {
				message += "[" + args.username + "] says: ";
			}
			
			message += args.message;
			
			SetMessage(message);
		}
	}
	*/
}
