using System;

namespace Symnity.Model.Messages
{
    /**
     * An abstract message class that serves as the base class of all message types.
     */
    [Serializable]
    public class Message
    {
        /**
         * Message type
         */
        public readonly MessageType Type;

        /**
         * Message payload, it could be the message hex, encryped text or plain text depending on the message type.
         */
        public readonly string Payload;

        /**
         * @internal
         * @param type
         * @param payload
         */
        public Message(MessageType type, string payload)
        {
            Type = type;
            Payload = payload;
        }
    }
}