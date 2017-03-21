using System;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.IO;

namespace AlterCollation
{
    public class ScriptStepResource : ScriptStep
    {
        public ScriptStepResource(string resourceIdentifier)
            : base(LoadResource(resourceIdentifier, null))
        {
        }

        public ScriptStepResource(string resourceIdentifier, object[] args)
            : base(LoadResource(resourceIdentifier, args))
        {
        }
        
        /// <summary>
        /// load the sql from a SQL Resource file
        /// </summary>
        /// <param name="resourceIdentifier"></param>
        /// <returns></returns>
        private static string LoadResource(string resourceIdentifier, object[] args)
        {
            string retVal;
            Assembly thisAssembly;
            Stream stream;
            StreamReader reader;
            thisAssembly = Assembly.GetExecutingAssembly();
            stream = thisAssembly.GetManifestResourceStream(resourceIdentifier);
            reader = new StreamReader(stream);

            retVal = reader.ReadToEnd();

            reader.Close();
            if (args != null)
                return string.Format(CultureInfo.InvariantCulture, retVal, args);
            else
                return retVal;
        }
    }
  



}
