using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
public class TTONetworkManager : MonoBehaviour
{
    public GameObject checker;
    int myReliableChannelId;

    int socketId, socketPort = 8888, connectionId;
    [SerializeField]
    Text ipText, errorOutput;
    [SerializeField]
    InputField ipInput;

    [SerializeField]
	bool isHost = false;
		public bool networked = false;

    #region Instance
    private static TTONetworkManager instance;
    public static TTONetworkManager INSTANCE
    {
        get
        {
            if(instance == null)
            {
                GameObject TTONetworkManagerObj = new GameObject("TTONetworkManager");
                instance = TTONetworkManagerObj.AddComponent<TTONetworkManager>();
            }
            return instance;
        }
    }
    #endregion
    void Start()
    {
         if(instance != null)
         {
             DestroyImmediate(this.gameObject);
         }
         else
         {
             instance = this;
             DontDestroyOnLoad(this.gameObject);
         }


         InitializeNetwork();

    }

    public void InitializeNetwork()
    {
        NetworkTransport.Init();
        ipText.text = Network.player.ipAddress;
        ConnectionConfig conConf = new ConnectionConfig();

        myReliableChannelId = conConf.AddChannel(QosType.Reliable);

        int maxConnections = 4;
        HostTopology topology = new HostTopology(conConf, maxConnections);

        socketId = NetworkTransport.AddHost(topology, socketPort);
        Debug.Log("Socket Open. SocketId is: " + socketId);
    }

    public void Connect()
    {
        byte error;
        string ip = ipInput.text;
        Debug.Log("IP Text Input: " + ip);
        connectionId = NetworkTransport.Connect(socketId, ip, socketPort, 0, out error);
        if(error != (byte)NetworkError.Ok)
        {
            
        }
        Debug.Log("ConnectionId: " + connectionId);
    }

    public void Connect(string IP)
    {
        byte error;
        connectionId = NetworkTransport.Connect(socketId, IP, socketPort, 0, out error);
        Debug.Log("ConnectionId: " + connectionId);
    }
   
    public void ConnectAsHost()
    {

        //Being host just needs to be set to true, you don't need to connect to yourself as host. Just connect to others when they connect to you.
        isHost = true;
      
		networked = true;
        
        //  byte error;
      //  string ipAddress = Network.player.ipAddress;
      ////  connectionId = NetworkTransport.Connect(socketId, ipAddress, socketPort, 0, out error);
        
    }

    public void SendSocketMessage()
    {
        byte error;
        byte[] buffer = new byte[1024];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, "MessageSending");

        int bufferSize = 1024;

        NetworkTransport.Send(socketId, connectionId, myReliableChannelId, buffer, bufferSize, out error);
    }

    public void SendSocketMessage(string message)
    {
        byte error;
        byte[] buffer = new byte[1024];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, message);

        int bufferSize = 1024;

        NetworkTransport.Send(socketId, connectionId, myReliableChannelId, buffer, bufferSize, out error);
    }

	public void SendSetLocalPlayerIDMessage()
	{
		TTOMessageManager.INSTANCE.CreateMessage(TTOGameManager.INSTANCE.LocalPlayerID, TTOMessageManager.MessageType.SetLocalPlayerID, "1",null,null,0,null,null);

	}
    bool gotip = false;
    void Update()
    {
        int recHostId;
        int recConnectionId;
        int recChannelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);
        //Debug.Log("recHostId:"+recHostId+ " recConID:"+recConnectionId+" recChanID"+recChannelId);
        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("incoming connection event received");
                Debug.Log("recHostId:" + recHostId + " recConID:" + recConnectionId + " recChanID" + recChannelId);
                Debug.Log("ConID: " + connectionId);
                if (isHost)
                {
                    if (connectionId != recConnectionId)
                    {
                        Debug.Log("In Here");
                        // TTOMessageManager.INSTANCE.CreateMessage(1, TTOMessageManager.MessageType.RequestIP);
                    }
                }
                else
                {
                    if (!gotip)
                    {
                      //  TTOMessageManager.INSTANCE.CreateMessage(0, TTOMessageManager.MessageType.SendIP, Network.player.ipAddress);
                        gotip = true;
                    }
                }
                break;
            case NetworkEventType.DataEvent:
                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                string message = formatter.Deserialize(stream) as string;
                Debug.Log("incoming message event received: " + message);
//                TTOMessageManager.INSTANCE.DecipherMessage(message);
                
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("remote client event disconnected");
                break;
        }

    }

    
}