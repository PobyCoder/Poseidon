using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Poseidon;

public class MatchDictionary
{
    private static MatchDictionary _matchDictionary = null;
    private static ConcurrentDictionary<string, ConcurrentDictionary<User, WebSocket>> matchList;

    private MatchDictionary()
    {
        matchList = new ConcurrentDictionary<string, ConcurrentDictionary<User, WebSocket>>();
    }

    public static MatchDictionary GetMatchDictionary()
    {
        if (_matchDictionary == null)
        {
            _matchDictionary = new MatchDictionary();
        }

        return _matchDictionary;
    }

    public ConcurrentDictionary<User, WebSocket> GetMatch(string matchId)
    {
        return matchList.TryGetValue(matchId, out var match) ? match : null;
    }
    
    public void SetMatch(string matchId, ConcurrentDictionary<User, WebSocket> match)
    {
        matchList.TryAdd(matchId, match);
    }

    public void RemoveMatch(string matchId)
    {
        matchList.TryRemove(matchId, out _);
    }
}