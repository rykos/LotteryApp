using System;
using System.Threading;
using LotteryApp.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LotteryApp.Hubs
{
    public class Lottery
    {
        private readonly IHubContext<LotteryHub, ILotteryClient> lotteryHub;
        private Timer timer;
        public Lottery(IHubContext<LotteryHub, ILotteryClient> lotteryHub)
        {
            this.lotteryHub = lotteryHub;
            QWE();
        }

        public async void QWE()
        {
            Console.WriteLine("Ran QWE");
            Random rnd = new Random();
            this.timer = new Timer(async (e) =>
            {
                await this.lotteryHub.Clients.All.Roll(rnd.Next(0, 100));
            }, null, 0, 3000);
        }
    }
}