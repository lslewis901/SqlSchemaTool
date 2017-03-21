using System;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Collections;
using System.Xml.Serialization;

// borrowed from:
// http://www.xmlforasp.net/codebank/util/srcview.aspx?path=../../CodeBank/System_Xml_Schema/BuildSchema/BuildXMLSchema.src&file=SchemaBuilder.cs&font=3

namespace Lewis.Xml.Converters
{
	public enum NestingType 
	{
		RussianDoll,
		SeparateComplexTypes
	}

	public class Xml2Xsd
	{
		private Hashtable XmlNS = new Hashtable();
		private XmlNamespaceManager nsmgr;

        public void ValidateSchema(object sender, ValidationEventArgs args) { }

		public Xml2Xsd(System.Xml.XmlNameTable tableName)
		{
			// initialize code here
			nsmgr = new XmlNamespaceManager(tableName);
			nsmgr.AddNamespace("xsd", schemaNS);
			nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
			nsmgr.AddNamespace("msdata", "urn:schemas-microsoft-com:xml-msdata");

			foreach (String prefix in nsmgr)
			{
				XmlNS.Add(prefix, nsmgr.LookupNamespace(prefix));
			}
		}

		static string schemaNS = "http://www.w3.org/2001/XMLSchema";
		ArrayList complexTypes = new ArrayList();
		NestingType generationType;

		public string BuildXSD(string xml, NestingType type) 
		{
			generationType = type;
			//Create schema element
			XmlSchema schema = new XmlSchema();
			schema.ElementFormDefault = XmlSchemaForm.Qualified;
			schema.AttributeFormDefault = XmlSchemaForm.Unqualified;
			schema.Version = "1.0";
			//Add additional namespaces using the Add() method shown below
			//if desired
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add("xsd", schemaNS);
			ns.Add("xs", "http://www.w3.org/2001/XMLSchema");
			ns.Add("msdata", "urn:schemas-microsoft-com:xml-msdata");
			schema.Namespaces = ns;
            
			//Begin parsing source XML document
			XmlDocument doc = new XmlDocument();
			try 
			{ //Assume string XML
				doc.LoadXml(xml);
			}
			catch 
			{
				try 
				{ //String XML load failed.  Try loading as a file path
					doc.Load(xml);
				}
				catch 
				{
					return "XML document is not well-formed.";
				}
			}

			XmlElement root = doc.DocumentElement;

			//Create root element definition for schema
			//Call CreateComplexType to either add a complexType tag
			//or simply add the necesary schema attributes
			XmlSchemaElement elem = CreateComplexType(root);
			//Add root element definition into the XmlSchema object
			schema.Items.Add(elem);
			//Reverse elements in ArrayList so root complexType appears first
			//where applicable
			complexTypes.Reverse();
			//In cases where the user wants to separate out the complexType tags
			//loop through the complexType ArrayList and add the types to the schema
			foreach(object obj in complexTypes) 
			{
				XmlSchemaComplexType ct = (XmlSchemaComplexType)obj;
				schema.Items.Add(ct);
			}

			//Compile the schema and then write its contents to a StringWriter
			try 
			{
                XmlSchemaSet xss = new XmlSchemaSet();
                xss.Add(schema);
                xss.ValidationEventHandler += new ValidationEventHandler(ValidateSchema);
                xss.Compile();
				StringWriter sw = new StringWriter();
                string retval = sw.ToString();
				schema.Write(sw);
                sw.Flush();
                sw.Close();
                sw.Dispose();
                return retval;
			} 
			catch (Exception exp) 
			{
				return exp.Message;
			}
		}

