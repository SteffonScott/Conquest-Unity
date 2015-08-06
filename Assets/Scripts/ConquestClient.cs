using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using cqClient.Game_Objects;

namespace cqClient
{
	public class XMLMessage
	{
		public XMLMessage()
		{
			body = new List<string>();
		}
		
		public string id { get; set; }
		public List<string> body { get; set; }
	}
	public class XMLResponse
	{
		public string user { get; set; }
		public string token { get; set; }
		public string command { get; set; }
		public List<XMLMessage> message { get; set; }
        public DateTime timestamp { get; set; }

		public XMLResponse()
		{
            timestamp = DateTime.Now;
			message = new List<XMLMessage>();
		}
		
	}
	public class ConquestClient : MonoBehaviour
    {
        #region
		public string _playerName { get; set; }
		public IPAddress _gameIp { get; set; }
		public int _port { get; set; }
		private string _token { get; set; }
		public bool _loggedIn { get; set; }
		private Connection _connection { get; set; }
		private List<XMLResponse> _serverMessages { get; set; }
		public Player _player { get; set; }
        public string _password { get; set; }
        #endregion Properties
        public ConquestClient()
        {
            _playerName = "Player";
            _token = "0123456789012345678901234567890";
            _connection = new Connection(IPAddress.Parse("99.7.194.3"), 9999);
            _loggedIn = false;
            _serverMessages = new List<XMLResponse>();
            _player = new Player();
            _password = "password";

            //Initialize the events
            _connection.DataReceived += new Connection.delDataReceived(connection_DataReceived);
            _connection.ConnectionStatusChanged += new Connection.delConnectionStatusChanged(connection_ConnectionStatusChanged);
        }
        public ConquestClient(IPAddress gameIp, int port)
        {
            _playerName = "Player";
            _token = "0123456789012345678901234567890";
            _connection = new Connection(gameIp, port);
            _loggedIn = false;
            _serverMessages = new List<XMLResponse>();
            _player = new Player();
            _password = "password";

            //Initialize the events
            _connection.DataReceived += new Connection.delDataReceived(connection_DataReceived);
            _connection.ConnectionStatusChanged += new Connection.delConnectionStatusChanged(connection_ConnectionStatusChanged);
        }
		public ConquestClient(IPAddress gameIp, int port, string playerName = "Player")
		{
			_playerName = playerName;
			_token = "0123456789012345678901234567890";
			_connection = new Connection(gameIp, port);
			_loggedIn = false;
			_serverMessages = new List<XMLResponse>();
			_player = new Player();
            _password = "password";
			
			//Initialize the events
			_connection.DataReceived += new Connection.delDataReceived(connection_DataReceived);
			_connection.ConnectionStatusChanged += new Connection.delConnectionStatusChanged(connection_ConnectionStatusChanged);
		}
		public void SetPlayerName(string name)
		{
			_playerName = name;
		}
        public void SetPassword(string password)
        {
            _password = password;
        }
		public bool Connect()
		{
			_connection.Connect();
			do
			{
				var _delayTimer = new System.Timers.Timer();
				_delayTimer.Interval = 5000;
				_delayTimer.Start();
			}
			while (_connection.ConnectionState != Connection.ConnectionStatus.Connected);
			return (_connection.ConnectionState == Connection.ConnectionStatus.Connected);
		}
		/// <summary>
		/// Build XML command using string and send to server if connection is established.
		/// </summary>
		public void SendCommand(string _command, string[] _parameters = null)
		{
			string xmlCommand = BuildCommand(_command, _parameters);
			Byte[] data = System.Text.UTF8Encoding.ASCII.GetBytes(xmlCommand);
			if (this._connection.ConnectionState != Connection.ConnectionStatus.Connected)
				return;
			_connection.Send(xmlCommand);
		}
		/// <summary>
		/// Build XML object and format the string output to server specification
		/// </summary>
		public string BuildCommand(string _command, string[] _parameters = null)
		{
			int id = 1;
			XElement conquest = new XElement("Conquest");
			conquest.Add(new XElement("User", _playerName));
			conquest.Add(new XElement("Token", _token));
			conquest.Add(new XElement("Command", _command));
			if (_parameters != null)
			{
				foreach (string param in _parameters)
				{
					XElement e_parameter = new XElement("Parameter", param);
					e_parameter.Add(new XAttribute("Id", id));
					conquest.Add(e_parameter);
					id++;
				}
			}
			return String.Format(conquest.ToString(SaveOptions.DisableFormatting) + Environment.NewLine);
		}
		/// <summary>
		/// Login to the server with string parameter as password (character name selected on object instantiation)
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		public bool Login()
		{
			SendCommand("Validate", new string[] { _password });
            // Find the server response to the validate command.
            XMLResponse response = new XMLResponse();
			do { response = FindResponse("Validate"); }
			while (response == null);
            // Check if the incoming validate response indicates a successful login or not.
			if (response.message[0].id == "46")
			{
				_loggedIn = true;
				_token = response.token;
				UpdateStats();
				return true;
			}
			return false;
		}
        /// <summary>
        /// Create a new character with the name '_playerName'e
        /// </summary>
		public void Newplayer()
		{
			SendCommand ("Newplayer", new string[] { _password, "", "" } );
			// Find the server response to the newplayer command.
            XMLResponse response = new XMLResponse();
			do { response = FindResponse("Newplayer"); }
			while (response == null);
            // Retrieve initial player stats after creating character.
			UpdateStats();
		}
		/// <summary>
		/// Poqwpulate player object with stats from server
		/// </summary>
		public void UpdateStats()
		{
			SendCommand("Person");
            // Find the server response to the 'Person' command.
            XMLResponse response = new XMLResponse();
			do { response = FindResponse("Person"); }
			while (response == null);
			// Populate client-side player statistics via 'Person' response.
			_player.kingdomName = response.message[0].body[0];
			_player.race = response.message[1].body[0];
			_player.level = response.message[1].body[1];
			_player.name = response.message[1].body[2];
			_player.movement = Convert.ToInt32(response.message[2].body[0].Substring(0, response.message[2].body[0].IndexOf(" ")));
			_player.structures = Convert.ToInt32(response.message[2].body[1].Substring(0, response.message[2].body[1].IndexOf(" ")));
			_player.gold = Convert.ToInt32(response.message[2].body[2].Substring(0, response.message[2].body[2].IndexOf(" ")));
			_player.land = Convert.ToInt32(response.message[3].body[0].Substring(0, response.message[3].body[0].IndexOf(" ")));
			_player.peasants = Convert.ToInt32(response.message[3].body[1].Substring(0, response.message[3].body[1].IndexOf(" ")));
			_player.food = Convert.ToInt32(response.message[3].body[2].Substring(0, response.message[3].body[2].IndexOf(" ")));
			_player.city = response.message[5].body[0];
			_player.continent = response.message[5].body[1];
			_player.experience = Convert.ToInt32(response.message[6].body[0]);
			_player.protection = (response.message[6].body[1] == "YES") ? true : false;
			_player.taxes = response.message[6].body[2];
		}
        /// <summary>
        /// Find the response which cooresponse to the last command sent.
        /// </summary>
        /// <param name="command">Command to find the response for.</param>
        /// <returns></returns>
		public XMLResponse FindResponse(string command)
        {
            try
            {
                XMLResponse response = _serverMessages.OrderByDescending(e => e.timestamp).First(a => a.command == command);
                return response;
            }
			catch(Exception ex) { 
				Exception remerror = ex; 
				return null; 
			}
		}
        /// <summary>
        /// Convert raw XML string data from server into XMLResponse objects.
        /// </summary>
        /// <param name="_response">Raw XML string data</param>
        /// <returns></returns>
		public XMLResponse ParseResponse(string _response)
		{
			int bodyId = 0;
			int id = 0;
			XMLResponse response = new XMLResponse();
			XElement XML;
			XML = XElement.Parse(_response);
			response.user = XML.Element("User").Value;
			response.command = XML.Element("Command").Value;
			response.token = XML.Element("Token").Value;
			foreach (XElement msgElem in XML.Elements("Message"))
			{
				response.message.Add(new XMLMessage());
				response.message[id].id = msgElem.Attribute("Id").Value;
				foreach (XElement bodyElem in msgElem.Elements("Body"))
				{
					response.message[id].body.Add(bodyElem.Value);
					bodyId += 1;
				}
				bodyId = 0;
				id += 1;
			}
			return response;
		}
        /// <summary>
        /// Event which occurs when the status of the connection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="status"></param>
		void connection_ConnectionStatusChanged(Connection sender, Connection.ConnectionStatus status)
		{
			
		}
        /// <summary>
        /// When data is received over the connection, translates it into an 'XMLResponse' object and adds it to the incoming message hopper. (_searverMessages)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
		void connection_DataReceived(Connection sender, object data)
		{
			//Interpret the received data object as XMLReponse
			XMLResponse response = ParseResponse(data as string);
			_serverMessages.Add(response);
		}
	}
}

