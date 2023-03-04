using System;
using System.Collections.Generic;

#nullable disable

namespace IDEA_Collection.Models
{
    public partial class Like
    {
        public int LikeId { get; set; }
        public int? PostId { get; set; }
        public int? AccountId { get; set; }

        public virtual Account Account { get; set; }
        public virtual Idea Post { get; set; }
    }
}
