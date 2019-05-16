using System;
using System.Diagnostics;
using System.Threading;
using ZeroMQ;

namespace FXComm
{
    public delegate void ReceivedEventHandler(string topic, string data);
    public delegate void LogReceivedEventHandler(EventLogEntryType logEntryType, string message);

    public class FXSubscriber
    {
        public event ReceivedEventHandler ReceivedEvent;
        public event LogReceivedEventHandler LogReceivedEvent;

        private readonly ZContext _zContext;
        private readonly ZSocket _zSocket;
        private string _address;
        private volatile bool _listenerStopped;
        private Thread _listenerThread;

        public FXSubscriber()
        {
            _zContext = new ZContext();
            _zSocket = new ZSocket(_zContext, ZSocketType.SUB);
        }

        public void Connect(string address)
        {
            _address = address;
            ZError error;
            _zSocket.Connect(address, out error);
        }

        public void Disconnect()
        {
            ZError error;
            _zSocket.Disconnect(_address, out error);
            _zSocket.Dispose();
            _zContext.Dispose();
        }

        public void Subscribe(string topic)
        {
            if (_listenerThread != null) return;

            _zSocket.Subscribe(topic);

            _listenerThread = new Thread(Listen) { };
            _listenerThread.Start();
        }

        public void Unsubscribe()
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
                            var topic = message[0].ReadString();
                            var data = message[1].ReadString();

                            OnReceived(topic, data);
                        }
                    }

                    // CPU için
                    Thread.Sleep(1);

                }
                catch (Exception e)
                {
                    OnLog(EventLogEntryType.Error, $"Subscriber listen exception. Error: {e.Message}");
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
