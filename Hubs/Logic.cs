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
            Console.WriteLine("Ran QWE");
            Random rnd = new Random();
            this.timer = new Timer(async (e) =>
            {
                await this.lotteryHub.Clients.All.Roll(rnd.Next(0, 100));
                StringBuilder sb = new StringBuilder();
                foreach (var user in ConnectedUsers.Users)
                {
                    sb.Append(user.Name);
                    sb.Append(',');
                }
                await this.lotteryHub.Clients.All.ReceiveMessage(sb.ToString());
            }, null, 0, 1500);
        }

        public void PurgeUsers()
        {
            this.purgeUsersTimer = new Timer((e) =>
            {
                ConnectedUsers.Users.RemoveWhere(x => x.Disconnected && x.ExpirationTime < DateTime.Now);
            }, null, 0, 60000);
        }
    }
}