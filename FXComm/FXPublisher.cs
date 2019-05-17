using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace FXComm
{
    public class FXPublisher
    {
        private readonly ZContext _zContext;
        private readonly ZSocket _zSocket;
        private string _address;

        public FXPublisher()
        {
            _zContext = new ZContext();
            _zSocket = new ZSocket(_zContext, ZSocketType.PUB)
            {
                // todo add: Timeoutlar dışarıdan verileccek
                SendTimeout = TimeSpan.FromSeconds(10),
                ReceiveTimeout = TimeSpan.FromSeconds(10),
                RequestRelaxed = true,
                Immediate = true,
            };
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

        public void Publish(string data, string topic = null)
        {
            ZError error;
            using (var packet = new ZMessage())
            {
                if (topic != null)
                    packet.Add(new ZFrame(topic));

                packet.Add(new ZFrame(data));
                if (!_zSocket.Send(packet, out error))
                {
                    throw new ZException(error);
                }
            }
        }

    }
}
