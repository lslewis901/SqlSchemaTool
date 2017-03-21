using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Lewis.Xml
{
	/// <summary>
	/// XSLTHelper class performs the XSL transformation of the XML doc loaded into the editor control.
	/// </summary>
	public static class XsltHelper
	{
        /// <summary>
        /// constant string value used by xslthelper class
        /// </summary>
        public const string SQLCREATEXSLT = "Lewis.SST.Xslt.Create_DB_Schema.xslt";
        /// <summary>
        /// constant string value used by xslthelper class
        /// </summary>
        public const string SQLDIFFXSLT = "Lewis.SST.Xslt.Diff_DB_Schema.xslt";
        /// <summary>
        /// constant string value used by xslthelper class
        /// </summary>
        public const string DEFAULTXSLT = "Lewis.SST.Xslt.DefaultSS.xslt";
        /// <summary>
        /// constant string value used by xslthelper class
        /// </summary>
        public const string PRETTYXSLT = "Lewis.SST.Xslt.Pretty.xslt";
        /// <summary>
        /// constant string value used by xslthelper class
        /// </summary>
        public const string HTMLXSLT = "Lewis.SST.Xslt.HTML_Table_View.xslt";

        /// <summary>
        /// Transforms the specified XML string
        /// </summary>
        /// <param name="xmlString"></param>
        /// <param name="embeddedResourcePath"></param>
        /// <returns></returns>
		public static string Transform(string xmlString, string embeddedResourcePath)
		{
            string xslString = ResourceReader.ReadFromResource(embeddedResourcePath);

            string result = string.Empty;

            try
            {
                XmlDocument xsl = new XmlDocument();
                xsl.LoadXml(xslString);

                StringWriter sw = new StringWriter();
                XmlTextReader xtr = new XmlTextReader(new StringReader(xmlString));
                XmlTextWriter xtw = new XmlTextWriter(sw);
                XslCompiledTransform t = new XslCompiledTransform();
                t.Load((IXPathNavigable)xsl, null, null);
                t.Transform(xtr, null, xtw, null);
                result = sw.ToString();
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
            catch(Exception ex) 
            {
                result = string.Format("{0} had the following error with the XML file:\n{1}\n{2}", ex.Source, ex.Message, xmlString);
            }

			return result;

		}

        /// <summary>
        /// Apply XSL Transformations against an XML file.
        /// Used to transform either the create XML schema file
        /// or the Diffgram XML schema file into a SQL script file.
        /// </summary>
        /// <param name="readFile">string value of the XML file name to read.</param>
        /// <param name="transformation">string value representing the embedded resouce locator path.</param>
        /// <param name="outputFile">string value of the XML file name to write.</param>
        public static void SQLTransform(string readFile, string transformation, string outputFile)
        {
            XmlTextReader xtr = new XmlTextReader(readFile);
            XPathDocument xmlPathDoc = new XPathDocument(xtr);

            // .Net Xslt Transformer
            // captures the transformed xml output as a stream so that we can do the
            // replacement of the escaped character sequences
            XslCompiledTransform xslTran = new XslCompiledTransform();
            XmlDocument xsl = new XmlDocument();
            string xslString = ResourceReader.ReadFromResource(transformation);
            xsl.LoadXml(xslString);
            xslTran.Load((IXPathNavigable)xsl);
            System.IO.MemoryStream ms = new MemoryStream();
            XmlTextWriter txtwriter = new XmlTextWriter(ms, System.Text.Encoding.ASCII);
            xslTran.Transform(xmlPathDoc, txtwriter);
            StringBuilder output = new StringBuilder(Encoding.UTF8.GetString(ms.ToArray()));
            output.Replace("&lt;", "<");
            output.Replace("&gt;", ">");
            output.Replace("&amp;", "&");
            StreamWriter sw = new StreamWriter(outputFile, false, System.Text.Encoding.ASCII);
            sw.Write(output.ToString());
            sw.Flush();
            sw.Close();
            ms.Close();
            sw.Dispose();
            ms.Dispose();
        }

	}
}
