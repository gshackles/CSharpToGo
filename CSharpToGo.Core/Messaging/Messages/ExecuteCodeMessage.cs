using TinyMessenger;

namespace CSharpToGo.Core.Messaging.Messages
{
    public class ExecuteCodeMessage : GenericTinyMessage<string>
    {
        public ExecuteCodeMessage(object sender, string content)
            : base(sender, content)
        {

        }
    }
}