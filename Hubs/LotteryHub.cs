using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading;
using System.Threading.Tasks;

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
    }

    public interface ILotteryClient
    {
        Task ReceiveMessage(string user, string message);
        Task ReceiveMessage(string message);
        Task Roll(int number);
    }
}