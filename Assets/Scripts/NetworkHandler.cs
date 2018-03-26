using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class NetworkHandler : MonoBehaviour
{

	private string mess = "test";
	public int port = 1488;

	private string js = "{" +
	                    "\"reset\": false," +
	                    "\"blockSize\": {\"x\": 1.0,\"y\": 1.0,\"z\": 1.0}," +
	                    "\"knifeSize\": {\"x\": 1.0,\"y\": 1.0,\"z\": 1.0}," +
	                    "\"points\": [" +
	                    "{\"x\": 1.0,\"y\": 1.0,\"z\": 1.0}," +
	                    "{\"x\": 1.0,\"y\": 1.0,\"z\": 1.0}," +
	                    "{\"x\": 1.0,\"y\": 1.0,\"z\": 1.0}" +
	                    "]" +
	                    "}";


	private TcpListener tcpListener;
	private Thread tcpListenerThread;
	private TcpClient connectedTcpClient;

	void Awake ()
	{	
		//Debug.Log("1");
		tcpListenerThread = new Thread (new ThreadStart (ListenForIncommingRequests)); 	
		//Debug.Log("2");
		tcpListenerThread.IsBackground = true; 		
		//Debug.Log("3");
		tcpListenerThread.Start (); 	
		//Debug.Log("4");

		Data d = new Data ();
		d.reset = true;
		d.blockSize = Vector3.up;
		d.knifeSize = Vector3.left;
		d.points = new Vector3[] { Vector3.right, Vector3.forward };

		string jjj = JsonUtility.ToJson (d);
		Debug.Log (jjj);


		Data data = (Data)JsonUtility.FromJson<Data> (jjj);
		Debug.Log (data.blockSize);
	}

	void Update ()
	{ 		
		if (Input.GetKeyDown (KeyCode.Space)) {             
			//SendMessage();  
			Debug.Log ("ИБУ Я КАКТО ВАШУ МАТ");
		} 	
	}

	private void ListenForIncommingRequests ()
	{ 		
		try {		
			tcpListener = new TcpListener (IPAddress.Any, port); 			
			tcpListener.Start ();              
			Debug.Log ("КСТА СЛУШАЮ КАРОЧ ХУИТЫ ГОРСТЬ...");              
			Byte[] bytes = new Byte[1024];  			
			while (true) {
				string clientMessage = "";
				using (connectedTcpClient = tcpListener.AcceptTcpClient ()) {	
					Debug.Log ("adsf");
					using (NetworkStream stream = connectedTcpClient.GetStream ()) { 						
						int length;  						
						while ((length = stream.Read (bytes, 0, bytes.Length)) != 0) { 							
							var incommingData = new byte[length]; 							
							Array.Copy (bytes, 0, incommingData, 0, length);							
							clientMessage += Encoding.UTF8.GetString (incommingData);
						}
						Debug.Log ("ХУИТЫ ПОЛУЧИЛ КАРОЧ Э: " + clientMessage); 
						mess = clientMessage;
					} 				
				} 			
			} 		
		} catch (SocketException socketException) { 			
			Debug.Log ("ИБАТЬ ТВОЙ РОТ!: " + socketException.ToString ()); 		
		}     
	}

	/// <summary> 	
	/// Send message to client using socket connection. 	
	/// </summary> 	
	private void SendMessage ()
	{ 		
		if (connectedTcpClient == null) {             
			return;         
		}  		

		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = connectedTcpClient.GetStream (); 			
			if (stream.CanWrite) {                 
				string serverMessage = "This is a message from your server."; 			
				// Convert string message to byte array.                 
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes (serverMessage); 				
				// Write byte array to socketConnection stream.               
				stream.Write (serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);               
				Debug.Log ("Server sent his message - should be received by client");           
			}       
		} catch (SocketException socketException) {             
			Debug.Log ("ИБАТЬ ТВОЙ РОТ!!: " + socketException);         
		} 	
	}
}

[Serializable]
public class Data
{
	public bool reset;

	public Vector3 blockSize;

	public Vector3 knifeSize;

	public Vector3[] points;
}
