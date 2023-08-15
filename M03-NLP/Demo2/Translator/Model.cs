using System;
using System.Collections.Generic;
using System.Text;

namespace text_analysis
{
    //[{"translations":[{"text":"Ciao, come ti chiami?","to":"it"}]}]

    public class Results
    {
        public List<Translation> translations { get; set; }
    }

    public class Translation
    {
        public string text { get; set; }
        public string to { get; set; }
    }

}
