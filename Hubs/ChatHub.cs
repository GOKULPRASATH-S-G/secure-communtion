// Hubs/ChatHub.cs (Corrected for 2-User Chat)
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SecureCommECC.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var publicKey = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (publicKey != null)
            {
                UserConnections.TryRemove(publicKey, out _);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public Task Register(string publicKey)
        {
            UserConnections[publicKey] = Context.ConnectionId;
            return Task.CompletedTask;
        }

        public async Task SendPrivateMessage(string recipientPublicKey, string encryptedMessage)
        {
            var senderPublicKey = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (senderPublicKey == null) return;

            if (UserConnections.TryGetValue(recipientPublicKey, out var recipientConnectionId))
            {
                await Clients.Client(recipientConnectionId).SendAsync("ReceiveMessage", senderPublicKey, encryptedMessage);
            }
        }
    }
}