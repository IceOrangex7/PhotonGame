using UnityEngine;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using System.Collections.Generic;

public class MyClient : MonoBehaviour ,IPhotonPeerListener
{
    LoadBalancingPeer m_peer;
    string m_userName, m_message;
    Dictionary<int, string> m_actorList = new Dictionary<int, string>();

    void Start()
    {
        m_peer = new LoadBalancingPeer(this,ConnectionProtocol.Udp);
        m_peer.Connect("192.168.1.55:5055","MyServer");
    }

    void Update()
    {
        m_peer.Service();
    }

    void SendMessage(OperationRequest operationRequest)
    {
        m_peer.OpCustom(operationRequest,true,0,true);
    }

    void OnGUI()
    {
        m_userName = GUILayout.TextField("玩家名称");
        if (GUILayout.Button("进入房间"))
        {


            EnterRoomParams _param = new EnterRoomParams();
            _param.RoomName = "测试";
            Hashtable _actorProperties = new Hashtable();
            _actorProperties.Add((byte)'n',m_userName);
            _param.PlayerProperties = _actorProperties;
            m_peer.OpJoinLobby(TypedLobby.Default);
            m_peer.OpJoinRoom(_param);
        }
        if (GUILayout.Button("离开房间"))
        {
            m_peer.OpLeaveRoom(false);
        }

        GUILayout.Label(m_message);
    }


    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log(message);
    }

    public void OnEvent(EventData eventData)
    {
        Debug.Log("触发事件:" + eventData.ToStringFull());
        switch (eventData.Code)
        {
            case EventCode.Join:
                {
                    string _actorName = ((Hashtable)eventData.Parameters[ActorProperties.PlayerName])[(byte)'n'].ToString();
                    int _actorNum = (int)eventData.Parameters[ActorProperties.UserId];
                    if (!m_actorList.ContainsKey(_actorNum))
                        m_actorList.Add(_actorNum, _actorName);
                    else
                        m_actorList[_actorNum] = _actorName;
                    
                    m_message = "玩家" + _actorName + "进入了房间";
                }
                break;
            case EventCode.Leave:
                {
                    int _actorNum = (int)eventData.Parameters[ActorProperties.UserId];
                    if (m_actorList.ContainsKey(_actorNum))
                    {
                        string _userName = m_actorList[_actorNum];
                        m_message = "玩家" + _userName + "离开了房间";
                    }
                }
                break;
            default:
                break;
        }
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        switch (operationResponse.OperationCode)
        {
            case OperationCode.Join:
                {
                    int num = (int)operationResponse.Parameters[ParameterCode.ActorNr];
                    Debug.Log("进入游戏房间,玩家编号为:" + num);
                }
                break;
            case OperationCode.Leave:
                {
                    Debug.Log("离开房间");
                }
                break;
            default:
                {
                    ExcuteMessage(operationResponse); 
                }
                break;
        }
    }

    void ExcuteMessage(OperationResponse operationResponse)
    {

    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.Connect:
                Debug.Log("连接成功");
                break;
            case StatusCode.Disconnect:
                Debug.Log("关闭成功");
                break;
            case StatusCode.ExceptionOnConnect:
                Debug.Log("连接异常");
                break;
            default:
                break;
        }
    }
}
