using System;

using System.Text;

namespace AlterCollation
{
    public class FullTextLanguage
    {

        private string name;
        private int lcid;

        public FullTextLanguage(string name, int lcid)
        {
            this.name = name;
            this.lcid = lcid;
        }

        public string Name
        {
            get { return name; }
        }

        public int Lcid
        {
            get { return lcid; }
        }

        public override string ToString()
        {
            return name;
        }


    }
}
