using System;

namespace OrkestraLib
{
    namespace Message
    {
        /// <summary>
        /// Message with the color information
        /// </summary>
        [Serializable]
        public class TryAgainMessage : Message
        {
            public bool retry;

            public TryAgainMessage(string json) : base(json) { }

            public TryAgainMessage(string sender,bool retry) :
              base(typeof(TryAgainMessage).Name, sender)
            {
                this.retry = retry;
            }

            public bool GetRetry()
            {
                return retry;
            }

            public override string FriendlyName()
            {
                return typeof(HandShakeMessage).Name;
            }
        }
    }
}