#pragma strict

var Speed : double;
private var isVisible: boolean;
private var cdTime: int;

function Start () {
	cdTime = 500;
}

function Update () {
	transform.position.x += Speed;
	cdTime--;
	
	//Accounts for load time of the objects where this code would treat the clouds as non-visible
	if(cdTime < 0) {
		if (!renderer.isVisible){
			transform.position.x -= 2000;
			cdTime = 500;
		}
	}
}