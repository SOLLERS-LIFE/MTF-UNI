using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTF.Utilities
{
    public partial class pageParameters
    {
        public string uid { get; set; }
        public string CurrentRole { get; set; }
        public string CurrentSearchField { get; set; }
        public int CurrentPageSize { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public string WorkFlowId { get; set; }
        public string CurrentSort { get; set; }
        public string SortDirection { get; set; }
        public string CurrentFilter { get; set; }
        public pageParameters()
        {
            CurrentPageSize = GlobalParameters.DefaultPageSize;
        }
    }
}
