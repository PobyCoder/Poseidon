using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace Poseidon;

public class SystemMessage
{
    public void Send(User user, string message)
    {
        SocketDictionary socketDictionary = SocketDictionary.GetSocketDictionary();
        ConcurrentDictionary<User, WebSocket> webSockets = socketDictionary.GetSocketList();
        ResponseSystemMessageSendType responseSystemMessageSend = new ResponseSystemMessageSendType
        {
            type = Enum.GetName(typeof(MessageSendType), MessageSendType.SystemMessage),
            username = "System",
            message = message
        };
        
        string responseSystemMessageSendJson = JsonConvert.SerializeObject(responseSystemMessageSend);
        byte[] encodedMessage = Encoding.UTF8.GetBytes(responseSystemMessageSendJson);

        webSockets.TryGetValue(user, out WebSocket mySocket);
        mySocket.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}