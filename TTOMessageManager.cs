using UnityEngine;
using System.Collections;

public class TTOMessageManager : MonoBehaviour {


    private static TTOMessageManager instance;
    public static TTOMessageManager INSTANCE
    {
        get
        {
            if(instance == null)
            {
                GameObject TTOMessageManagerObj = new GameObject("TTOMessageManager");
                instance = TTOMessageManagerObj.AddComponent<TTOMessageManager>();
            }
            return instance;
        }
    }
	
    public enum MessageType
    {
        SendIP,// = "SendIP",
        SetLocalPlayerID,
        RequestIP,// = "RequestIP",
        MoveUnit,// = "MoveUnit",
        Attack,// = "Attack",
        ChangeHealth,// = "ChangeUnitHealth",
        CreateUnit,// = "CreateUnit",
        DestroyUnit,// = "DestroyUnit",
        ChangeBuildingCapture,
        CaptureBuilding,// = "CaptureBuilding"
        ShowCube,
		EndGame
    }
    
    // Use this for initialization
	void Start () {
        if(instance != null)
        {
            DestroyImmediate(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
       
	}

    [SerializeField]
    GameObject cube;

    public void DecipherMessage(string message)
    {
        string[] messageArray = new string[10];
        messageArray = message.Split(',');
        Debug.Log("Message: "+messageArray[1]);
        MessageType type = (MessageType)System.Enum.Parse(typeof(MessageType), messageArray[1]);
        
        switch (type)
        {
            case MessageType.SendIP:
             //   TTONetworkManager.INSTANCE.Connect(messageArray[2]);
			//TODO: Sort out seperate player numbers
			     break;
			case MessageType.SetLocalPlayerID:
				TTOGameManager.INSTANCE.LocalPlayerID = System.Int32.Parse(messageArray[2]);
			//TTONetworkManager.INSTANCE.networked = true;
				break;
            case MessageType.RequestIP:
                CreateMessage(0, MessageType.SendIP, Network.player.ipAddress);
                break;
            case MessageType.MoveUnit:
                //Message: PlayerNo, MessageType, unitID, Current Tile X, Current Tile Y, Desired Tile X, Desired Tile Y
                int playerNumber = System.Int32.Parse(messageArray[0]);
                //TTOUnit unit = TTOGameManager.INSTANCE.GetUnitByID(System.Int32.Parse(messageArray[2]), playerNumber);
                TTOTile current = TTOGameManager.INSTANCE.GetTileByCoordinate(new Vector2(System.Int32.Parse(messageArray[3]),System.Int32.Parse(messageArray[4])));
				TTOUnit unit = current.TileUnit.GetComponent<TTOUnit>();    
				TTOTile desired = TTOGameManager.INSTANCE.GetTileByCoordinate(new Vector2(System.Int32.Parse(messageArray[5]), System.Int32.Parse(messageArray[6])));

                TTOGameManager.INSTANCE.MultiplayerMovement(unit, current, desired);
                break;
            case MessageType.Attack:
                //GameManager Attack Unit
                break;
            case MessageType.ChangeHealth:
                //GameManager change units health
			//Message: PlayerNo, MessageType UnitTileX, UnitTileY, AmountToChange
				TTOUnit unitTemp = TTOGameManager.INSTANCE.GetTileByCoordinate(new Vector2(System.Int32.Parse(messageArray[2]), 
				                                                                            System.Int32.Parse(messageArray[3]))).TileUnit.GetComponent<TTOUnit>();
				unitTemp.ChangeHealth(System.Int32.Parse(messageArray[4]));
			//TTOGameManager.INSTANCE.c
			break;
            case MessageType.CreateUnit:
                //GameManager Create Unit
                //Message: PlayerNo, MessageType, unitType, unitID, DesiredTileX, DesiredTileY
			Debug.Log (messageArray[0] + " " + messageArray[1] + " " + messageArray[2] + " " + messageArray[3] + " " + messageArray[4] + " " + messageArray[5]);
                TTOGameManager.INSTANCE.CreateUnit(TTOUnitManager.Instance.GetTypeByString(messageArray[2]), System.Int32.Parse(messageArray[0]),
                    TTOGameManager.INSTANCE.GetTileByCoordinate(new Vector2(System.Int32.Parse(messageArray[4]), 
                        System.Int32.Parse(messageArray[5]))));
                break;
            case MessageType.DestroyUnit:
                //Message: PlayerNo, MessageType, UnitCoordinateX, UnitCoordinateY
                var destroyUnitTempTile = TTOGameManager.INSTANCE.GetTileByCoordinate(new Vector2(System.Int32.Parse(messageArray[2]), 
                        System.Int32.Parse(messageArray[3])));
                destroyUnitTempTile.TileUnit.GetComponent<TTOUnit>().DestroyUnit();
                //GameManager Destroy Unit
                break;
            case MessageType.ChangeBuildingCapture:
                //Message: PlayerNo, MessageType, TileX, TileY, value
                var changeBuildingCaptureTempTile = TTOGameManager.INSTANCE.GetTileByCoordinate(new Vector2(System.Int32.Parse(messageArray[2]),
                        System.Int32.Parse(messageArray[3])));
                changeBuildingCaptureTempTile.GetComponent<TTOBuilding>().DecreaseCaptureValue(System.Int32.Parse(messageArray[4]), 
                    System.Int32.Parse(messageArray[0]));
                break;
            case MessageType.CaptureBuilding:
                break;
            case MessageType.ShowCube:
                if (cube.activeInHierarchy)
                    cube.SetActive(false);
                else
                    cube.SetActive(true);
                break;
			case MessageType.EndGame:
				//Message: PlayerNo, MessageType, PlayerVictory
				TTOGameManager.INSTANCE.HitEndGame(System.Int32.Parse(messageArray[2]));
				break;
            default:
                Debug.Log("Error");
                break;
        }

    }

    public void SendCubeMessage()
    {
        CreateMessage(0, MessageType.ShowCube);
    }

    bool CheckForIP(string s)
    {
        if (s.Contains("ipAddress"))
        {
            Debug.Log("true");
            return true;
        }
        return false;
    }

    //TODO: Change GameObjects to TTOUnits once Merge has occured
    public void CreateMessage(int playerNumber, MessageType type, string neededString = "", GameObject unit1 = null, 
        GameObject unit2 = null, int value = 0, TTOTile tile1 = null, TTOTile tile2 = null)
    {
        string messageString = "";
        switch (type)
        {
            case MessageType.SendIP:
                messageString = playerNumber.ToString()+ "," + MessageType.SendIP.ToString()+","+ neededString;
                break;
			case MessageType.SetLocalPlayerID:
			Debug.Log ("Set Local Player ID");
				messageString = playerNumber.ToString()+"," + MessageType.SetLocalPlayerID.ToString()+","+neededString;
			break;
            case MessageType.RequestIP:
                messageString = playerNumber.ToString() + "," + MessageType.RequestIP.ToString();
                break;
            case MessageType.MoveUnit:
                //Message: PlayerNo, MessageType, unitID, Current Tile X, Current Tile Y, Desired Tile X, Desired Tile Y
                messageString = playerNumber.ToString() + "," + MessageType.MoveUnit.ToString() + "," + neededString + "," + tile1.TileCoordinates.x.ToString()
                    + "," + tile1.TileCoordinates.y.ToString() + "," + tile2.TileCoordinates.x.ToString()
                    + "," + tile2.TileCoordinates.y.ToString();
                break;
            case MessageType.Attack:
                break;
            case MessageType.ChangeHealth:
			//Message: PlayerNo, MessageType UnitTileX, UnitTileY, AmountToChange
			messageString = playerNumber.ToString() +","+ MessageType.ChangeHealth.ToString()+","+tile1.TileCoordinates.x + ","+tile1.TileCoordinates.y +","+value;
                break;
            case MessageType.CreateUnit:
                //Message: PlayerNo, MessageType, unitType, unitID, DesiredTileX, DesiredTileY
                messageString = playerNumber.ToString() + "," + MessageType.CreateUnit.ToString() + "," + neededString + "," + 0 + "," + tile1.TileCoordinates.x.ToString()
                    + "," + tile1.TileCoordinates.y.ToString();
                break;
            case MessageType.DestroyUnit:
                break;
            case MessageType.CaptureBuilding:
                break;
            case MessageType.ShowCube:
                messageString = playerNumber.ToString() + "," + MessageType.ShowCube.ToString();
                break;
			
			case MessageType.ChangeBuildingCapture:
			//Message: PlayerNo, MessageType, TileX, TileY, value
				messageString = playerNumber.ToString() + "," +MessageType.ChangeBuildingCapture.ToString() + "," + tile1.TileCoordinates.x.ToString() 
				+ "," + tile1.TileCoordinates.y.ToString() + "," + value;
					
			break;
			case MessageType.EndGame:
				messageString = playerNumber.ToString() +","+MessageType.EndGame.ToString()+","+value;
			break;
            default:
                break;
        }
        //TTONetworkManager.INSTANCE.SendSocketMessage(messageString);
    }
}
