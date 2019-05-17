using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace FXComm
{
    public class FXPuller
    {
        public event ReceivedEventHandler ReceivedEvent;
        public event LogReceivedEventHandler LogReceivedEvent;

        private readonly ZContext _zContext;
        private readonly ZSocket _zSocket;
        private string _address;
        private volatile bool _listenerStopped;
        private Thread _listenerThread;

        public FXPuller()
        {
            _zContext = new ZContext();
            _zSocket = new ZSocket(_zContext, ZSocketType.PULL);
        }

        public void Connect(string address)
        {
            _address = address;
            ZError error;
            _zSocket.Bind(address, out error);
        }

        public void Disconnect()
        {
            ZError error;
            _zSocket.Disconnect(_address, out error);
            _zSocket.Dispose();
            _zContext.Dispose();
        }

        public void Start()
        {
            if (_listenerThread != null) return;
            _listenerThread = new Thread(Listen) { };
            _listenerThread.Start();
        }

        public void Stop()
        {
            if (_listenerThread == null) return;

            _listenerStopped = true;
            _listenerThread.Join(1000);
        }

        private void Listen()
        {
            ZError error;
            while (!_listenerStopped)
            {
                try
                {
                    using (var message = _zSocket.ReceiveMessage(ZSocketFlags.DontWait, out error))
                    {
                        if (message != null)
                        {
                           
                            if (message.Count == 1)
                                OnReceived(null, message[0].ReadString());
                            else
                                OnReceived(message[0].ReadString(), message[1].ReadString());
                                
                        }
                    }

                    // CPU için
                    Thread.Sleep(1);

                }
                catch (Exception e)
                {
                    OnLog(EventLogEntryType.Error, $"Puller listen exception. Error: {e.Message}");
                }
            }
        }

        private void OnReceived(string topic, string data)
        {
            ReceivedEvent?.Invoke(topic, data);
        }

        private void OnLog(EventLogEntryType logEntryType, string message)
        {
            LogReceivedEvent?.Invoke(logEntryType, message);
        }
    }
}
