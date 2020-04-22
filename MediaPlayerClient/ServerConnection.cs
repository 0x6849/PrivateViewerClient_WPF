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
            await socket.ConnectAsync(serverUri, cts);

            var ctss = new CancellationTokenSource();

            Task.Factory.StartNew(
                async () =>
                {
                    var rcvBytes = new byte[128];
                    var rcvBuffer = new ArraySegment<byte>(rcvBytes);
                    while (true)
                    {
                        try
                        {
                            WebSocketReceiveResult rcvResult = await socket.ReceiveAsync(rcvBuffer, ctss.Token);
                            byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                            string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                            Application.Current.Dispatcher.Invoke(new System.Action(() => { HandleMessage(rcvMsg); }));
                            
                        } catch (Exception e)
                        {
                        Debug.Print(e.ToString());
                        }
                    }
                    Debug.Print("how did this end????");
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
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, new CancellationToken());
        }

        public static async Task<string> ReadString(ClientWebSocket ws)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new Byte[8192]);

            WebSocketReceiveResult result = null;

            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
        }

    }

    public enum Action
    {
        join, leave, createRoom, listRooms, removeRoom, change, getUpdate
    }

}
