using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class AnswersFinished : Message
        {

            public bool value;


            public AnswersFinished(string json) : base(json) { }

            public AnswersFinished(string userId, bool value) :
                base(typeof(AnswersFinished).Name, userId)
            {

                this.value = value;
            }


            public override string FriendlyName()
            {
                return typeof(AnswersFinished).Name;
            }
        }
    }
}