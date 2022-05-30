using System;

namespace OrkestraLib
{
    namespace Message
    {
        /// <summary>
        /// Message with the color information
        /// </summary>
        [Serializable]
        public class ActMessage : Message
        {
            public ActMessages response;

            public ActMessage(string json) : base(json) { }

            public ActMessage(string sender, ActMessages response) :
              base(typeof(ActMessage).Name, sender)
            {
                this.response = response;
            }

            public ActMessages GetActs()
            {
                return response;
            }

            public override string FriendlyName()
            {
                return typeof(ActMessage).Name;
            }
        }
    }
}