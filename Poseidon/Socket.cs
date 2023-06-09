using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Poseidon;

public class Socket
{
    WebSocketReceiveResult result;
    
    public async Task Start(WebSocket webSocket, HttpContext context)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        var query = context.Request.Query;
        var token = query["token"];
        JwtTokenSystem jwtTokenSystem = new JwtTokenSystem();
        User user = jwtTokenSystem.ValidateJwtToken(token);
        if (user != null)
        {
            Program.logger.Info($"{user.usn}({user.uid})님이 서버 연결");
            Init(user);
            socketDictionary.SetMySocket(user, webSocket);
            var buffer = new byte[1024 * 4];
            CancellationTokenSource cts = new CancellationTokenSource();
            while (true)
            {
                StringBuilder message = new StringBuilder();
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), cts.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Program.logger.Info($"{user.usn}님이 서버 연결 해제");
                    Init(user);
                    break;
                }
                
                message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                Program.Router.Routing(user, message, cts);
            }
        }
        else
        {
            Program.logger.Error("Websocket Token is invalid");
            context.Response.StatusCode = 400;
        }
    }
    
    public void Init(User user)
    {
        string uid = user.uid;
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        CurrentGroupDictionary currentGroupDictionary = CurrentGroupDictionary.GetCurrentGroupDictionary();
        MessageHistoryDictionary messageHistoryDictionary = MessageHistoryDictionary.GetMessageHistoryDictionary();
        MessageBanDictionary messageBanDictionary = MessageBanDictionary.GetMessageBanDictionary();
        CurrentMatchDictionary currentMatchDictionary = CurrentMatchDictionary.GetCurrentMatchDictionary();
        
        // 소켓 초기화
        socketDictionary.RemoveMySocket(user);
        
        // 그룹 관련 초기화
        string myGroupKey = currentGroupDictionary.GetMyGroup(uid);
        if (myGroupKey != null)
        {
            GroupLeave groupLeave = new GroupLeave();
            groupLeave.Leave(user, new StringBuilder(), new CancellationTokenSource(), myGroupKey);
        }
        
        // 메세지 제한 초기화
        messageHistoryDictionary.RemoveMyMessageHistory(uid);
        messageBanDictionary.RemoveMessageBan(uid);
        
        // 매치 관련 초기화
        string matchId = currentMatchDictionary.GetMyMatchId(uid);
        if (matchId != null)
        {
            MatchLeave matchLeave = new MatchLeave();
            matchLeave.Leave(user, new StringBuilder(), new CancellationTokenSource());
        }
    }
}
