using TinyMessenger;

namespace CSharpToGo.Core.Messaging.Messages
{
    public class ResultInputSelectedMessage : GenericTinyMessage<string>
    {
        public ResultInputSelectedMessage(object sender, string content)
            : base(sender, content)
        {
        }
    }
}