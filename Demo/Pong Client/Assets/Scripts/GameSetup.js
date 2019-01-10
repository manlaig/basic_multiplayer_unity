#pragma strict

//Reference the camera
var mainCam : Camera;

//Reference the colliders we are going to adjust
var topWall : BoxCollider2D;
var bottomWall : BoxCollider2D;
var leftWall : BoxCollider2D;
var rightWall : BoxCollider2D;

//Reference the players
var Player01 : Transform;
var Player02 : Transform;

function Start () { //Only set this to Update if you know the screen size can change during a playsession.

	//Move each wall to its edge location:
	topWall.size = new Vector2 (mainCam.ScreenToWorldPoint (new Vector3 (Screen.width * 2f, 0f, 0f)).x, 1f);
	topWall.center = new Vector2 (0f, mainCam.ScreenToWorldPoint (new Vector3 ( 0f, Screen.height, 0f)).y + 0.5f);
	
	bottomWall.size = new Vector2 (mainCam.ScreenToWorldPoint (new Vector3 (Screen.width * 2, 0f, 0f)).x, 1f);
	bottomWall.center = new Vector2 (0f, mainCam.ScreenToWorldPoint (new Vector3( 0f, 0f, 0f)).y - 0.5f);
	
	leftWall.size = new Vector2(1f, mainCam.ScreenToWorldPoint(new Vector3(0f, Screen.height*2f, 0f)).y);;
	leftWall.center = new Vector2(mainCam.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x - 0.5f, 0f);
	
	rightWall.size = new Vector2(1f, mainCam.ScreenToWorldPoint(new Vector3(0f, Screen.height*2f, 0f)).y);
	rightWall.center = new Vector2(mainCam.ScreenToWorldPoint(new Vector3(Screen.width, 0f, 0f)).x + 0.5f, 0f);

	//Move the players to a fixed distance from the edges of the screen:
	Player01.position.x = mainCam.ScreenToWorldPoint (new Vector3 (75f, 0f, 0f)).x;
	Player02.position.x = mainCam.ScreenToWorldPoint (new Vector3 (Screen.width -75f, 0f, 0f)).x;
}