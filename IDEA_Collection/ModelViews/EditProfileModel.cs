using System;
using System.ComponentModel.DataAnnotations;

namespace IDEA_Collection.ModelViews
{
    public class EditProfileModel
    {
        [Key]
        public int CustomerId { get; set; }

        [Display(Name = "FullName")]
        public string FullName { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }
        [Display(Name = "Avata")]
        public string Avata { get; set; }
        [Display(Name = "Birthdate")]
        public DateTime Birthdate { get; set; }
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        [Display(Name = "DepartmentID")]
        public int DepartmentID { get; set; }

    }
}
