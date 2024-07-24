using System;
using System.Collections.Generic;
using System.Text;

namespace aoaifunctions.ResponseEntities
{
    public class Values
    {
        public Values()
        {
            errors = new List<string>();
            warnings = new List<string>();
        }

        public string recordId { get; set; }
        public Data data { get; set; }
        public List<string> errors { get; set; }
        public List<string> warnings { get; set; }
    }
}
