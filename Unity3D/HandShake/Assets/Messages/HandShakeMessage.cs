using System;

namespace OrkestraLib
{
    namespace Message
    {
        /// <summary>
        /// Message with the color information
        /// </summary>
        [Serializable]
        public class HandShakeMessage : Message
        {
            public string handShake;

            public HandShakeMessage(string json) : base(json) { }

            public HandShakeMessage(string sender, string handShake) :
              base(typeof(HandShakeMessage).Name, sender)
            {
                this.handShake = handShake;
            }

            public string GetHandShake()
            {
                return handShake;
            }

            public override string FriendlyName()
            {
                return typeof(HandShakeMessage).Name;
            }
        }
    }
}