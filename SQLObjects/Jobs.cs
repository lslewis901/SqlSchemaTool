using System;

namespace Lewis.SST.SQLObjects
{
	/// <summary>
	/// Jobs Class - Not Yet Implemented.
	/// </summary>
	public class Jobs : BaseObject
	{
		/// <summary>
		/// Not yet implemented.
		/// </summary>
		public Jobs() : base () { }

        /// <summary>
        /// Jobs call
        /// </summary>
        /// <typeparam name="Jobs"></typeparam>
        /// <param name="_connection"></param>
        /// <param name="args"></param>
        public override void GetObject<Jobs>(System.Data.SqlClient.SqlConnection _connection, params object[] args)
        {
            throw new Exception("The method or operation is not implemented.");
        }
	}
}
