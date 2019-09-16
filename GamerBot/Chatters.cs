using System.Collections.Generic;

namespace GamerBot
{
    /// <summary>
    /// Chatters is just a class to group together the list of users in the chat who are chatting,
    /// a nice friendly structure to hold this information, we probably don't need it. 
    /// </summary>
    public class Chatters
    {
        private readonly List<string> _users;

        public Chatters()
        {
            _users = new List<string>();
        }

        public void Register(string user)
        {
            if (!_users.Contains(user))
            {
                _users.Add(user);
            }
        }
    }
}