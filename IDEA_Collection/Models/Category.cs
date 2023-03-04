using System;
using System.Collections.Generic;

#nullable disable

namespace IDEA_Collection.Models
{
    public partial class Category
    {
        public Category()
        {
            Ideas = new HashSet<Idea>();
        }

        public int CatId { get; set; }
        public string CatName { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public string Thumb { get; set; }

        public virtual ICollection<Idea> Ideas { get; set; }
    }
}
