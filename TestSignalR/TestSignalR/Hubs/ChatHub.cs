using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestSignalR.Map;

namespace TestSignalR.Hubs
{
    public class ChatHub: Hub
    {

        public async Task JoinHub(string who)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, who);
        }

        public async Task LeaveHub(string who)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, who);
        }
    }
}
