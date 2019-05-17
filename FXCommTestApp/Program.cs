using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FXComm;

namespace FXCommTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //PubSub();
            PushPull("route" +
                     "");
        }

        private static void PushPull(string topic = null)
        {
            var pusher = new FXPusher();
            pusher.Connect("tcp://127.0.0.1:5595");

            var puller = new FXPuller();
            puller.Connect("tcp://127.0.0.1:5595");
            puller.ReceivedEvent += Puller_ReceivedEvent;
            puller.Start();

            pusher.Push("pushdata", topic);
            Console.WriteLine("Pushed");
            Console.ReadLine();

            pusher.Disconnect();
            puller.Stop();
            puller.Disconnect();

        }

        private static void Puller_ReceivedEvent(string topic, string data)
        {
            Console.WriteLine("Puller Received. {0}, {1}", topic, data); ;
        }

        private static void PubSub()
        {
            var publisher = new FXPublisher();
            publisher.Connect("tcp://127.0.0.1:5561");

            //var subscriber = new FXSubscriber();
            //subscriber.ReceivedEvent += SubscriberOnReceivedEvent;
            //subscriber.Connect("tcp://127.0.0.1:5561");
            ////subscriber.Subscribe("bms.triggerStation1.DischargeBin");
            //subscriber.Subscribe();

            Thread.Sleep(500);

            publisher.Publish("data");

            Console.WriteLine("Published");

            Console.ReadLine();

            publisher.Disconnect();
            //subscriber.Unsubscribe();
            //subscriber.Disconnect();
        }

        private static void SubscriberOnReceivedEvent(string topic, string data)
        {
            Console.WriteLine("Received. {0}, {1}", topic, data); ;
        }
    }
}
