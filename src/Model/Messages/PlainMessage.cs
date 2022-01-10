using System;

namespace Symnity.Model.Messages
{
    [Serializable]
    public class PlainMessage : Message
    {
        /**
         * Create plain message object.
         * @returns PlainMessage
         */
        public static PlainMessage Create(string  message) {
            return new PlainMessage(message);
        }
        
        /**
         * @internal
         * @param payload
         */
        public PlainMessage(string payload) 
        : base(MessageType.PlainMessage, payload)
        {
        }
    }
}