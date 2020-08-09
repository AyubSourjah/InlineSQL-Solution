using System;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace InLineSQL
{
    /// <summary>
    /// Provides Access to Embedded SQL Statements for Various RBMS Providers
    /// </summary>
    public class SqlScript : IDisposable
    {
        private string _dbProvider;
        private string _dbParmSymbol;
        private string _dbScriptTag;
        private readonly XmlDocument _xmldoc = null;
        private readonly Assembly _callAssembly = null;
        private readonly Stream _xmlStream;
        private bool _disposedValue;

        public void Initialize()
        {
            _dbProvider = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["DefaultConnection"]].ProviderName;

            switch (_dbProvider)
            {
                case "System.Data.SqlClient":
                    this._dbParmSymbol = "@";
                    this._dbScriptTag = "mssql";
                    break;
                case "System.Data.OracleClient":
                    this._dbParmSymbol = ":";
                    this._dbScriptTag = "oracle";
                    break;
                case "IBM.Data.Informix":
                    this._dbParmSymbol = "@";
                    this._dbScriptTag = "ifx";
                    break;
                default:
                    throw new NotImplementedException("Database not implemented");
            }
        }
        public SqlScript(string resourceNamespace)
        {
            _callAssembly = System.Reflection.Assembly.GetCallingAssembly();
            _xmlStream = _callAssembly.GetManifestResourceStream(resourceNamespace);

            _xmldoc = new XmlDocument();
            _xmldoc.Load(_xmlStream);

            this.Initialize();
        }
        public SqlScript(string resourceNamespace, Type containingType)
        {
            _dbProvider = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["DefaultConnection"]].ProviderName;

            _callAssembly = containingType.Assembly;
            _xmlStream = _callAssembly.GetManifestResourceStream(resourceNamespace);

            _xmldoc = new XmlDocument();
            _xmldoc.Load(_xmlStream);

            this.Initialize();
        }
        public string GetSql(string scriptId)
        {
            XmlNodeList nodeList = _xmldoc.SelectNodes(string.Format(CultureInfo.InvariantCulture,
                "/sql/scripts[@id='{0}']", scriptId));

            if (nodeList.Count > 1)
                throw new InvalidOperationException("Duplicate script ID's found!");
            else if (nodeList.Count == 0)
                throw new InvalidOperationException("Script ID has not been defined!");

            string defaultSqlType = _xmldoc.SelectSingleNode("/sql").Attributes["default"].Value;
            string sql = string.Empty;

            if (this.HasSQLBlock(scriptId, _dbScriptTag, ref sql) == false)
            {
                foreach (XmlNode node in nodeList[0].ChildNodes)
                {
                    if (node.Name == _dbScriptTag)
                    {
                        //This code check for the cdata node within the current node
                        //if the cdate node is present load the value within it
                        //or else load the value within the current node
                        if (node.HasChildNodes)
                            sql = node.FirstChild.Value;
                        else sql = node.InnerText;

                        break;
                    }
                }

                if (String.IsNullOrEmpty(sql))
                {
                    XmlNode defaultSqlNode = _xmldoc.SelectSingleNode(
                        string.Format(CultureInfo.InvariantCulture,
                        "/descendant::{1}[ancestor::scripts/@id='{0}']", scriptId, defaultSqlType));

                    //This code check for the cdata node within the current node
                    //if the cdate node is present load the value within it
                    //or else load the value within the current node
                    if (defaultSqlNode.HasChildNodes)
                        sql = defaultSqlNode.FirstChild.Value;
                    else sql = defaultSqlNode.InnerText;
                }
            }

            //Format the sql parameter symbols to the connected database type symbol
            return this.Parse(sql);
        }
        public string GetSql(string scriptId, IsolationLevel mode)
        {
            StringBuilder sql = new StringBuilder();

            if (_dbProvider == "System.Data.SqlClient")
            {
                switch (mode)
                {
                    case IsolationLevel.ReadUncommitted:
                        sql.Append("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");
                        break;

                    default:
                        break;
                }
            }

            sql.Append(GetSql(scriptId));

            return sql.ToString();
        }
        private bool HasSQLBlock(string scriptID, string sqlType, ref string sqlOut)
        {
            bool hasBlock = false;

            XmlNode sqlBlockNode = _xmldoc.SelectSingleNode(
                string.Format(CultureInfo.InvariantCulture,
                "/descendant::sqlblock[ancestor::scripts/@id='{0}']", scriptID));

            if (sqlBlockNode != null)
            {
                StringBuilder sb = new StringBuilder();

                XmlNode commonNode = sqlBlockNode.SelectSingleNode(
                    string.Format(CultureInfo.InvariantCulture,
                    "/descendant::sqlblock[ancestor::scripts/@id='{0}']/common",
                    scriptID));
                XmlNode venderNode = sqlBlockNode.SelectSingleNode(
                    string.Format(CultureInfo.InvariantCulture,
                    "/descendant::sqlblock[ancestor::scripts/@id='{0}']/{1}",
                    scriptID, sqlType));

                //Check for cdata node with the current node and load
                //its value or else load the inntext of the current node
                if (commonNode.HasChildNodes)
                    sb.Append(commonNode.ChildNodes[0].Value);
                else sb.Append(commonNode.InnerText);

                sb.Append(" ");

                //Check for cdata node with the current node and load
                //its value or else load the inntext of the current node
                if (venderNode.HasChildNodes)
                    sb.Append(venderNode.FirstChild.Value);
                else sb.Append(venderNode.InnerText);

                sqlOut = sb.ToString();

                hasBlock = true;
            }

            return hasBlock;
        }
        public string Parse(string sqlCommandText)
        {
            StringBuilder converted = new StringBuilder();
            converted.Append(sqlCommandText);

            if (_dbProvider.Equals("System.Data.SqlClient", StringComparison.Ordinal) == false &&
                _dbProvider.Equals("IBM.Data.Informix", StringComparison.Ordinal) == false)
            {
                converted.Replace("@", _dbParmSymbol);
            }

            return converted.ToString();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _xmlStream.Close();
                    _xmlStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
