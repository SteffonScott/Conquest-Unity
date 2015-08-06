using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// using CielaSpike;
using System.Text;
using System;
using cqClient;
using System.Net;
using System.Diagnostics;
using UnityEngine.UI;

public class SocketScript : MonoBehaviour
{
	
	//variables
	public ConquestClient client;
	private string serverMsg;
	public string msgToServer;
	
	
	void Awake()
	{
		//add a copy of TCPConnection to this game object
		// Debug.Log ("TEST");
		client = gameObject.AddComponent<ConquestClient>();
		client._gameIp = IPAddress.Parse ("99.7.194.3");
		client._port = 9999;
		client.Connect ();
	}
	
	void Start()
	{
	}
	
	void Update()
	{
		
		//keep checking the server for messages, if a message is received from server, it gets logged in the Debug console (see function below)
		SocketResponse();
		
	}
	
	void OnGUI()
	{
		/*
        //if connection has not been made, display button to connect
        if (myTCP.socketReady == false) {
            0
            if (GUILayout.Button ("Connect")) {
                //try to connect
                Debug.Log("Attempting to connect..");
                myTCP.setupSocket();
            }
			
            }
			
            //once connection has been made, display editable text field with a button to send that string to the server (see function below)
            if (myTCP.socketReady == true) {
				
                msgToServer = GUILayout.TextField(msgToServer);
				
                if (GUILayout.Button ("Write to server", GUILayout.Height(30))) {
                    SendToServer(msgToServer);
                }
				
            }
        */
		
	}
	
	//socket reading script
	void SocketResponse()
	{
		
	}
	
	//send message to the server
	public void SendToServer(string str)
	{

	}
    public void Login()
    {
        client._playerName = GameObject.Find("inpt_username").GetComponent<InputField>().text;
        client._password = GameObject.Find("inpt_password").GetComponent<InputField>().text;
		client.Login ();
		if (client._loggedIn) {
			GameObject.Find ("pName").GetComponent<Text> ().text = String.Format (client._player.race + " " + client._player.level + " " + client._playerName);
			GameObject.Find ("pMovement").GetComponent<Text> ().text = Convert.ToString (client._player.movement);
			GameObject.Find ("pStructures").GetComponent<Text> ().text = Convert.ToString (client._player.structures); 
			GameObject.Find ("pFood").GetComponent<Text> ().text = Convert.ToString (client._player.food); 
			GameObject.Find ("pLand").GetComponent<Text> ().text = Convert.ToString (client._player.land); 
			GameObject.Find ("pPeasants").GetComponent<Text> ().text = Convert.ToString (client._player.peasants); 
			GameObject.Find ("pGold").GetComponent<Text> ().text = Convert.ToString (client._player.gold); 
		}
		// Debug.WriteLine ("Player name: {0}", client._player.name);
		/*if (client._loggedIn)
			Console.WriteLine("Player name: {0} - Structures: {1} - Movement: {2} - Gold: {3} - Land: {4} - Peasants: {5}", client._player.name, client._player.structures, client._player.movement, client._player.gold, client._player.land, client._player.peasants);
		*/
	}
	public void Newplayer()
	{ 
		// Debug.Log (GameObject.Find ("inpt_username").GetComponent<InputField> ().text);
		// Set player name based on value of input field "Username"
		if (GameObject.Find ("inpt_username").GetComponent<InputField> ().text.Length > 0 && GameObject.Find ("inpt_password").GetComponent<InputField> ().text.Length > 0) 
		{
			client.SetPlayerName (GameObject.Find ("inpt_username").GetComponent<InputField> ().text);
			client.SetPassword(GameObject.Find ("inpt_password").GetComponent<InputField> ().text);
		}
        client.Newplayer();
	}
    
}