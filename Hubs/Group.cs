using System.Collections.Generic;

namespace LotteryApp.Hubs
{
    public class Group
    {
        public string Id;//Public id
        public User Creator;//User that created this room
        public HashSet<User> Users = new HashSet<User>();//Connected users

        public Group(string groupId, User creator)
        {
            this.Id = groupId;
            this.Creator = creator;
        }
    }
}