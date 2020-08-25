using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace LotteryApp.Hubs
{
    public class LotteryHub : Hub<ILotteryClient>
    {
        private DateTime lastRoll = DateTime.Now;

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message);
        }

        public async Task SendMessageToCaller(string message)
        {
            await Clients.Caller.ReceiveMessage(message);
        }

        public async Task JoinedRoom()
        {
            User user = ConnectedUsers.GetUser(Context.ConnectionId);
            if (user != default)
            {
                object[] users = this.UsersToTransport(ConnectedUsers.Users.Where(x => x.GroupId == user.GroupId).ToArray());
                var syncUsersTask = Clients.Caller.SyncUsers(users);//Sync caller users
                var notifyUsersTask = Clients.GroupExcept(user.GroupId, user.ConnectionId).UserJoined(this.UserToTransport(user));//Notify others that caller joined
                Task.WaitAll(new Task[] { syncUsersTask, notifyUsersTask });
            }
        }

        private async Task LeftRoom()
        {
            User user = ConnectedUsers.GetUser(Context.ConnectionId);
            if (user != default)
            {
                await Clients.GroupExcept(user.GroupId, user.ConnectionId).UserLeft(this.UserToTransport(user));
            }
        }

        public async Task SetName(string name)
        {
            await Task.Run(() =>
            {
                var user = ConnectedUsers.Users.FirstOrDefault(x => x.UID == Context.UserIdentifier);
                if (user != default)
                {
                    Console.WriteLine("Set name to " + name);
                    user.Name = name;
                }
            });
        }

        public async Task JoinRoom(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            var user = ConnectedUsers.Users.FirstOrDefault(x => x.UID == Context.UserIdentifier);
            if (user != default)
            {
                user.GroupId = groupId;
            }
            await Clients.Caller.GoToRoom(groupId);
        }

        public async Task CreateNewRoom()
        {
            string groupId;
            do
            {
                groupId = new string(Guid.NewGuid().ToString().Take(5).ToArray());
            } while (ConnectedUsers.Users.Select(x => x.GroupId).Contains(groupId));
            ConnectedUsers.Users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId).GroupId = groupId;
            await Clients.Caller.GoToRoom(groupId);
        }

        public override async Task OnConnectedAsync()
        {
            User user;
            if ((user = ConnectedUsers.Users.FirstOrDefault(x => x.UID == Context.UserIdentifier)) != default)//Already exists in memory
            {
                user.ConnectionId = Context.ConnectionId;
                user.Disconnected = false;
                user.Principal = Context.User;
                if (user.GroupId != default)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, user.GroupId);
                }
            }
            else
            {
                user = new User(Context.UserIdentifier, Context.User);
                user.ConnectionId = Context.ConnectionId;
                ConnectedUsers.Users.Add(user);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            User user = ConnectedUsers.Users.FirstOrDefault(x => x.UID == Context.UserIdentifier);
            user.Disconnected = true;
            user.ExpirationTime = DateTime.Now.AddMinutes(1);
            await this.LeftRoom();
            await base.OnDisconnectedAsync(exception);
        }

        private object UserToTransport(User user)
        {
            return new { user.Name, user.Id };
        }
        private object[] UsersToTransport(User[] users)
        {
            object[] transportUsers = new object[users.Length];
            int i = 0;
            foreach (User user in users)
            {
                transportUsers[i] = new { user.Id, user.Name };
                i++;
            }
            return transportUsers;
        }
    }

    public interface ILotteryClient
    {
        Task ReceiveMessage(string user, string message);
        Task ReceiveMessage(string message);
        Task Roll(int number);
        Task GoToRoom(string roomId);
        Task SyncUsers(object[] users);
        Task UserLeft(object user);
        Task UserJoined(object user);
    }

    public static class ConnectedUsers
    {
        public static HashSet<User> Users = new HashSet<User>();

        public static User GetUser(string connectionId)
        {
            return Users.FirstOrDefault(x => x.ConnectionId == connectionId);
        }
    }

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

public class UserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        HttpContext httpContext = connection.GetHttpContext();
        string id;
        if (httpContext.Request.Cookies["UserID"] != default)
        {
            id = httpContext.Request.Cookies["UserID"];
        }
        else
        {
            return Guid.NewGuid().ToString();//Force random guid if maintain cookie does not exist
        }
        return id;
    }
}