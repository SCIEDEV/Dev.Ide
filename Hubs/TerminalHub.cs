using System;
using Microsoft.AspNetCore.SignalR;

namespace Dev.Ide.Hubs
{
    public class TerminalHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendOutput(string connectionID, string output)
        {
            await Clients.Client(connectionID).SendAsync("Output", output);
        }

        public async Task Input(string connectionID, string output)
        {
            await Clients.Client(connectionID).SendAsync("Output", output);
        }
    }
}