		public XmlSchemaElement CreateComplexType(XmlElement element) 
		{
			ArrayList namesArray = new ArrayList();

			//Create complexType
			XmlSchemaComplexType ct = new XmlSchemaComplexType();
			if (element.HasChildNodes) 
			{
				//loop through children and place in schema sequence tag
				XmlSchemaSequence seq = new XmlSchemaSequence();
				foreach (XmlNode node in element.ChildNodes) 
				{
					if (node.NodeType == XmlNodeType.Element) 
					{
						if (namesArray.BinarySearch(node.Name) < 0) 
						{
							namesArray.Add(node.Name);
							namesArray.Sort(); //Needed for BinarySearch()
							XmlElement tempNode = (XmlElement)node;
							XmlSchemaElement sElem = null;
							//If node has children or attributes then create a new
							//complexType container
							if (tempNode.HasChildNodes || tempNode.HasAttributes) 
							{
								sElem = CreateComplexType(tempNode);
							} 
							else 
							{ //No comlexType needed...add SchemaTypeName
								sElem = new XmlSchemaElement();
								sElem.Name = tempNode.Name;

								string el_namesp = schemaNS;
								string el_name = sElem.Name;
								if (sElem.Name.IndexOf(":") > 0)
								{
									el_name = sElem.Name.Split(':')[1];
									string prefix = sElem.Name.Split(':')[0];
									el_namesp = XmlNS[prefix].ToString();
								}

								if (tempNode.InnerText == null || tempNode.InnerText == String.Empty) 
								{
									sElem.SchemaTypeName = new XmlQualifiedName("string", el_namesp);
								} 
								else 
								{
									//Try to detect the appropriate data type for the element
									sElem.SchemaTypeName = new XmlQualifiedName(CheckDataType(tempNode.InnerText), el_namesp);
								}
							}
							//Detect if node repeats in XML so we can handle maxOccurs
							try 
							{ 
								//We don't support namespaces now so prevent an error if one occurs
								if (element.SelectNodes(node.Name, nsmgr).Count > 1) 
								{
									sElem.MaxOccursString = "unbounded";
								}
							}
							catch (Exception ex)
							{
								throw ex;
							}
							//Add element to sequence tag
							seq.Items.Add(sElem);
						}
					}
				}
				//Add sequence tag to complexType tag
				if (seq.Items.Count > 0) ct.Particle = seq;
			}
			if (element.HasAttributes) 
			{
				foreach (XmlAttribute att in element.Attributes) 
				{
					if (att.Name.IndexOf("xmlns") == -1) 
					{
						string namesp = schemaNS;
						string name = att.Name;
						string prefix = string.Empty;
						if (att.Name.IndexOf(":") > 0)
						{
							name = att.Name.Split(':')[1];
							prefix = att.Name.Split(':')[0];
							namesp = XmlNS[prefix].ToString();
						}
						XmlSchemaAttribute sAtt = new XmlSchemaAttribute();
						sAtt.Name = name; 
						sAtt.SchemaTypeName = new XmlQualifiedName(CheckDataType(att.Value), namesp);
						ct.Attributes.Add(sAtt);
					}
				}
			}

			//Now that complexType is created, create element and add 
			//complexType into the element using its SchemaType property
			string elnamesp = schemaNS;
			string elname = element.Name;
			if (element.Name.IndexOf(":") > 0)
			{
				elname = element.Name.Split(':')[1];
				string prefix = element.Name.Split(':')[0];
				elnamesp = XmlNS[prefix].ToString();
			}
			XmlSchemaElement elem = new XmlSchemaElement();
			elem.Name = elname;
			if (ct.Attributes.Count > 0 || ct.Particle != null) 
			{
				//Handle nesting style of schema
				if (generationType == NestingType.SeparateComplexTypes) 
				{
					string typeName = element.Name + "Type";
					ct.Name = typeName;
					complexTypes.Add(ct);
					elem.SchemaTypeName = new XmlQualifiedName(typeName, null);
				} 
				else 
				{
					elem.SchemaType = ct;
				}
			} 
			else 
			{
				if (element.InnerText == null || element.InnerText == String.Empty) 
				{
					elem.SchemaTypeName = new XmlQualifiedName("string", elnamesp);
				} 
				else 
				{
					elem.SchemaTypeName = new XmlQualifiedName(CheckDataType(element.InnerText), elnamesp);
				}

			}
			return elem;
		}

		private string CheckDataType(string data) 
		{
			//Int test
			try 
			{
				Int32.Parse(data);
				return "int";
			} 
			catch {}

			//Decimal test
			try 
			{
				Decimal.Parse(data);
				return "decimal";
			} 
			catch {}

			//DateTime test
			try 
			{
				DateTime.Parse(data);
				return "dateTime";
			} 
			catch {}

			//Boolean test
			if (data.ToLower() == "true" || data.ToLower() == "false") 
			{
				return "boolean";
			}

			return "string";
		}

	}
}

