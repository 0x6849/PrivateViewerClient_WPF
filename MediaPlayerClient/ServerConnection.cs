using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Windows;
using System.Threading;

namespace MediaPlayerClient
{
    public class ServerConnection
    {
        public readonly string Room;
        private ClientWebSocket socket;
        private Uri serverUri;

        public event Action<MediaResult> MessageReceivedEvent;

        public ServerConnection(string serverURL, string room)
        {
            Room = room;
            serverUri = new Uri("wss://" + serverURL);
        }

        public async Task Connect()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            var cts = new CancellationToken();
            socket = new ClientWebSocket();
            try
            {
                await socket.ConnectAsync(serverUri, cts);
            }
            catch
            {
                Debug.Print("error on connection establish");
                return;
            }
            var ctss = new CancellationTokenSource();

            Task.Factory.StartNew(
                async () =>
                {
                    var rcvBytes = new byte[128];
                    var rcvBuffer = new ArraySegment<byte>(rcvBytes);
                    while (true)
                    {
                        WebSocketReceiveResult rcvResult = null;
                        try
                        {
                            rcvResult = await socket.ReceiveAsync(rcvBuffer, ctss.Token);
                        }
                        catch
                        {
                            Debug.Print("error on communication");
                            socket = null;
                            break;
                        }
                        try
                        {
                            byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                            string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                            Application.Current.Dispatcher.Invoke(new System.Action(() => { HandleMessage(rcvMsg); }));
                            
                        } catch (Exception e)
                        {
                        Debug.Print(e.ToString());
                        }
                    }
                }, ctss.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            await SendRequest(MediaCommand.Join(Room));
        }

        private void HandleMessage(string msg)
        {
            MediaResult result = MediaResult.FromJson(msg);
            if (result.isOk())
            {
                MessageReceivedEvent?.Invoke(result);
            } else
            {
                Debug.Print("error: " + result.ToString());
            }
            Debug.Print(result.ToString());
        }

        public async Task SendRequest(MediaCommand cmd)
        {
            await SendString(cmd.ToJson());
        }

        public async Task SendString(string data)
        {
            var encoded = Encoding.UTF8.GetBytes(data);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
            await socket?.SendAsync(buffer, WebSocketMessageType.Text, true, new CancellationToken());
        }

    }

    public enum Action
    {
        join, leave, createRoom, listRooms, removeRoom, change, getUpdate
    }

}
