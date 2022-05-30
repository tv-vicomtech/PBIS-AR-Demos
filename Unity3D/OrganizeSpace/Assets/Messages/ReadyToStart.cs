using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class ReadyToStart : Message
        {

            public bool value;


            public ReadyToStart(string json) : base(json) { }

            public ReadyToStart(string userId, bool value) :
                base(typeof(ReadyToStart).Name, userId)
            {

                this.value = value;
            }


            public override string FriendlyName()
            {
                return typeof(ReadyToStart).Name;
            }
        }
    }
}