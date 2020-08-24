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

        public async Task SetName(string name)
        {
            var user = ConnectedUsers.Users.FirstOrDefault(x => x.Id == Context.UserIdentifier);
            if (user != default)
            {
                Console.WriteLine("Set name to " + name);
                user.Name = name;
            }
        }

        public override Task OnConnectedAsync()
        {
            User user;
            if ((user = ConnectedUsers.Users.FirstOrDefault(x => x.Id == Context.UserIdentifier)) != default)//Already exists in memory
            {
                user.Disconnected = false;
                user.Principal = Context.User;
            }
            else
            {
                ConnectedUsers.Users.Add(new User(Context.UserIdentifier, Context.User));
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            User user = ConnectedUsers.Users.FirstOrDefault(x => x.Id == Context.UserIdentifier);
            user.Disconnected = true;
            user.ExpirationTime = DateTime.Now.AddMinutes(1);
            return base.OnDisconnectedAsync(exception);
        }
    }

    public interface ILotteryClient
    {
        Task ReceiveMessage(string user, string message);
        Task ReceiveMessage(string message);
        Task Roll(int number);
    }

    public static class ConnectedUsers
    {
        public static HashSet<User> Users = new HashSet<User>();
    }

    public class User
    {
        public string Name;
        public string Id;
        public ClaimsPrincipal Principal;
        public DateTime ExpirationTime;
        public bool Disconnected = false;
        public User(string id, ClaimsPrincipal principal)
        {
            Id = id;
            Principal = principal;
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
            throw new System.Exception();
        }
        return id;
    }
}