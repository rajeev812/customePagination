using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApp.Entity
{
  public  class Student
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }
        [Required(ErrorMessage = "Enter Student Name")]
        [Display(Name ="Student Name")]
        public string StudentName { get; set; }

        public string StudentRegNumber { get; set; }

        [Required(ErrorMessage = "Enter Class")]
        [Display(Name = "Add Class")]
        public string AddCls { get; set; }

        [Required(ErrorMessage = "Enter Father Name")]
        [Display(Name = "Father Name")]
        public string FatherName { get; set; }

        [Required(ErrorMessage = "Enter Mother Name")]
        [Display(Name = "Mother Name")]
        public string MotherName { get; set; }


        [Required(ErrorMessage = "Enter Father Mobile Number")]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "Entered Mobile No is not valid.")]
        [Display(Name = "Father Mobile")]
        public string FatherMobileNo { get; set; }

        [Required(ErrorMessage = "Enter Mother Mobile Number")]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "Entered Mobile No is not valid.")]
        [Display(Name = "Mother Mobile")]
        public string MotherMobileNo { get; set; }

        //[Required(ErrorMessage = "Enter Email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string  EmailId { get; set; }

        public Dictionary<int,string> schoolName { get; set; }

        public int SchooldId { get; set; }
        public string SchoolsName { get; set; }

        public Dictionary<int, string> SchoolChildList { get; set; }
        public bool IsBranch { get; set; }
        public bool IsBranchExl { get; set; }
        public int SchooldChildId { get; set; }
        public string SchoolsChildName { get; set; }

        //public ICollection<School> Schools { get; set; }

    }
}
