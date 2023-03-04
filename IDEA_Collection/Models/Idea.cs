using System;
using System.Collections.Generic;

#nullable disable

namespace IDEA_Collection.Models
{
    public partial class Idea
    {
        public Idea()
        {
            Comments = new HashSet<Comment>();
            LikesNavigation = new HashSet<Like>();
            UnlikesNavigation = new HashSet<Unlike>();
        }

        public int PostId { get; set; }
        public string Title { get; set; }
        public string Scontents { get; set; }
        public string Contents { get; set; }
        public string Thumb { get; set; }
        public bool Published { get; set; }
        public bool Anonymously { get; set; }
        public string Alias { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Author { get; set; }
        public int? AccountId { get; set; }
        public int? CatId { get; set; }
        public int? Likes { get; set; }
        public int? Unlikes { get; set; }

        public virtual Account Account { get; set; }
        public virtual Category Cat { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> LikesNavigation { get; set; }
        public virtual ICollection<Unlike> UnlikesNavigation { get; set; }
    }
}
