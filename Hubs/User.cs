using System;
using System.Security.Claims;

namespace LotteryApp.Hubs
{
    public class User
    {
        public string Name;//User name
        public string Id;//Public id
        public string ConnectionId;//ConnectionId
        public string UID;//User Identifier
        public string GroupId;//Group id
        public ClaimsPrincipal Principal;
        public DateTime ExpirationTime;
        public bool Disconnected = false;
        public User(string uID, ClaimsPrincipal principal)
        {
            UID = uID;
            Principal = principal;
            this.Id = Guid.NewGuid().ToString();
        }
    }
}