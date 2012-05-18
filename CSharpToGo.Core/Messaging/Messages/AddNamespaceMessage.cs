using TinyMessenger;

namespace CSharpToGo.Core.Messaging.Messages
{
    public class AddNamespaceMessage : GenericTinyMessage<string>
    {
        public AddNamespaceMessage(object sender, string content)
            : base(sender, content)
        {
        }
    }
}