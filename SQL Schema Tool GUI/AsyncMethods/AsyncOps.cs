using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

using Lewis.SST;
using Lewis.SST.Controls;
using Lewis.Xml;
using Lewis.Xml.Converters;

using NLog;

using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace Lewis.SST.AsyncMethods
{
	// <summary>The Lewis.SST.AsyncMethods namespace classes are used to provide asynchronous threaded operations. 
	// The use of these classes does require making changes to the overridden DoWork method in the AsyncThread class.
	// </summary>

    /// <summary>
    /// AsyncCreateXSD class.
    /// </summary>
    public sealed class AsyncCreateXSD : AsyncOperation
    {
        private static Logger logger = LogManager.GetLogger("Lewis.SST.AsyncMethods.AsyncCreateXSD");

        /// <summary>
        /// Delegate declaration for async call to create XSD document.
        /// </summary>
        public delegate void CreateXSDDelegate(string FileName);
        /// <summary>
        /// Event handler declaration for async call to create XSD document.
        /// </summary>
        public event CreateXSDDelegate CreateXSD;

        private XmlDocument _xmlDoc;
        private string _fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCreateXSD"/> class.
        /// </summary>
        /// <param name="isi">The isi.</param>
        /// <param name="xmlDoc">The XML doc.</param>
        /// <param name="FileName">Name of the file.</param>
        public AsyncCreateXSD(ISynchronizeInvoke isi, XmlDocument xmlDoc, string FileName)
            : base(isi)
        {
            // populate private vars
            _xmlDoc = xmlDoc;
            _fileName = FileName;
        }

        /// <summary>
        /// To be overridden by the deriving class - this is where the work
        /// will be done.  The base class calls this method on a worker
        /// thread when the Start method is called.
        /// </summary>
        protected override void DoWork()
        {
            DoCreateXSD();
            // When a cancel occurs, we'd better acknowledge cancellation.
            if (CancelRequested)
            {
                AcknowledgeCancel();
            }
        }

        private void DoCreateXSD()
        {
            string xsdOutput = string.Empty;
            try
            {
                Xml2Xsd xsd = new Xml2Xsd(_xmlDoc.NameTable);
                xsdOutput = xsd.BuildXSD(_xmlDoc.OuterXml, NestingType.SeparateComplexTypes);

                // create XSD file from string
                System.Xml.XmlTextWriter xw = new XmlTextWriter(_fileName, System.Text.Encoding.UTF8);
                xw.WriteRaw(xsdOutput);
                xw.Close();
            }
            catch (Exception ex)
            {
                logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
            }

            // Stop if cancellation was requested
            if (!CancelRequested)
            {
                OnCreateXSD(_fileName);
            }
        }

        private void OnCreateXSD(string FileName)
        {
            lock (this)
            {
                FireAsync(CreateXSD, FileName);
            }
        }
    }

    /// <summary>
    /// Async SerializeDB class
    /// </summary>
    public sealed class AsyncSerializeDB : AsyncOperation
    {
        private static Logger logger = LogManager.GetLogger("Lewis.SST.AsyncMethods.AsyncSerializeDB");

        public delegate void CompleteSerializeDBDelegate(string FileName);
        public event CompleteSerializeDBDelegate CompleteSerializeDB;

        private string _SQLServer;
        private string _DBName;
        private string _UID;
        private string _PWD;
        private string _SQLfile;
        private bool _Translate;
        private bool _Primary;
        private byte _SQLObjects = 0xff;
        private string _customXSLT;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCreateXSD"/> class.
        /// </summary>
        /// <param name="isi">The isi.</param>
        /// <param name="SQLServer"></param>
        /// <param name="DBName"></param>
        /// <param name="UID"></param>
        /// <param name="PWD"></param>
        /// <param name="SQLfile"></param>
        /// <param name="Translate"></param>
        /// <param name="Primary"></param>
        public AsyncSerializeDB(ISynchronizeInvoke isi, string SQLServer, string DBName, string UID, string PWD, string SQLfile,
            bool Translate, bool Primary, byte SQLObjects, string CustomXSLT)
            : base(isi)
        {
            // populate private vars
            _SQLServer = SQLServer;
            _DBName = DBName;
            _UID = UID;
            _PWD = PWD;
            _SQLfile = SQLfile;
            _Translate = Translate;
            _Primary = Primary;
            _SQLObjects = SQLObjects;
            _customXSLT = CustomXSLT;
        }

        /// <summary>
        /// To be overridden by the deriving class - this is where the work
        /// will be done.  The base class calls this method on a worker
        /// thread when the Start method is called.
        /// </summary>
        protected override void DoWork()
        {
            DoSerializeDB();
            // When a cancel occurs, we'd better acknowledge cancellation.
            if (CancelRequested)
            {
                AcknowledgeCancel();
            }
        }

        private void DoSerializeDB()
        {
            string outputFileName = string.Empty;
            try
            {
                outputFileName = SQLSchemaTool.SerializeDB(_SQLServer, _DBName, _UID, _PWD, _SQLfile, _Translate, _Primary, true, _SQLObjects, _customXSLT);
            }
            catch (Exception ex)
            {
                logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
            }

            // if no cancellation was requested fire event delegate
            if (!CancelRequested)
            {
                OnCompleteSerializeDB(outputFileName);
            }
        }

        private void OnCompleteSerializeDB(string FileName)
        {
            lock (this)
            {
                if (CompleteSerializeDB != null)
                {
                    FireAsync(CompleteSerializeDB, FileName);
                }
            }
        }
    }

    /// <summary>
    /// Async TransformSQL class transforms from xml to SQL
    /// </summary>
    public sealed class AsyncTransformSQL : AsyncOperation
    {
        private static Logger logger = LogManager.GetLogger("Lewis.SST.AsyncMethods.AsyncTransformXML");

        public delegate void CompleteTransformDelegate(string FileName);
        public event CompleteTransformDelegate CompleteTransformSQL;

        public enum TransformType
        { 
            Create,
            Diff
        }

        private string _inputFileName;
        private string _outputFileName;
        private TransformType _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCreateXSD"/> class.
        /// </summary>
        /// <param name="isi">The isi.</param>
        /// <param name="SQLServer"></param>
        /// <param name="DBName"></param>
        /// <param name="UID"></param>
        /// <param name="PWD"></param>
        /// <param name="SQLfile"></param>
        /// <param name="Translate"></param>
        /// <param name="Primary"></param>
        public AsyncTransformSQL(ISynchronizeInvoke isi, string inputFileName, string outputFileName, TransformType type)
            : base(isi)
        {
            // populate private vars
            _inputFileName = inputFileName;
            _outputFileName = outputFileName;
            _type = type;
        }

        /// <summary>
        /// To be overridden by the deriving class - this is where the work
        /// will be done.  The base class calls this method on a worker
        /// thread when the Start method is called.
        /// </summary>
        protected override void DoWork()
        {
            DoSQLTransform();
            // When a cancel occurs, we'd better acknowledge cancellation.
            if (CancelRequested)
            {
                AcknowledgeCancel();
            }
        }

        private void DoSQLTransform()
        {
            string transformType = _type == TransformType.Create ? XsltHelper.SQLCREATEXSLT : XsltHelper.SQLDIFFXSLT;
            string outputFileName = _outputFileName;
            try
            {
                XsltHelper.SQLTransform(_inputFileName, transformType, outputFileName);
            }
            catch (Exception ex)
            {
                logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
            }

            // if no cancellation was requested fire event delegate
            if (!CancelRequested)
            {
                OnCompleteTransformSQL(outputFileName);
            }
        }

        private void OnCompleteTransformSQL(string FileName)
        {
            lock (this)
            {
                if (CompleteTransformSQL != null)
                {
                    FireAsync(CompleteTransformSQL, FileName);
                }
            }
        }
    }

    /// <summary>
    /// Async CompareDB class
    /// </summary>
    public sealed class AsyncCompareDB : AsyncOperation
    {
        private static Logger logger = LogManager.GetLogger("Lewis.SST.AsyncMethods.AsyncCompareDB");

        public delegate void CompleteCompareDBDelegate(string FileName);
        public event CompleteCompareDBDelegate CompleteCompareDB;

        private string _Source;
        private string _Destination;
        private string _diffFile;
        private string _SQLfile;
        private bool _CompareSprocText;
        private bool _CompareViewText;
        private bool _IsTranslate;
        private byte _SQLObjects = 0xff;
        private string _customXSLT;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCreateXSD"/> class.
        /// </summary>
        /// <param name="isi">The isi.</param>
        /// <param name="SQLServer"></param>
        /// <param name="DBName"></param>
        /// <param name="UID"></param>
        /// <param name="PWD"></param>
        /// <param name="SQLfile"></param>
        /// <param name="Translate"></param>
        /// <param name="Primary"></param>
        public AsyncCompareDB(ISynchronizeInvoke isi, string Source, string Destination, string DiffFile, string SQLfile,
            bool CompareSprocText, bool CompareViewText, bool IsTranslate, byte SQLObjects, string CustomXSLT)
            : base(isi)
        {
            // populate private vars
            _Source = Source;
            _Destination = Destination;
            _diffFile = DiffFile;
            _SQLfile = SQLfile;
            _CompareSprocText = CompareSprocText;
            _CompareViewText = CompareViewText;
            _IsTranslate = IsTranslate;
            _SQLObjects = SQLObjects;
            _customXSLT = CustomXSLT;
        }

        /// <summary>
        /// To be overridden by the deriving class - this is where the work
        /// will be done.  The base class calls this method on a worker
        /// thread when the Start method is called.
        /// </summary>
        protected override void DoWork()
        {
            DoCompareDB();
            // When a cancel occurs, we'd better acknowledge cancellation.
            if (CancelRequested)
            {
                AcknowledgeCancel();
            }
        }

        private void DoCompareDB()
        {
            string outputFileName = string.Empty;
            try
            {
                outputFileName = SQLSchemaTool.CompareSchema(_Source, _Destination, _diffFile, _SQLfile, _CompareSprocText,
                    _CompareViewText, _IsTranslate, _SQLObjects, _customXSLT);
            }
            catch (Exception ex)
            {
                logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
            }

            // if no cancellation was requested fire event delegate
            if (!CancelRequested)
            {
                OnCompleteCompareDB(outputFileName);
            }
        }

        private void OnCompleteCompareDB(string FileName)
        {
            lock (this)
            {
                if (CompleteCompareDB != null)
                {
                    FireAsync(CompleteCompareDB, FileName);
                }
            }
        }
    }

    /// <summary>
    /// Async TransformHTML class transforms from XML to HTML
    /// </summary>
    public sealed class AsyncTransformHTML : AsyncOperation
    {
        private static Logger logger = LogManager.GetLogger("Lewis.SST.AsyncMethods.AsyncTransformHTML");

        public delegate void CompleteTransformDelegate(string FileName);
        public event CompleteTransformDelegate CompleteTransformHTML;

        public enum TransformType
        {
            Report,
            Standard,
            Custom
        }

        private string _inputFileName;
        private string _outputFileName;
        private string _customXSLT;
        private TransformType _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCreateXSD"/> class.
        /// </summary>
        /// <param name="isi">The isi.</param>
        /// <param name="SQLServer"></param>
        /// <param name="DBName"></param>
        /// <param name="UID"></param>
        /// <param name="PWD"></param>
        /// <param name="SQLfile"></param>
        /// <param name="Translate"></param>
        /// <param name="Primary"></param>
        public AsyncTransformHTML(ISynchronizeInvoke isi, string inputFileName, string outputFileName, TransformType type)
            : base(isi)
        {
            // populate private vars
            _inputFileName = inputFileName;
            _outputFileName = outputFileName;
            _type = type;
        }

        public AsyncTransformHTML(ISynchronizeInvoke isi, string inputFileName, string outputFileName, TransformType type, string customXSLT)
            : base(isi)
        {
            // populate private vars
            _inputFileName = inputFileName;
            _outputFileName = outputFileName;
            _type = type;
            _customXSLT = customXSLT;
        }

        /// <summary>
        /// To be overridden by the deriving class - this is where the work
        /// will be done.  The base class calls this method on a worker
        /// thread when the Start method is called.
        /// </summary>
        protected override void DoWork()
        {
            DoHTMLTransform();
            // When a cancel occurs, we'd better acknowledge cancellation.
            if (CancelRequested)
            {
                AcknowledgeCancel();
            }
        }

        private void DoHTMLTransform()
        {
            string transformType = _type == TransformType.Report ? XsltHelper.HTMLXSLT : (_type == TransformType.Standard ? XsltHelper.DEFAULTXSLT : _customXSLT);
            FileInfo fi = new FileInfo(_outputFileName);
            string outputFileName = _outputFileName;
            try
            {
                string xml = File.ReadAllText(_inputFileName);
                string html = XsltHelper.Transform(xml, transformType);
                if (fi.Exists)
                {
                    fi.Delete();
                }
                File.WriteAllText(outputFileName, html);
                outputFileName = fi.FullName;  
            }
            catch (Exception ex)
            {
                logger.Error(SQLSchemaTool.ERRORFORMAT, ex.Message, ex.Source, ex.StackTrace);
            }

            // if no cancellation was requested fire event delegate
            if (!CancelRequested)
            {
                OnCompleteTransformHTML(outputFileName);
            }
        }

        private void OnCompleteTransformHTML(string FileName)
        {
            lock (this)
            {
                if (CompleteTransformHTML != null)
                {
                    FireAsync(CompleteTransformHTML, FileName);
                }
            }
        }
    }
}
