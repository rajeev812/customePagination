using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApp.Entity
{
    public class School
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SchoolId { get; set; }
        [Required(ErrorMessage = "Enter School Name")]
        [Display(Name = "School Name")]
        public string SchoolName { get; set; }

        [Required(ErrorMessage = "Enter Address")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Enter Phone Number")]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        //[Required(ErrorMessage = "Enter Start Time")]
        //[Display(Name = "Start Time")]
        //[DataType(DataType.Time)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm}")]
        //public DateTime StratTime { get; set; }
        //[Required(ErrorMessage = "Enter End Time")]
        //[Display(Name = "End Time")]
        //[DataType(DataType.Time)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:H:mm}")]
        //public DateTime Endtime { get; set; }
        public bool IsHavingBranch { get; set; }
        public int? BranchId { get; set; }
        public int ParentId { get; set; }

        public Dictionary<int,string> SchoolList { get; set; }
        public string SchRegNo { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
