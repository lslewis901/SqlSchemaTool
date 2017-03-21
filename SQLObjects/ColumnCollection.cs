using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Lewis.SST.SQLObjects
{
	/// <summary>
	/// Collection of Column Objects.  Includes a method to serialize the entire collection. <seealso cref="COLUMN"/>
	/// </summary>
	[Serializable()]
	public class ColumnCollection : CollectionBase
	{
		private string _tableName;

		private Lewis.SST.SQLObjects.COLUMN.ColumnAction _action;

		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnCollection"/> class. Default constructor required for serialization.
		/// </summary>
		public ColumnCollection(){// Default constructor required for serialization
		}

		/// <summary>
		/// Creates the ColumnCollection instance with a passed in tablename
		/// </summary>
		/// <param name="tableName">String value for the SQL table name.</param>
		public ColumnCollection(string tableName)
		{
			_tableName = tableName;
		}

		/// <summary>
		/// Gets or sets the Tablename for the ColumnCollection object
		/// </summary>
		public string TableName
		{
			get	{return _tableName;	}
			set	{_tableName = value;}
		}

		/// <summary>
		/// Gets or sets the SchemaAction Enum for the ColumnCollection object.
		/// This is used in the serialization to create an Action attribute for each table object.
		/// </summary>
		public Lewis.SST.SQLObjects.COLUMN.ColumnAction SchemaAction
		{
			get{return _action;}
			set{_action = value;}
		}

		/// <summary>
		/// Adds a Column object to the ColumnCollection.
		/// </summary>
		/// <param name="_column">Column Object to add</param>
		public void Add(COLUMN _column)
		{
			List.Add( _column );
		}
		
		/// <summary>
		/// Gets the current Column object index from the ColumnCollection.
		/// </summary>
		public COLUMN this[int Index]
		{
			get{return (COLUMN)List[Index];}
		}

		/// <summary>
		/// Checks to see if the Column object is contained in the ColumnCollection.
		/// </summary>
		/// <param name="value">Column Object to check for.</param>
		/// <returns>returns true if Column object is in the ColumnCollection.</returns>
		public bool Contains( COLUMN value )  
		{
			// If value is not of type Column, this will return false.
			return( List.Contains( value ) );
		}

		/// <summary>
		/// Provides a method to serialize the entire column collection
		/// </summary>
		/// <param name="doc">Parent XMLDoc that the returned XML node will be appended to.</param>
		/// <returns>XMLNode that contains the serialized ColumnCollection.</returns>
		public XmlNode SerializeAsXmlNode(XmlDocument doc)
		{
			System.IO.MemoryStream ms = new MemoryStream();
			XmlSerializer serializer = new XmlSerializer(typeof(ColumnCollection));
			XmlTextReader xRead = null;
			XmlNode xTable = null;
			try
			{
				xTable = doc.CreateNode(XmlNodeType.Element, "TABLE", doc.NamespaceURI);
				// create child nodes to hold information from source
				xTable.AppendChild(doc.CreateNode(XmlNodeType.Element, "TABLE_NAME", doc.NamespaceURI));
				xTable.AppendChild(doc.CreateNode(XmlNodeType.Element, "TABLE_OWNER", doc.NamespaceURI));
				xTable.AppendChild(doc.CreateNode(XmlNodeType.Element, "TABLE_FILEGROUP", doc.NamespaceURI));
				xTable.AppendChild(doc.CreateNode(XmlNodeType.Element, "TABLE_REFERENCE", doc.NamespaceURI));
				xTable.AppendChild(doc.CreateNode(XmlNodeType.Element, "TABLE_CONSTRAINTS", doc.NamespaceURI));
				xTable.AppendChild(doc.CreateNode(XmlNodeType.Element, "TABLE_ORIG_CONSTRAINTS", doc.NamespaceURI));
				xTable.AppendChild(doc.CreateNode(XmlNodeType.Element, "TABLE_ORIG_REFERENCE", doc.NamespaceURI));
				xTable.Attributes.Append((XmlAttribute)doc.CreateNode(XmlNodeType.Attribute, "Action", doc.NamespaceURI));
				xTable.SelectSingleNode("TABLE_NAME").InnerXml = _tableName;
				xTable.Attributes["Action"].Value = _action.ToString();

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
				throw new Exception("ColumnCollection Serialization Error.", ex);
			}
			finally
			{
				ms.Close();
				if (xRead != null) xRead.Close();
			}
			return xTable;
		}
	}

	/// <summary>
	/// The Column Class, for creating an object that represents a single SQL table column's schema information. <seealso cref="ColumnCollection"/>
	/// </summary>
	[Serializable()]
	public class COLUMN
	{
		/// <summary>
		/// Column Action enumeration used to create an XML attribute for XML table nodes and column nodes.
		/// </summary>
		public enum ColumnAction
		{
			/// <summary>
			/// Indicates that this object/node should be added to the destination SQL server database.
			/// </summary>
			Add = 0,
			/// <summary>
			/// Indicates that this object/node should be altered or changed to the destination SQL server database.
			/// </summary>
			Alter,
			/// <summary>
			/// Indicates that this object/node should be dropped or removed to the destination SQL server database.
			/// </summary>
			Drop,
			/// <summary>
			/// Indicates that this object/node has no differences on the destination SQL server database from the source.
			/// </summary>
			UnChanged
		}

		private ColumnAction _action;
		private string _table_Name;
		private string _column_Name;
		private string _type;
		private string _base_type;
		private string _collation;
		private string _default_orig_name;
		private string _default_orig_value;
		private string _default_name;
		private string _default_owner;
		private string _default_value;
		private string _rule_name;
		private string _rule_owner;
		private string _rule_orig_name;
		private string _rule_orig_owner;
		private string _calc_Text;
		private string _computed;
		private string _identity;
		private string _rowguidcol;
		private string _ORIG_rowguidcol;
		private string _nullable;

		private string _notforrepl;
		private string _full_text;
		private string _ansipad;

		private int _length;
		private int _prec; 
		private int _scale; 
		private int _seed;
		private int _increment;

		/// <summary>
		/// main entry point to instantiate the column class object
		/// </summary>
		public COLUMN()
		{
		}

		/// <summary>
		/// main entry point to instantiate the column class object with a string Column_Name
		/// </summary>
		/// <param name="Column_Name"></param>
		public COLUMN(string Column_Name)
		{
			_column_Name = Column_Name;
		}

		/// <summary>
		/// Converts an XML node with the expected layout into a valid column object.
		/// </summary>
		/// <param name="xn">The XML column node to be converted into a column object.</param>
		/// <returns>The Column object result of the conversion process.</returns>
		public COLUMN Convert(XmlNode xn)
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
						case "column_name":
						{
							this.Column_Name = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "type":
						{
							this.Type = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "collation":
						{
							this.Collation = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "base_type":
						{
							this.Base_Type = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "default_orig_name":
						{
							this.Default_Orig_Name = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "default_name":
						{
							this.Default_Name = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "default_owner":
						{
							this.Default_Owner = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "default_value":
						{
							this.Default_Value = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "default_orig_value":
						{
							this.Default_Orig_Value = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "rule_name":
						{
							this.Rule_Name = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "rule_owner":
						{
							this.Rule_Owner = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "rule_orig_name":
						{
							this.Rule_Orig_Name = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "rule_orig_owner":
						{
							this.Rule_Orig_Owner = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "calc_text":
						{
							this.Calc_Text = x.InnerXml == null ? "" : x.InnerXml;
							break;
						}
						case "iscomputed":
						{
							this.isComputed = x.InnerXml.ToLower();
							break;
						}
						case "isidentity":
						{
							this.isIdentity = x.InnerXml.ToLower();
							break;
						}
						case "isrowguidcol":
						{
							this.isRowGuidCol = x.InnerXml.ToLower();
							break;
						}
						case "orig_rowguidcol":
						{
							this.ORIG_RowGuidCol = x.InnerXml.ToLower();
							break;
						}
						case "isnullable":
						{
							this.isNullable = x.InnerXml.ToLower();
							break;
						}
						case "notforrepl":
						{
							this.NotforRepl = x.InnerXml.ToLower();
							break;
						}
						case "fulltext":
						{
							this.FullText = x.InnerXml.ToLower();
							break;
						}
						case "ansipad":
						{
							this.AnsiPad = x.InnerXml.ToLower();
							break;
						}
						case "length":
						{
							this.Length = System.Convert.ToInt32( x.InnerXml.Length > 0 ? x.InnerXml : null );
							break;
						}
						case "prec":
						{
							this.Prec = System.Convert.ToInt32( x.InnerXml.Length > 0 ? x.InnerXml : null );
							break;
						}
						case "scale":
						{
							this.Scale = System.Convert.ToInt32( x.InnerXml.Length > 0 ? x.InnerXml : null );
							break;
						}
						case "seed":
						{
							this.Seed = System.Convert.ToInt32( x.InnerXml.Length > 0 ? x.InnerXml : null );
							break;
						}
						case "increment":
						{
							this.Increment = System.Convert.ToInt32( x.InnerXml.Length > 0 ? x.InnerXml : null );
							break;
						}
					}
				}
			}
			catch(Exception ex)
			{
				throw new Exception("Column Conversion Error.", ex);
			}
			return this;
		}

		/// <summary>
		/// Gets or sets the Column Object's ColumnAction property.
		/// </summary>
		public ColumnAction Action
		{
			get{return _action;}
			set{_action = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's TABLE_NAME property.
		/// </summary>
		public string TABLE_NAME
		{
			get{return _table_Name;}
			set{_table_Name = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Column_Name property.
		/// </summary>
		public string Column_Name
		{
			get{return _column_Name;}
			set{_column_Name = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Type property.
		/// The Type property is the same as a UDDT type or 
		/// if there is no UDDT bound to the Column, the type
		/// will be the same as the Base_Type.
		/// </summary>
		public string Type
		{
			get{return _type;}
			set{_type = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Base_Type property.
		/// </summary>
		public string Base_Type
		{
			get{return _base_type;}
			set{_base_type = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Length property.
		/// </summary>
		public int Length
		{
			get{return _length;}
			set{_length = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Prec (precision) property.
		/// </summary>
		public int Prec
		{
			get{return _prec;}
			set{_prec = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Scale property.
		/// </summary>
		public int Scale
		{
			get{return _scale;}
			set{_scale = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Seed property.
		/// </summary>
		public int Seed
		{
			get{return _seed;}
			set{_seed = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Increment property.
		/// </summary>
		public int Increment
		{
			get{return _increment;}
			set{_increment = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's isNullable property.
		/// </summary>
		public string isNullable
		{
			get{return _nullable;}
			set{_nullable = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's isIdentity property.
		/// </summary>
		public string isIdentity
		{
			get{return _identity;}
			set{_identity = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's isComputed property.
		/// </summary>
		public string isComputed
		{
			get{return _computed;}
			set{_computed = value;}
		}

//		/// <summary>
//		/// Gets or sets the Column Object's isIndexable property.
//		/// </summary>
//		public bool isIndexable
//		{
//			get{return _indexable;}
//			set{_indexable = value;}
//		}

		/// <summary>
		/// Gets or sets the Column Object's Default_Orig_Name property.
		/// This property is used to find the original default name so 
		/// that it can be removed when altering or dropping the column.
		/// </summary>
		public string Default_Orig_Name
		{
			get{return _default_orig_name;}
			set{_default_orig_name = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Default_Name property.
		/// </summary>
		public string Default_Name
		{
			get{return _default_name;}
			set{_default_name = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Rule_Name property.
		/// </summary>
		public string Rule_Name
		{
			get{return _rule_name;}
			set{_rule_name = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Rule_Owner property.
		/// </summary>
		public string Rule_Owner
		{
			get{return _rule_owner;}
			set{_rule_owner = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Rule_Orig_Name property.
		/// </summary>
		public string Rule_Orig_Name
		{
			get{return _rule_orig_name;}
			set{_rule_orig_name = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Rule_Orig_Owner property.
		/// </summary>
		public string Rule_Orig_Owner
		{
			get{return _rule_orig_owner;}
			set{_rule_orig_owner = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Default_Owner property.
		/// </summary>
		public string Default_Owner
		{
			get{return _default_owner;}
			set{_default_owner = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Default_Value property.
		/// </summary>
		public string Default_Value
		{
			get{return _default_value;}
			set{_default_value = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Default_Orig_Value property.
		/// </summary>
		public string Default_Orig_Value
		{
			get{return _default_orig_value;}
			set{_default_orig_value = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's NotforRepl property.
		/// </summary>
		public string NotforRepl
		{
			get{return _notforrepl;}
			set{_notforrepl = value;}
		}

		/// <summary>
		/// Gets or sets a value indicating whether [row GUID col].
		/// </summary>
		/// <value><c>true</c> if [row GUID col]; otherwise, <c>false</c>.</value>
		public string isRowGuidCol
		{
			get{return _rowguidcol;}
			set{_rowguidcol = value;}
		}

		/// <summary>
		/// Gets or sets a bool value indicating ORIG_isRowGuidCol is <c>true</c> or <c>false</c>.
		/// </summary>
		/// <value>if ORIG_RowGuidCol <c>true</c>; otherwise, <c>false</c>.</value>
		public string ORIG_RowGuidCol
		{
			get{return _ORIG_rowguidcol;}
			set{_ORIG_rowguidcol = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's FullText property.
		/// </summary>
		public string FullText
		{
			get{return _full_text;}
			set{_full_text = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's AnsiPad property.
		/// </summary>
		public string AnsiPad
		{
			get{return _ansipad;}
			set{_ansipad = value;}
		}

		/// <summary>
		/// Gets or sets the Column Object's Collation property.
		/// </summary>
		public string Collation
		{
			get{return _collation;}
			set{_collation = value;}
		}

		/// <summary>
		/// Gets or sets the calc_ text.
		/// </summary>
		/// <value>The calc_ text.</value>
		public string Calc_Text
		{
			get{return _calc_Text;}
			set{_calc_Text = value;}
		}
		
	}
}
