using TinyMessenger;

namespace CSharpToGo.Core.Messaging.Messages
{
    public class MessageLongClickedMessage : GenericTinyMessage<string>
    {
        public MessageLongClickedMessage(object sender, string content)
            : base(sender, content)
        {
        }
    }
}