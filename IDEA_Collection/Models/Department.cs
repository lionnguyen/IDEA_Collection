using System;
using System.Collections.Generic;

#nullable disable

namespace IDEA_Collection.Models
{
    public partial class Department
    {
        public Department()
        {
            Accounts = new HashSet<Account>();
        }

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public DateTime? StartDates { get; set; }
        public DateTime? ClosureDates { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
    }
}
