using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class YourTurn : Message
        {
            public bool value;

            public YourTurn(string json) : base(json) { }

            public YourTurn(string userId, bool value) :
                base(typeof(YourTurn).Name, userId)
            {
                this.value = value;
            }


            public override string FriendlyName()
            {
                return typeof(YourTurn).Name;
            }
        }
    }
}