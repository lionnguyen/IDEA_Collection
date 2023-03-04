using System;
using System.Collections.Generic;

#nullable disable

namespace IDEA_Collection.Models
{
    public partial class Comment
    {
        public int CommentId { get; set; }
        public string Contents { get; set; }
        public string Thumb { get; set; }
        public bool Published { get; set; }
        public string Alias { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? AccountId { get; set; }
        public int? Likes { get; set; }
        public int? Unlikes { get; set; }
        public bool Anonymously { get; set; }
        public int? PostId { get; set; }

        public virtual Account Account { get; set; }
        public virtual Idea Post { get; set; }
    }
}
