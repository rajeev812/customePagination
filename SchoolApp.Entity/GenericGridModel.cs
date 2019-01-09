using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApp.Entity
{
    public class GenericGridModel<T> where T : class, new()
    {
        public int TotalCount { get; set; }
        public int currentPage { get; set; }
        //public bool CanAdd { get; set; }
        //public bool CanEdit { get; set; }
        //public bool CanDelete { get; set; }
        //public bool CanViewSingle { get; set; }
        //public bool CanViewMultiple { get; set; }
        //public bool IsViewOnly { get; set; }
        public T model { get; set; }
        public List<T> ItemDetails { get; set; }

        public GenericGridModel()
        {
            this.ItemDetails = new List<T>();
        }
    }
}
