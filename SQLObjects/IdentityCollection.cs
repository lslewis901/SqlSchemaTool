using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Infor.SST.SQLObjects
{
	/// <summary>
	/// Summary description for IdentityCollection.
	/// </summary>
	public class IdentityCollection : CollectionBase
	{
		private string _tableName;

		public IdentityCollection()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public IdentityCollection(string tableName)
		{
			_tableName = tableName;
		}

		public string TableName
		{
			get
			{
				return _tableName;
			}
			set
			{
				_tableName = value;
			}
		}

		public void Add(IDENTITY_COLUMN Identity)
		{
			List.Add( Identity );
		}
		
		public IDENTITY_COLUMN this[int Index]
		{
			get{return (IDENTITY_COLUMN)List[Index];}
		}

		public bool Contains( IDENTITY_COLUMN value )  
		{
			// If value is not of type Column, this will return false.
			return( List.Contains( value ) );
		}

		public XmlNode SerializeAsXmlNode(XmlDocument doc)
		{
			System.IO.MemoryStream ms = new MemoryStream();
			XmlSerializer serializer = new XmlSerializer(typeof(ColumnCollection));
			XmlTextReader xRead = null;
			XmlNode xTable = null;
			try
			{
				xTable = doc.CreateNode(XmlNodeType.Element, "TABLE", doc.NamespaceURI);

				serializer.Serialize(ms, this);
				ms.Position = 0;
				xRead = new XmlTextReader( ms );
				xRead.MoveToContent();
				string test = xRead.ReadInnerXml();
				XmlDocumentFragment docFrag = doc.CreateDocumentFragment();
				docFrag.InnerXml = test;

				xTable.AppendChild(docFrag);
			}
			catch(Exception ex)
			{
				throw new Exception("IdentityCollection Serialization Error.", ex);
			}
			finally
			{
				ms.Close();
				if (xRead != null) xRead.Close();
			}
			return xTable;
		}
	}

	[Serializable()]
	public class IDENTITY_COLUMN
	{
		private string _table_Name;
		private string _identity;
		private int _seed;
		private int _increment;
		private int _not_for_replication;

		public IDENTITY_COLUMN()
		{
		}

		public IDENTITY_COLUMN(string Identity)
		{
			_identity = Identity;
		}

		public IDENTITY_COLUMN Convert(XmlNode xn)
		{
			try
			{
				foreach(XmlNode x in xn.ChildNodes)
				{
					switch(x.Name.ToLower())
					{
						case "table_name":
						{
							this.TABLE_NAME = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "identity":
						{
							this.Identity = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "seed":
						{
							this.Seed = System.Convert.ToInt32(x.InnerXml);
							break;
						}
						case "increment":
						{
							this.Increment = System.Convert.ToInt32(x.InnerXml);
							break;
						}
						case "not_x0020_for_x0020_replication":
						{
							this.Not_x0020_For_x0020_Replication = System.Convert.ToInt32(x.InnerXml);
							break;
						}
					}
				}
			}
			catch(Exception ex)
			{
				throw new Exception("Identity_Column Conversion Error.", ex);
			}
			return this;
		}

		public string TABLE_NAME
		{
			get{return _table_Name;}
			set{_table_Name = value;}
		}

		public string Identity
		{
			get{return _identity;}
			set{_identity = value;}
		}

		public int Seed
		{
			get{return _seed;}
			set{_seed = value;}
		}

		public int Increment
		{
			get{return _increment;}
			set{_increment = value;}
		}

		public int Not_x0020_For_x0020_Replication
		{
			get{return _not_for_replication;}
			set{_not_for_replication = value;}
		}

	}
}
