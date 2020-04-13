using Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace CoffeeRoasterDesktopBackground
{
    public static class MessageHandler
    {
        public static IObservable<IMessage> MessageRecieved => messageRecievedSubject;

        private static readonly Subject<IMessage> messageRecievedSubject = new Subject<IMessage>();

        public static void HandleIncomingMessage(string message)
        {
            var incomingMessage = message.ToString().Split("_");

            if (incomingMessage.Count() == 0)
                return;

            int.TryParse(incomingMessage[0], out int messageType);

            switch (messageType)
            {
                case 1:
                    HandleTemperatureMessage(incomingMessage);
                    break;
                case 2:
                    HandleSystemMessage(incomingMessage);
                    break;
                case 3:
                    HandleErrorMessage(incomingMessage);
                    break;
                case 0:
                default:
                    break;
            }
        }

        private static void HandleErrorMessage(string[] messageData)
        {

        }

        private static void HandleSystemMessage(string[] messageData)
        {

        }

        private static void HandleTemperatureMessage(string[] messageData)
        {
            int.TryParse(messageData[2], out int temperature);
            int.TryParse(messageData[3], out int timeInSeconds);

            messageRecievedSubject.OnNext(new TemperatureMessage()
            {
                Temperature = temperature,
                TimeInSeconds = timeInSeconds
            });
        }
    }
}
