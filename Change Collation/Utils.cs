using System;
using System.Text;

namespace AlterCollation
{
    class Utils
    {
        private Utils()
        {
        }

        /// <summary>
        /// build up a connection string for given some connection parameters
        /// </summary>
        /// <param name="server"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string ConnectionString(string server, string userId, string password)
        {

            if (userId == null || userId.Length == 0)
                return string.Format("Data Source={0};Trusted_Connection=SSPI;Initial Catalog=master;Application Name=CSL Change Collation;Connect Timeout=5;Pooling=false;", server);
            else

                return string.Format("Application Name={1};Connect Timeout=8;Persist Security Info=False;Database=master;User ID={2};Password={3};Data Source={0};Connect Timeout=5;Pooling=false;", server, "Collation Changer", userId, password);

        }
    }
}
