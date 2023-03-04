using System;
using System.Collections.Generic;

#nullable disable

namespace IDEA_Collection.Models
{
    public partial class Account
    {
        public Account()
        {
            Comments = new HashSet<Comment>();
            Ideas = new HashSet<Idea>();
            Likes = new HashSet<Like>();
            Unlikes = new HashSet<Unlike>();
        }

        public int AccountId { get; set; }
        public string Phone { get; set; }
        public DateTime? Birthday { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public bool Active { get; set; }
        public string FullName { get; set; }
        public int? RoleId { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Avatar { get; set; }

        public virtual Department Department { get; set; }
        public virtual Role Role { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Idea> Ideas { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<Unlike> Unlikes { get; set; }
    }
}
