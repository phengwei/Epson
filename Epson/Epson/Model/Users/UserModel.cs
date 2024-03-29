﻿namespace Epson.Model.Users
{
    public class UserModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
        public int TeamId { get; set; }
        public string Teams { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }
}
