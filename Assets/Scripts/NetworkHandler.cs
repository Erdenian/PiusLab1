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

	public int port = 1488;
    public TableController tableController;
    public Text log;

	private Data data;
	private TcpListener tcpListener;
	private Thread tcpListenerThread;
	private TcpClient connectedTcpClient;

	void Awake ()
	{
		// создаем параллельный поток для получения сообщений клиента
		tcpListenerThread = new Thread (new ThreadStart (ListenForIncommingRequests));  
		// делаем поток фоновым
		tcpListenerThread.IsBackground = true;
		// запускаем поток обработки
		tcpListenerThread.Start ();
	}

	void Update ()
	{ 		
		if (Input.GetKeyDown (KeyCode.Space)) {
			//SendMessage();
		} 	
	}

	//Получение данных для работы стэнда
	public Data GetData(){
		return data;
	}

	// поток обработки входящих сообщений
	private void ListenForIncommingRequests ()
	{ 		
		try {
			// запускаем сервер
			tcpListener = new TcpListener (IPAddress.Any, port); 			
			tcpListener.Start ();              
			Debug.Log ("Ожидаем сообщения...");              
			Byte[] bytes = new Byte[1024];  

			//обработка сообщений
			while (true) {
				string clientMessage = "";
				using (connectedTcpClient = tcpListener.AcceptTcpClient ()) {
					using (NetworkStream stream = connectedTcpClient.GetStream ()) { 						
						int length;  						
						while ((length = stream.Read (bytes, 0, bytes.Length)) != 0) { 							
							var incommingData = new byte[length]; 							
							Array.Copy (bytes, 0, incommingData, 0, length);							
							clientMessage += Encoding.UTF8.GetString (incommingData);
						}
						Debug.Log ("Получено сообщение: " + clientMessage);
                        try
                        {
							data = (Data)JsonUtility.FromJson<Data> (clientMessage);// разбор полученного сообщения и инициализация объекта
                            Debug.Log("Количество точек: " + data.points.Length);
                            tableController.StartCutting(data.reset, data.blockSize, data.knifeSize, data.points);
						}catch(Exception ex){
							Debug.Log ("Json поврежден. Данные не получены" + ex.Message);
                            log.text += "Json поврежден. Данные не получены." + ex.Message;
                        }
					} 				
				} 			
			} 		
		} catch (SocketException socketException) { 			
			Debug.Log ("Ошибка: " + socketException.ToString ());
            log.text += socketException.Message;
        }     
	}

	private void SendMessage ()
	{ 		
		if (connectedTcpClient == null) {             
			return;         
		}  		

		try {			
			NetworkStream stream = connectedTcpClient.GetStream (); 			
			if (stream.CanWrite) {                 
				string serverMessage = "Ответ сервера";                 
				byte[] serverMessageAsByteArray = Encoding.UTF8.GetBytes (serverMessage);
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length); 
				Debug.Log("Сервер отправил сообщение клиенту");
			}       
		} catch (SocketException socketException) {             
			Debug.Log ("Ошибка: " + socketException);
            log.text += socketException.Message;
        } 	
	}
}

//данные
[Serializable]
public class Data
{
	public bool reset;
	public Vector3 blockSize; // размеры бруска
	public Vector3 knifeSize; // размеры ножа
	public Vector3[] points; // точки траектории движения
}
