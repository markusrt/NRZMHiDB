using System;

namespace HaemophilusWeb.Utils
{
    public static class ExceptionUtils
    {
        public static bool AnyMessageMentions(this Exception e, string messageContent)
        {
            return e != null &&
                   (e.Message.Contains(messageContent) || e.InnerException.AnyMessageMentions(messageContent));
        }
    }
}