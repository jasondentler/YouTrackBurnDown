using System;

namespace BetterBurnDown.YouTrack
{
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException(string message) : base(message)
        {
            
        }
    }
}