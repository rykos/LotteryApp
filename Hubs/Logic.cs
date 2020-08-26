using System;
using System.Text;
using System.Threading;
using LotteryApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LotteryApp.Hubs
{
    public class Lottery
    {
        private readonly IHubContext<LotteryHub, ILotteryClient> lotteryHub;
        private Timer timer, purgeUsersTimer;
        public Lottery(IHubContext<LotteryHub, ILotteryClient> lotteryHub)
        {
            this.lotteryHub = lotteryHub;
            this.QWE();
            this.PurgeUsers();
        }

        public async void QWE()
        {
            Random rnd = new Random();
            this.timer = new Timer(async (e) =>
            {
                await this.lotteryHub.Clients.All.Roll(rnd.Next(0, 100));
                foreach (var user in ConnectedUsers.Users)
                {
                    await lotteryHub.Clients.Client(user.ConnectionId).ReceiveMessage($"Id:{user.ConnectionId} Name:{user.Name} GID:{user.GroupId} UID:{user.UID}");
                }
            }, null, 0, 1500);
        }

        public void PurgeUsers()
        {
            this.purgeUsersTimer = new Timer((e) =>
            {
                User[] users = ConnectedUsers.Users.Where(x => x.Disconnected && x.ExpirationTime < DateTime.Now).ToArray();
                foreach(User user in users)
                {
                    ActiveGroups.RemoveUserFromGroup(user.GroupId, user);
                }
                ConnectedUsers.Users.Where(x => x.Disconnected && x.ExpirationTime < DateTime.Now).ToArray();
            }, null, 0, 60000);
        }
    }
}