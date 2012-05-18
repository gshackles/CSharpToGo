using TinyMessenger;

namespace CSharpToGo.Core.Messaging
{
    public class MessageHub
    {
        private static readonly TinyMessengerHub _instance = new TinyMessengerHub();

        static MessageHub() { }
        private MessageHub() { }

        public static TinyMessengerHub Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}