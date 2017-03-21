using System;
using System.Xml;
using System.Xml.Schema;

namespace Lewis.Xml.Converters
{
	/// <summary>
	/// Build template of xml file by schema file.
	/// </summary>
	public class Xsd2Xml
	{
		public Xsd2Xml()
		{
		}

		public void Convert(string SrcXmlSchemaFileName, string DestXmlFileName)
		{
			GetXmlDocument(SrcXmlSchemaFileName).Save(DestXmlFileName);
		}

		public XmlDocument GetXmlDocument(string SrcXmlSchemaFileName)
		{
			XmlDocument xmlDocument = new XmlDocument();
			System.IO.StreamReader streamReader = new System.IO.StreamReader(SrcXmlSchemaFileName);
            XmlSchemaSet xss = new XmlSchemaSet();
			XmlSchema xmlSchema = XmlSchema.Read(streamReader, null);
			streamReader.Close();
            xss.Add(xmlSchema);
            xss.Compile();
			foreach(XmlSchemaElement xmlSchemaElement in xmlSchema.Elements.Values)
			{
				IterChildElement(xmlDocument, xmlSchemaElement);
			}

			return xmlDocument;
		}


		private void IterChildElement(XmlNode XmlParentNode, XmlSchemaElement XmlSchemaElement)
		{
			XmlNode XmlSubParentNode = AppendCorrespondedXmlNode(XmlParentNode, XmlSchemaElement);
			XmlSchemaComplexType complexType = XmlSchemaElement.SchemaType as XmlSchemaComplexType;
			if (complexType != null)
			{
				XmlSchemaSequence subElements = complexType.Particle as XmlSchemaSequence;	
				foreach(XmlSchemaElement childElement in subElements.Items)
				{
					decimal numNewXmlNodes = generateArrayByMinOccur ? childElement.MinOccurs : childElement.MaxOccurs;
					for (int i = 0; i <= numNewXmlNodes - 1; i++)
					{
						IterChildElement(XmlSubParentNode, childElement);
					}
				}
			}

		}

		private XmlNode AppendCorrespondedXmlNode(XmlNode XmlParentNode, XmlSchemaElement XmlSchemaElement)
		{
			XmlNode xmlNode = GetOwnerDoument(XmlParentNode).CreateNode(XmlNodeType.Element, XmlSchemaElement.Name, "");
			XmlParentNode.AppendChild(xmlNode);
			return xmlNode;
		}

		private XmlDocument GetOwnerDoument(XmlNode XmlNode)
		{
			return XmlNode is XmlDocument ? (XmlDocument)XmlNode : XmlNode.OwnerDocument;
		}

		public bool generateArrayByMinOccur = true;

	}
}
