using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Poseidon;

public class SocketDictionary
{
    private static SocketDictionary _socketDictionary = null;
    private ConcurrentDictionary<User, WebSocket> socketList;

    private SocketDictionary()
    {
        socketList = new ConcurrentDictionary<User, WebSocket>();
    }

    public static SocketDictionary GetSocketDictionary()
    {
        if (_socketDictionary == null)
        {
            _socketDictionary = new SocketDictionary();
        }

        return _socketDictionary;
    }
    
    public ConcurrentDictionary<User, WebSocket> GetSocketList()
    {
        return socketList;
    }
    
    public WebSocket GetTargetSocket(string uid)
    {
        ConcurrentDictionary<User, WebSocket> webSockets = GetSocketList();
        User findUser = webSockets.Keys.FirstOrDefault(thisUser => thisUser.uid == uid);
        if(findUser == null) return null;
        webSockets.TryGetValue(findUser, out WebSocket targetSocket);
        return targetSocket;
    }

    public ConcurrentDictionary<User, WebSocket> SetMySocket(User user, WebSocket webSocket)
    {
        socketList.TryAdd(user, webSocket);
        return socketList;
    }
    
    public void RemoveMySocket(User user)
    {
        socketList.TryRemove(user, out _);
    }
}