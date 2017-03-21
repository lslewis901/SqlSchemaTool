using System;
using System.Data;
using System.Web;
using System.Web.SessionState;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

#region change history
/// 03-09-2009: C02: LLEWIS: changes to handle errors if DataTable(s) is null in 
///                          ExportData methods
#endregion


namespace Lewis.Xml.Converters
{
	# region Summary

	/// <summary>
	/// Exports datatable to CSV or Excel format.
	/// This uses DataSet's XML features and XSLT for exporting.
	/// 
	/// C#.Net Example to be used in WebForms
	/// ------------------------------------- 
	/// using MyLib.ExportData;
	/// 
	/// private void btnExport_Click(object sender, System.EventArgs e)
	/// {
	///   try
	///   {
	///     // Declarations
	///     DataSet dsUsers =  ((DataSet) Session["dsUsers"]).Copy( );
    ///     Lewis.XML.ExportData.Export export = new Lewis.XML.ExportData.Export("Web"); 
	///     string FileName = "UserList.csv";
	///     int[] ColList = {2, 3, 4, 5, 6};
    ///     export.ExportData(dsUsers.Tables[0], ColList, Export.ExportFormat.CSV, FileName);
	///   }
	///   catch(Exception Ex)
	///   {
	///     lblError.Text = Ex.Message;
	///   }
	/// }	
	/// </summary>

	# endregion // Summary

	public class Export
	{		
		public enum ExportFormat : int {CSV = 1, Excel = 2, XML = 3}; // Export format enumeration			

		System.Web.HttpResponse response;

		private string appType;	
			
		public Export()
		{
			appType = "Web";
			response = System.Web.HttpContext.Current.Response;
		}

		public Export(string ApplicationType)
		{
			appType = ApplicationType;
			if(appType != "Web" && appType != "Win") throw new Exception("Provide valid application format (Web/Win)");
			if (appType == "Web") response = System.Web.HttpContext.Current.Response;
		}
		
		#region ExportData OverLoad : Type#1
		
		// Function  : ExportData 
		// Arguments : table, FormatType, FileName
		// Purpose	 : To get all the column headers in the datatable and 
		//			   exorts in CSV / Excel format with all columns

		public void ExportData(DataTable table, ExportFormat FormatType, string FileName)
		{
			try
			{
                if (table == null || table.Rows.Count == 0) 
					throw new Exception("There are no details to export.");				
				
				// Create Dataset
                using (DataSet dsExport = new DataSet("Export"))
                {
                    using (DataTable dtExport = table.Copy())
                    {
                        dtExport.TableName = "Values";
                        dsExport.Tables.Add(dtExport);

                        // Getting Field Names
                        string[] sHeaders = new string[dtExport.Columns.Count];
                        string[] sFileds = new string[dtExport.Columns.Count];

                        for (int i = 0; i < dtExport.Columns.Count; i++)
                        {
                            sHeaders[i] = dtExport.Columns[i].ColumnName;
                            sFileds[i] = dtExport.Columns[i].ColumnName;
                        }

                        if (appType == "Web")
                            Export_with_XSLT_Web(dsExport, sHeaders, sFileds, FormatType, FileName);
                        else if (appType == "Win")
                            Export_with_XSLT_Windows(dsExport, sHeaders, sFileds, FormatType, FileName);
                    }
                }
			}			
			catch(Exception Ex)
			{
				throw Ex;
			}			
		}

		#endregion // ExportData OverLoad : Type#1

		#region ExportData OverLoad : Type#2

		// Function  : ExportData 
		// Arguments : table, ColumnList, FormatType, FileName		
		// Purpose	 : To get the specified column headers in the datatable and
		//			   exorts in CSV / Excel format with specified columns

