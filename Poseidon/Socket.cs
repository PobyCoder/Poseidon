using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Poseidon;

public class Socket
{
    static readonly ConcurrentDictionary<User, WebSocket> webSockets = new ConcurrentDictionary<User, WebSocket>();
    private static readonly Router Router = new Router();
    WebSocketReceiveResult result;
    
    public async Task Start(WebSocket webSocket, HttpContext context)
    {
        var query = context.Request.Query;
        var token = query["token"];
        JwtTokenSystem jwtTokenSystem = new JwtTokenSystem();
        User user = jwtTokenSystem.ValidateJwtToken(token);
        if (user != null)
        {
            Program.logger.Info($"{user.usn}({user.uid})님이 서버 연결");
            Init(webSockets, user);
            webSockets.TryAdd(user, webSocket);
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
                    Init(webSockets, user);
                    break;
                }
                message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                Router.Routing(webSockets, user, message, cts);
            }
        }
        else
        {
            Program.logger.Error("Token is invalid");
            context.Response.StatusCode = 400;
        }
    }
    
    public void Init(ConcurrentDictionary<User,WebSocket> webSockets, User user)
    {
        string uid = user.uid;
        
        // 소켓 초기화
        webSockets.TryRemove(user, out _);
        
        // 그룹 관련 초기화
        Program.currenGroup.TryGetValue(uid, out string myGroupKey);
        if (myGroupKey != null)
        {
            GroupLeave groupLeave = new GroupLeave();
            groupLeave.Leave(webSockets, user, new StringBuilder(), new CancellationTokenSource(), myGroupKey);
        }
    }
}