		public void ExportData(DataTable table, int[] ColumnList, ExportFormat FormatType, string FileName)
		{
			try
			{
                if (table == null || table.Rows.Count == 0)
					throw new Exception("There are no details to export");
				
				// Create Dataset
                using (DataSet dsExport = new DataSet("Export"))
                {
                    using (DataTable dtExport = table.Copy())
                    {
                        dtExport.TableName = "Values";
                        dsExport.Tables.Add(dtExport);

                        if (ColumnList.Length > dtExport.Columns.Count)
                            throw new Exception("ExportColumn List should not exceed Total Columns");

                        // Getting Field Names
                        string[] sHeaders = new string[ColumnList.Length];
                        string[] sFileds = new string[ColumnList.Length];

                        for (int i = 0; i < ColumnList.Length; i++)
                        {
                            if ((ColumnList[i] < 0) || (ColumnList[i] >= dtExport.Columns.Count))
                                throw new Exception("ExportColumn Number should not exceed Total Columns Range");

                            sHeaders[i] = dtExport.Columns[ColumnList[i]].ColumnName;
                            sFileds[i] = dtExport.Columns[ColumnList[i]].ColumnName;
                        }

                        if (appType == "Web")
                            Export_with_XSLT_Web(dsExport, sHeaders, sFileds, FormatType, FileName);
                        else if (appType == "Win")
                            Export_with_XSLT_Windows(dsExport, sHeaders, sFileds, FormatType, FileName);
                    }
                }
			}			
			catch(Exception Ex)
			{
				throw Ex;
			}			
		}
		
		#endregion // ExportData OverLoad : Type#2

		#region ExportData OverLoad : Type#3

		// Function  : ExportData 
		// Arguments : table, ColumnList, Headers, FormatType, FileName	
		// Purpose	 : To get the specified column headers in the datatable and	
		//			   exorts in CSV / Excel format with specified columns and 
		//			   with specified headers

		public void ExportData(DataTable table, int[] ColumnList, string[] Headers, ExportFormat FormatType, 
			string FileName)
		{
			try
			{
                if (table == null || table.Rows.Count == 0)
					throw new Exception("There are no details to export");
				
				// Create Dataset
                using (DataSet dsExport = new DataSet("Export"))
                {
                    using (DataTable dtExport = table.Copy())
                    {
                        dtExport.TableName = "Values";
                        dsExport.Tables.Add(dtExport);

                        if (ColumnList.Length != Headers.Length)
                            throw new Exception("ExportColumn List and Headers List should be of same length");
                        else if (ColumnList.Length > dtExport.Columns.Count || Headers.Length > dtExport.Columns.Count)
                            throw new Exception("ExportColumn List should not exceed Total Columns");

                        // Getting Field Names
                        string[] sFileds = new string[ColumnList.Length];

                        for (int i = 0; i < ColumnList.Length; i++)
                        {
                            if ((ColumnList[i] < 0) || (ColumnList[i] >= dtExport.Columns.Count))
                                throw new Exception("ExportColumn Number should not exceed Total Columns Range");

                            sFileds[i] = dtExport.Columns[ColumnList[i]].ColumnName;
                        }

                        if (appType == "Web")
                            Export_with_XSLT_Web(dsExport, Headers, sFileds, FormatType, FileName);
                        else if (appType == "Win")
                            Export_with_XSLT_Windows(dsExport, Headers, sFileds, FormatType, FileName);
                    }
                }
			}			
			catch(Exception Ex)
			{
				throw Ex;
			}			
		}
		
		#endregion // ExportData OverLoad : Type#3

		#region ExportData OverLoad : Type#3
		
		// Function  : ExportData 
		// Arguments : table, FormatType, FileName
		// Purpose	 : To get all the column headers in the datatable and 
		//			   exorts in CSV / Excel format with all columns

		public void ExportData(DataTableCollection tables, ExportFormat FormatType, string FileName)
		{
			try
			{
				string NewFileName;
		        if (tables == null)
                    throw new Exception("There are no details to export.");				

				foreach(DataTable DetailsTable in tables)
				{
					if(DetailsTable.Rows.Count == 0) 
						throw new Exception("There are no details to export.");				
					
					NewFileName = FileName.Substring(0,FileName.LastIndexOf("."));
					NewFileName+= " - " + DetailsTable.TableName;
					NewFileName+= FileName.Substring(FileName.LastIndexOf("."));
								
					// Create Dataset
					DataSet dsExport = new DataSet("Export");
					DataTable dtExport = DetailsTable.Copy();
					dtExport.TableName = "Values"; 
					dsExport.Tables.Add(dtExport);	
				
					// Getting Field Names
					string[] sHeaders = new string[dtExport.Columns.Count];
					string[] sFileds = new string[dtExport.Columns.Count];
				
					for (int i=0; i < dtExport.Columns.Count; i++)
					{
						sHeaders[i] = dtExport.Columns[i].ColumnName;
						sFileds[i] = dtExport.Columns[i].ColumnName;					
					}

					if(appType == "Web")
						Export_with_XSLT_Web(dsExport, sHeaders, sFileds, FormatType, NewFileName);
					else if(appType == "Win")
						Export_with_XSLT_Windows(dsExport, sHeaders, sFileds, FormatType, NewFileName);
				}
			}			
			catch(Exception Ex)
			{
				throw Ex;
			}
        }

        #endregion //ExportData OverLoad : Type#4

        public void ExportDataSetToExcel(DataSet ds, string filename)
        {
            // first let's clean up the response.object
            //response.Clear();
            //response.Charset = "";

            // set the response mime type for excel
            //response.ContentType = "application/vnd.ms-excel";
            //response.AddHeader("Content-Disposition", "attachment;filename=\"" + filename + "\"");

            // create a string writer
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    // instantiate a datagrid
                    GridView dg = new GridView();
                    for(int ii = 1; ii < ds.Tables.Count; ii++)
                    {
                        DataTable dt = ds.Tables[ii];
                        GridView dgChild = new GridView();
                        dg.Controls.Add(dgChild);
                        dgChild.DataSource = dt;
                        dgChild.DataBind();
                    }
                    dg.DataSource = ds.Tables[0];
                    dg.DataBind();
                    dg.RenderControl(htw);
                    File.AppendAllText(filename,sw.ToString());
                    //response.End();
                }
            }
        }


		#region Export_with_XSLT_Web

		// Function  : Export_with_XSLT_Web 
		// Arguments : dsExport, sHeaders, sFileds, FormatType, FileName
		// Purpose   : Exports dataset into CSV / Excel format

		private void Export_with_XSLT_Web(DataSet dsExport, string[] sHeaders, string[] sFileds, ExportFormat FormatType, string FileName)
		{
			try
			{				
				// Appending Headers
				response.Clear();
				response.Buffer= true;
				
				if(FormatType == ExportFormat.CSV)
				{
					response.ContentType = "text/csv";
					response.AppendHeader("content-disposition", "attachment; filename=" + FileName);
				}		
				else
				{
					response.ContentType = "application/vnd.ms-excel";
					response.AppendHeader("content-disposition", "attachment; filename=" + FileName);
				}

				// XSLT to use for transforming this dataset.						
				MemoryStream stream = new MemoryStream( );
				XmlTextWriter writer = new XmlTextWriter(stream, Encoding.Default);
				CreateStylesheet(writer, sHeaders, sFileds, FormatType);
				writer.Flush( ); 
				stream.Seek( 0, SeekOrigin.Begin);
                XmlDocument xsl = new XmlDocument();
                xsl.Load(stream);

                //XslTransform xslTran = new XslTransform();				
                //xslTran.Load(new XmlTextReader(stream), null, null);				
                //System.IO.StringWriter  sw = new System.IO.StringWriter();			
                //xslTran.Transform(xmlDoc, null, sw, null);

                XmlDataDocument xmlDoc = new XmlDataDocument(dsExport);

                StringWriter sw = new StringWriter();
                XmlTextWriter xtw = new XmlTextWriter(sw);
                XslCompiledTransform t = new XslCompiledTransform();
                t.Load((IXPathNavigable)xsl, null, null);
                t.Transform((IXPathNavigable)xmlDoc, xtw);
					
				//Writeout the Content				
				response.Write(sw.ToString());			
				sw.Close();
                xtw.Close();
				writer.Close();
				stream.Close();			
				response.End();
                sw.Dispose();
                stream.Dispose();
			}
			catch(ThreadAbortException Ex)
			{
				string ErrMsg = Ex.Message;
			}
			catch(Exception Ex)
			{
				throw Ex;
			}
		}		
		
		#endregion // Export_with_XSLT 

		#region Export_with_XSLT_Windows 

		// Function  : Export_with_XSLT_Windows 
		// Arguments : dsExport, sHeaders, sFileds, FormatType, FileName
		// Purpose   : Exports dataset into CSV / Excel format

		private void Export_with_XSLT_Windows(DataSet dsExport, string[] sHeaders, string[] sFileds, ExportFormat FormatType, string FileName)
		{
			
			try
			{				
				// XSLT to use for transforming this dataset.						
				MemoryStream stream = new MemoryStream( );
				XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);				
				CreateStylesheet(writer, sHeaders, sFileds, FormatType);
				writer.Flush( ); 
				stream.Seek( 0, SeekOrigin.Begin);
                XmlDocument xsl = new XmlDocument();
                xsl.Load(stream);

                //XslTransform xslTran = new XslTransform();				
                //xslTran.Load(new XmlTextReader(stream), null, null);				
                //System.IO.StringWriter  sw = new System.IO.StringWriter();			
                //xslTran.Transform(xmlDoc, null, sw, null);

                XmlDataDocument xmlDoc = new XmlDataDocument(dsExport);

                StringWriter sw = new StringWriter();
                XmlTextWriter xtw = new XmlTextWriter(sw);
                XslCompiledTransform t = new XslCompiledTransform();
                t.Load((IXPathNavigable)xsl, null, null);
                t.Transform((IXPathNavigable)xmlDoc, xtw);

                //Writeout the Content				
                File.WriteAllText(FileName, sw.ToString());
                sw.Close();
                xtw.Close();
                writer.Close();
                stream.Close();
                sw.Dispose();
                stream.Dispose();
            }			
			catch(Exception Ex)
			{
				throw Ex;
			}
		}		
		
		#endregion // Export_with_XSLT 

		#region CreateStylesheet 

		// Function  : WriteStylesheet 
		// Arguments : writer, sHeaders, sFileds, FormatType
		// Purpose   : Creates XSLT file to apply on dataset's XML file 

		private void CreateStylesheet(XmlTextWriter writer, string[] sHeaders, string[] sFileds, ExportFormat FormatType)
		{
			try
			{
				// xsl:stylesheet
				string ns = "http://www.w3.org/1999/XSL/Transform";	
				writer.Formatting = Formatting.Indented;
				writer.WriteStartDocument( );				
				writer.WriteStartElement("xsl","stylesheet",ns);
				writer.WriteAttributeString("version","1.0");
				writer.WriteStartElement("xsl:output");
				writer.WriteAttributeString("method","text");
				writer.WriteAttributeString("version","4.0");
				writer.WriteEndElement( );
				
				// xsl-template
				writer.WriteStartElement("xsl:template");
				writer.WriteAttributeString("match","/");

				// xsl:value-of for headers
				for(int i=0; i< sHeaders.Length; i++)
				{
					writer.WriteString("\"");
					writer.WriteStartElement("xsl:value-of");
					writer.WriteAttributeString("select", "'" + sHeaders[i] + "'");
					writer.WriteEndElement( ); // xsl:value-of
					writer.WriteString("\"");
					if (i != sFileds.Length - 1) writer.WriteString( (FormatType == ExportFormat.CSV ) ? "," : "	" );
				}
								
				// xsl:for-each
				writer.WriteStartElement("xsl:for-each");
				writer.WriteAttributeString("select","Export/Values");
				writer.WriteString("\r\n");				
				
				// xsl:value-of for data fields
				for(int i=0; i< sFileds.Length; i++)
				{					
					writer.WriteString("\"");
					writer.WriteStartElement("xsl:value-of");
					writer.WriteAttributeString("select", sFileds[i]);
					writer.WriteEndElement( ); // xsl:value-of
					writer.WriteString("\"");
					if (i != sFileds.Length - 1) writer.WriteString( (FormatType == ExportFormat.CSV ) ? "," : "	" );
				}
								
				writer.WriteEndElement( ); // xsl:for-each
				writer.WriteEndElement( ); // xsl-template
				writer.WriteEndElement( ); // xsl:stylesheet
				writer.WriteEndDocument( );					
			}
			catch(Exception Ex)
			{
				throw Ex;
			}
		}
		
		#endregion // WriteStylesheet

	}
}
