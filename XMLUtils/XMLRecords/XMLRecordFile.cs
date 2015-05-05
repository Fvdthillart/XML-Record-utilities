using System;
using System.Xml;
using System.Text;
using System.Management;
using System.IO;
using System.Collections.Generic;
using System.Threading; 
using System.Runtime.InteropServices;
using Core.Anonymization;
using Core.XMLRecords.Anonymization;

namespace Core.XMLRecords
{
    /// <summary>
    /// <para>
    /// This class divides an XMLfile in its constituent XML records.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// An XML record is an XML subtree whose root element is identified by a containertag and is uniqely identified in the source XML file by one or more identifier tags.
    /// </para>
    /// <para>
    /// It implements ways to process XML Records. see <c>XMLRecord.Processtype</c> for details
    /// </para>
    /// </remarks>    
    public class XMLRecordFile
    {
        /// <summary>
        /// name for temporary workfile. Value="temp.xml"
        /// </summary>
        private const string tempfile = "temp.xml";
        /// <summary>
        /// Postfix for filenames that would otherwise be longer than 259 characters. Value = "_(truncated)"
        /// </summary>
        private const string trunc = "_(truncated)";
        /// <summary>
        /// Postfix for xml files. Value = ".xml"
        /// </summary>
		    private const string postfix = ".xml";
        /// <summary>
        /// Constant to indicate the maximum length of a filename. Value = 259 because 260 gave an error
        /// </summary>
		    private const int maxFilenameLength = 259; // de 259 moet omdat er gestruikeld wordt over precies 260
        /// <summary>
        /// XML declaration. Value = "<![CDATA[<?xml version=\"1.0\" encoding=\"UTF-8\"?>]]>"
        /// </summary>
        private const string xmlDecl = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
		    /// <summary>
		    /// Stores the currentdirectory
		    /// </summary>
		    private string curDir = "";
        /// <summary>
        /// Stores the filename part of full filename
        /// </summary>
		    private string _filename;
        /// <summary>
        /// Stores the path part of full filename
        /// </summary>
        private string _filepath; //the directory the file resides in
        /// <summary>
        /// Stores the fileextension part of full filename
        /// </summary>
		    private string _fileext; //the extension of the file
        /// <summary>
        /// This array contain the names of the XML elements whose subtree will be the subtree of the XMLRecord. <br />
        /// For most applications, only one containertag is necessary but for anonymizeing large files sometimes more than one containertag is
        /// required.
        /// </summary>
		    private string[] _containerTags;
        /// <summary>
        /// This array contain the names of the XML elements whose values are concatenated to create a unique keyvalue for the XMLTree.
        /// </summary>
		    private string[] _IDTags;
        /// <summary>
        /// Intial element of an XML file. Used to add to the begin and end of an XML Subtree in an XMLRecord to allow 
        /// the XML to be validated and ready to be processed
        /// </summary>
		    private string _namespaceTag = "";
        /// <summary>
        /// Closing tag of initial element of an XML file. Used to add to the begin and end of an XML Subtree in an XMLRecord to allow 
        /// the XML to be validated and ready to be processed
        /// </summary>
		    private string _endNamespaceTag = "";

        /// <summary>
        /// List of the XMLrecords in an XMLfile if in memory processing is required
        /// </summary>
		    private List<XMLRecord> _XMLRecords;

        /// <summary>
        /// If a data element that needs to be anonymized is in the filename, this member stores the filename with the anonymized value
        /// </summary>
        private string _anonymized_filename="";
        /// <summary>
        /// Returns an anonymized version of the XML filename. 
        /// </summary>
        /// <remarks>
        /// <para>Sometimes values to be anonymized are included in the filename. The resulting filename must 
        /// then also be anonymized as specified by the Anonymize rules</para>
        /// </remarks>
        public string Anonymized_filename
        {
            get { return cleanFilename(_anonymized_filename); }
        }

		    /// <summary>
		    /// The ProcessType enumeration determines how an XMLfile is processed.
		    /// </summary>
        /// <remarks>
        /// </remarks>
    		public enum ProcessType : byte {
            /// <summary>
            /// ToMemory indicates that the XMLRecords are stored in memory and compared using the microsoft XML Diff tool.
            /// </summary>
            ToMemory=1,
            /// <summary>
            /// ToFile indicates that the XML File will be split into smaller files where each files contains 1 XML record.
            /// </summary>
            ToFile,
            /// <summary>
            /// This later addition is an expansion on the ToFile processing method and uses the anonymizer class to 
            /// anonymize values indicated by rules in a seperate file.
            /// </summary>
            AnonymizeToFiles,
            /// <summary>
            /// Anonymize indicates that the XML file will be anonymized and a copy of the original XML file with the anonymized data 
            /// will be stored in a new XML file with _anonymous.xml instead of .xml appended to the filename
            /// </summary>
            Anonymize
        };

		    //constructor

        /// <include file='generic_docs.xml' path='//XMLRecords/ConstructorText[@name="summary"]/summary'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="containerTag"]/param[@name="containerTag"]'/>
        public XMLRecordFile(string file, string containerTag)
        {
          string[] containers = new string[1];
          containers[0] = containerTag;
          common_constructor(file, containers);
          _IDTags = new string[0];
        }

        /// <summary>
        /// Initializes the instance. All checking of parameters is done by the calling class so no checking is done inside the constructor.
        /// This constructor allows for more than one containertag to be passed
        /// </summary>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        /// <param name="containerTags">The tags that define the subtrees of the XML record.
        /// The tahs are meant to be an element directly under the root element of the file 
        /// but it can be any element since the processing will not break when this is the case.</param>
        public XMLRecordFile(string file, string[] containerTags)
        {
          common_constructor(file, containerTags);
          _IDTags = new string[0];
        }
        /// <include file='generic_docs.xml' path='//XMLRecords/ConstructorText[@name="summary"]/summary'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="containerTag"]/param[@name="containerTag"]'/>
        /// <param name="identifyingTag">a comma seperated value list with all the identifying tags that identify the elements 
        /// whose value make up the ID part of the XML Record</param>
        public XMLRecordFile(string file, string containerTag, string identifyingTag)
        {
          string[] containers = new string[1];
          containers[0] = containerTag;
          common_constructor(file, containers);
          _IDTags = new string[1];
          _IDTags[0] = identifyingTag;
        }

        /// <include file='generic_docs.xml' path='//XMLRecords/ConstructorText[@name="summary"]/summary'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="containerTag"]/param[@name="containerTag"]'/>
        /// <param name="IDTags">a string array with all the identifying tags that identify the elements whose value 
        /// make up the ID part of the XML Record</param>
        public XMLRecordFile(string file, string containerTag, string[] IDTags)
		    {	
          string[] containers = new string[1];
          containers[0] = containerTag;
			    common_constructor(file,containers);
			    _IDTags = IDTags;
		    }

        /// <include file='generic_docs.xml' path='//XMLRecords/ConstructorText[@name="summary"]/summary'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        /// <param name="containerTags">The tags that define the root element of the subtree for the XML record. 
        /// The tags are meant to be an element directly under the root element of the original XML file but 
        /// it can be any element since the processing will not break when this is the case</param>
        protected void common_constructor(string file, string[] containerTags)
		    {	
			    processFilename(file);			
			    _containerTags = containerTags;
			    _XMLRecords = new List<XMLRecord>();
			    curDir = Directory.GetCurrentDirectory();
		    }
		
        /// <summary>
        /// This file takes the filename and splits it into a fixed path, filename and extension
        /// </summary>
        /// <remarks>
        /// The parameter for filename can be a single filename or a relative path or a fully qualified name. This function determines the fully 
        /// qualified filename and stores it in 3 members
        /// </remarks>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        protected void processFilename(string file)
        {

			    int pos = file.LastIndexOf("\\");
			    if( pos >= 0) {
				    _filepath = file.Substring(0,pos);
				    _filename = file.Substring(pos + 1);
			    } else {
				    _filepath = Directory.GetCurrentDirectory();
				    _filename = file;
			    }
			
			    pos = _filename.LastIndexOf(".");
			    if( pos >= 0) {
				    _fileext  = _filename.Substring(pos + 1);
				    _filename = _filename.Substring(0,pos);
			    } else {
				    _fileext = "";
			    }
		    }

        //private functions
        /// <summary>
        /// Returns the attributes of an XML element. Needed for XMLReader part
        /// </summary>
        /// <param name="reader">Instance of XMLReader</param>
        /// <param name="elementAttributeCount">count of the attributes</param>
        /// <returns>return string with all atrributes</returns>
        [Obsolete("Needed for addtoXMLrecord method which is deprecated")]
        private string getAttributes(XmlTextReader reader, int elementAttributeCount)
        {
            StringBuilder sb = new StringBuilder("");
            for (int t = 0; t < elementAttributeCount; t++)
            {
                reader.MoveToAttribute(t);
                string name = reader.Name;
                string value = reader.Value;
                sb.AppendFormat(" {0}=\"{1}\"", name, value);
            }
            reader.MoveToElement(); //zet de reader terug op het huidige element
            return sb.ToString();
        }

        /// <summary>
        /// Adding XML to XMLRecord. No longer needed
        /// </summary>
        /// <param name="reader">XMLreader on the XML file</param>
        /// <param name="XMLRecord">Text to be added to XMLrecord</param>
        /// <returns>Text to be added
        /// </returns>
        [Obsolete("XML subtree are now loaded in to XMLDocument instance")]
        private string addToXMLRecord(XmlTextReader reader, string XMLRecord)
        {
            StringBuilder sb = new StringBuilder(XMLRecord);
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    sb.AppendFormat("<{0}", reader.Name);
                    if (reader.HasAttributes) sb.AppendFormat(" {0}", getAttributes(reader, reader.AttributeCount));
                    if (reader.IsEmptyElement)
                    { //Dit kan bij een tag als <straatnaam />
                        sb.AppendFormat("/>\r\n");
                    }
                    else
                    {
                        sb.AppendFormat(">\r\n");
                    }
                    break;
                case XmlNodeType.Text:
                    sb.AppendFormat("{0}\r\n", ToXmlString(reader.Value));
                    break;
                case XmlNodeType.CDATA:
                    sb.AppendFormat("<![CDATA[{0}]]>\r\n", reader.Value);
                    break;
                case XmlNodeType.ProcessingInstruction:
                    sb.AppendFormat("<?{0} {1}?>\r\n", reader.Name, reader.Value);
                    break;
                case XmlNodeType.Comment:
                    sb.AppendFormat("<!--{0}-->\r\n", reader.Value);
                    break;
                case XmlNodeType.XmlDeclaration:
                    sb.AppendFormat("<?xml version='1.0'?>\r\n");
                    break;
                case XmlNodeType.Document:
                    break;
                case XmlNodeType.DocumentType:
                    sb.AppendFormat("<!DOCTYPE {0} [{1}]", reader.Name, reader.Value);
                    break;
                case XmlNodeType.EntityReference:
                    sb.AppendFormat(reader.Name);
                    break;
                case XmlNodeType.EndElement:
                    sb.AppendFormat("</{0}>\r\n", reader.Name);
                    break;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Creates a directory for which windows compression is activated
        /// </summary>
        /// <param name="dirname">Directory to be created</param>
        private void CreateCompressedDir(string dirname)
        {
            DirectoryInfo dir = new DirectoryInfo(dirname);

            if (!dir.Exists)
            {
                dir.Create();
                try
                {
                    // Enable compression for the output folder
                    // (this will save a ton of disk space)

                    string objPath = "Win32_Directory.Name=" + "'" + dir.FullName.Replace("\\", @"\\").TrimEnd('\\') + "'";

                    using (ManagementObject obj = new ManagementObject(objPath))
                    {
                        using (obj.InvokeMethod("Compress", null, null))
                        {
                            // I don't really care about the return value, 
                            // if we enabled it great but it can also be done manually
                            // if really needed
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("Cannot enable compression for folder '" + dir.FullName + "': " + ex.Message, "WMI");
                }
            }
        }

        /// <summary>
        /// Writes an XMLRecord based on the processtype
        /// </summary>
        /// <param name="XMLRec"></param>
        /// <param name="Type">Processtype. <see cref="ProcessType"/></param>
        /// <returns>the filename of the written file</returns>
        private string Write(XMLRecord XMLRec, ProcessType Type)
        {
            // Check if a folders exists with the same name as _filename. If not, create it

            if (!Directory.Exists(_filename)) CreateCompressedDir(_filename);

            //write XMLRecord to that directory. use 'cleaned' tags as filename.
            //'Clean' means that all characters that aren't allowed in a filename like \/:* are replaced with underscore
            string[] IDValues = XMLRec.getIDs();


            StringBuilder filename = new StringBuilder("");
            filename.AppendFormat("{0}\\{1}\\{2}_{3}", curDir, _filename, cleanFilename(_containerTags[0]), cleanFilename(XMLRecord.getIDTagString(_IDTags, IDValues)));
            // Console.WriteLine(filename.ToString());
            string XMLFilename = filename.ToString();
            if (XMLFilename.Length + postfix.Length > maxFilenameLength)
            {
                // Console.WriteLine(XMLFilename);
                XMLFilename = XMLFilename.Substring(0, maxFilenameLength - trunc.Length - postfix.Length - 1);
                XMLFilename = String.Concat(XMLFilename, trunc);
            }
            XMLFilename = String.Concat(XMLFilename, postfix);
            if (XMLFilename.Substring(0, 1) == "_") Console.WriteLine(XMLFilename);

            if (Type == ProcessType.AnonymizeToFiles)
                XMLRecord.Write(XMLFilename, XMLRec.getXMLRecord());
            else
                XMLRec.Write(XMLFilename, _namespaceTag, _endNamespaceTag);

            return XMLFilename;
        }
        
        /// <summary>
        /// Determines if the booleans in  boolean array are all true
        /// </summary>
        /// <param name="test">array to be tested</param>
        /// <returns>returns true if all elements are true</returns>
        private bool AllTrue(bool[] test)
        {
            bool allTrue = true;
            for (int i = 0; i < test.Length; i++)
            {
                allTrue &= test[i];
            }

            return allTrue;
        }

        /// <summary>
        /// Determines if an element is an element whose value is needed for the unique identifier of the XML Record
        /// </summary>
        /// <param name="element">element that might be an identifying element</param>
        /// <returns>true if element is an IDTag</returns>
        private bool IsIDTag(string element)
        {
            bool found = false;
            for (int i = 0; i < _IDTags.Length; i++)
            {
                found |= element.Equals(_IDTags[i], StringComparison.OrdinalIgnoreCase);
            }
            return found;
        }
        
        //public

    /// <summary>
		/// Returns the filename without the path
		/// </summary>
		/// <returns> :the filename + extension but not the full path</returns>
		public string getFilename(){
			
			StringBuilder sb = new StringBuilder(_filename);
			sb.AppendFormat(".{0}", _fileext);
			// Console.WriteLine(sb.ToString());
			return sb.ToString();
		}
		
    /// <summary>
    /// Combines the 3 seperate members for the filename, extension and path into a fully qualified filename
    /// </summary>
    /// <returns> the fully qualified filename</returns>
		public string getFullFilename(){
			
			StringBuilder sb = new StringBuilder(_filepath);
			sb.AppendFormat("\\{0}.{1}", _filename, _fileext);
			// Console.WriteLine(sb.ToString());
			return sb.ToString();
		}
		
        /// <summary>
        /// This function replaces special characters with the HTML encoded value.
        /// </summary>
        /// <remarks>
        /// <para>This encoding prevents processing errors  due to special characters</para>
        /// <para>The following lists show the translations</para>
        /// <list type="number">
        /// <listheader>
        /// <description>Encoded value</description>
        /// </listheader>
        /// <item><term><![CDATA[ & ]]></term>
        /// <description><![CDATA[ &amp; ]]></description></item>
        /// <item><term><![CDATA[ " ]]></term>
        /// <description><![CDATA[ &quot; ]]></description></item>
        /// <item><term><![CDATA[ < ]]></term>
        /// <description><![CDATA[ &lt; ]]></description></item>
        /// <item><term><![CDATA[ > ]]></term>
        /// <description><![CDATA[ &gt; ]]></description></item>
        /// </list>
        /// <para>There are functions in the .Net libraries that do this. I didn't find them when authoring this code.</para>
        /// </remarks>
        /// <param name="aString">The string to be encoded</param>
        /// <returns>An string containing encoded special characters</returns>
    [Obsolete("Used in obsoleted method")]
		public string ToXmlString(string aString)
		{
			if (aString == null) return "";
			StringBuilder myResult= new StringBuilder("");
			foreach( char myChar in aString) {
				switch (myChar) {
					case '&':
						myResult.Append("&amp;");
						break;
					case '\"':
						myResult.Append("&quot;");
						break;
					case '<':
						myResult.Append("&lt;");
						break;
					case '>':
						myResult.Append("&gt;");
						break;
					default:
						myResult.Append(myChar);
						break;
				}
			}
			return myResult.ToString();
		}
		
		
		/// <summary>
        /// This function returns all the ID's in the XMLRecords list if filled by the function Process and processtype ToMemory
		/// </summary>
		/// <returns>A list of CSV strings containing the ID's of every XMLrecord in the list</returns>
		public List<string> getIDList(){
		
			List<string> IDList = new List<string>();
			foreach(XMLRecord XMLRec in _XMLRecords) {
				IDList.Add(XMLRec.getID());
                //Console.WriteLine(XMLRec.getID());
				
			}
			
			return IDList;
		}
		
    /// <summary>
    /// Searches the list of XMLRecords based on the passed ID
    /// </summary>
    /// <param name="ID">a CSV string with all the IDs of the XML record to be found</param>
    /// <returns>the index of the XMLTree if found else -1</returns>
    public int IndexOf(string ID) {
			XMLRecord searchID = new XMLRecord(ID, "");
			return _XMLRecords.IndexOf(searchID);
		}
        /// <summary>
        /// Searches the list of XMLRecords based on the passed ID
        /// </summary>
        /// <param name="ID">a CSV string with all the IDs of the XML record to be found</param>
        /// <returns>the xml subtree if found, null otherwise</returns>
		public XMLRecord getXMLRecord(string ID) {
			if (this.IndexOf(ID) >= 0)
				return _XMLRecords[this.IndexOf(ID)];
			return null;
		}
		
    /// <summary>
    /// This function removes illegal characters for a filename from a string
    /// </summary>
    /// <param name="filename">the filename to be cleaned</param>
    /// <returns>the cleaned filename</returns>
    /// <remarks>
    /// <para>The function removes the characters <![CDATA[:,*,/,\,,,=,?,<,>,|, ]]> and replaces __ with _.</para>
    /// <para>If the fuilename is null X is returned</para>
    /// <para>If the filename is empty, 0 is returned</para>
    /// </remarks>
		public static string cleanFilename(string filename) {
			if (filename == null) return "X";
			if (filename.Length <= 0 ) return "0";
			string cleanName = filename.Replace(":", "_");
			//verwijderen control karakters
			for (int i = 0; i< 32;i++)
				cleanName = cleanName.Replace(Convert.ToChar(i).ToString(), "");
			cleanName = cleanName.Replace("*", "_");
			cleanName = cleanName.Replace("/", "_");
			cleanName = cleanName.Replace("\\", "_");
			cleanName = cleanName.Replace(",", "_");
			cleanName = cleanName.Replace("=", "_");
			cleanName = cleanName.Replace("?", "");
			cleanName = cleanName.Replace("<", "_");
			cleanName = cleanName.Replace(">", "_");
			cleanName = cleanName.Replace("|", "_");
			cleanName = cleanName.Replace(" ", "_");
			cleanName = cleanName.Replace("__", "_");
			return cleanName;
		}


	    /// <summary>
	    /// returns the starting tag of the XMLRecordfile
	    /// </summary>
	    /// <returns>starting tag</returns>
		public string getNamespaceTag() {
			return _namespaceTag;
		}

        /// <summary>
 	    /// returns the starting tag of the XMLRecordfile
        /// </summary>
        /// <returns>end tag</returns>
		public string getEndNamespaceTag() {
			return _endNamespaceTag;
		}

        /// <summary>
        /// Pretty prints XML
        /// </summary>
        /// <param name="XML">XML to be be indented and with line endings</param>
        /// <returns>pretty XML</returns>
        public string PrintXML(string XML)
        {
            if (XML == null) return "";
			if (XML.Length == 0) return "";
			
			string Result = "";
			
            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(XML);

                writer.Formatting = Formatting.Indented;
                

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                String FormattedXML = sReader.ReadToEnd();

                Result = FormattedXML;
            }
            catch (XmlException)
            {
            }
            writer.Close();

            return Result;
        }
        
        /// <summary>
        /// Append anonymized text to workfile
        /// </summary>
        /// <param name="xmlText">The xml to be appended</param>
        private void AppendAnonymized(string xmlText)
        {
            using (StreamWriter anonymized = File.AppendText(tempfile))
                anonymized.WriteLine(xmlText);
        }
        
        /// <summary>
        /// This function returns true when value is in the array
        /// </summary>
        /// <param name="list">The array to be searched</param>
        /// <param name="value">The value to be found</param>
        /// <returns></returns>
        private bool In(string[] list, string value){

          bool result = false;
          int pos = -1;
          int lbound = list.GetLowerBound(0);
          int ubound = list.GetUpperBound(0);

          for (int i = lbound; i <= ubound; i++)
          {
            if (list[i].Equals(value, StringComparison.OrdinalIgnoreCase))
            {
              pos = i;
              break;
            }
          }
          result = (pos >= 0);
          return result;
        }
		
		    ///<summary>
		    /// This function processes the given XML file according to the processing type provided. 
		    /// An XMLRecord consists of an ID and a string containing the XML Record, a part of the original file
        ///</summary>
        ///<param name="Type">Indicates how the XML file should be processed.<see cref="XMLRecordFile.ProcessType"/></param>
        public void Process(ProcessType Type)
    		{
		      	int i = 0;
            string ExceptionMsg = ""; //used by the exception thrown if something goes wrong
            AnonymizerXML anonymizer = null;
            string output_anonymized_filename = "";

            if (Type == ProcessType.AnonymizeToFiles || Type == ProcessType.Anonymize)
                anonymizer = new AnonymizerXML(_filename);

            if (Type == ProcessType.Anonymize)
            {
               using( StreamWriter anonymized = File.CreateText(tempfile))
                   anonymized.WriteLine(xmlDecl);
            }
			
	        //Initialize the token by token parser. Strictly IO, no in memory
			    XmlTextReader myReader = new XmlTextReader(this.getFullFilename());
			    try {
            bool canRead = myReader.Read();
            //read token by token
				    while (canRead) {
					    //Haal de namespacetag op (de roottag over het algemeen)
					    string elementName = myReader.Name;
					    string elementValue= myReader.Value;
					    XmlNodeType Nodetype = myReader.NodeType; //zie de documentatie voor nodetypes
					    int elementAttributeCount = myReader.AttributeCount;

              if (myReader.NodeType == XmlNodeType.Element && _namespaceTag.Length == 0 && myReader.HasAttributes)
                {

                    StringBuilder sb = new StringBuilder("<");
                    sb.AppendFormat("{0}", elementName);
                    // if(myReader.NamespaceURI.Length > 0) Console.WriteLine("Namespace {0}:{1}={2},{3}",elementName,myReader.NamespaceURI,elementValue, elementAttributeCount);

                    //get all the attributes. myReader will read the attributes and skip to the next node
                    sb.Append(getAttributes(myReader, elementAttributeCount));
                    sb.AppendFormat(">");
                    _namespaceTag = sb.ToString();
                    //If anonymization is active write the namespace tag
                    if (Type == ProcessType.Anonymize )
                        AppendAnonymized(_namespaceTag);

                    sb.Clear();
                    sb.AppendFormat("</{0}>", elementName);
                    _endNamespaceTag = sb.ToString();
                    // Console.WriteLine(_namespaceTag);
                    // Console.WriteLine(_endNamespaceTag);
                }

                //Critical piece of this function: the finding of each containertag
                if (myReader.NodeType == XmlNodeType.Element && In(_containerTags,elementName) && myReader.IsStartElement())
                {   
                    //Read the XML subtree including the element identified by containertag
                    //the node by node reader skips to the next element after the closing tag of the element identoified by container tag
						        string outerxml = myReader.ReadOuterXml();
                    string[] IDs = new string[_IDTags.Length];

                    // Create the XmlDocument.
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(outerxml);

                    //Zoek de identifidier tags en stel de ID's samen
                    for (int c = 0; c < _IDTags.Length; c++) {
                      StringBuilder myID = new StringBuilder("");
                      XmlNodeList elemList = doc.GetElementsByTagName(_IDTags[c]);
                        //if more than 1 value is found, append all the found values
                      if (elemList.Count > 0)
                      {
                          for (int t = 0; t < elemList.Count; t++)
                          {
                              myID.AppendFormat("{0}_", elemList[t].InnerXml);
                          }
                          // Remove last _
                          myID.Remove(myID.Length - 1, 1);
                          IDs[c] = myID.ToString();
                      }
                    }

                //-------------------------------
                //Processtype specific coding
                //--------------------------------
                if((Type == ProcessType.AnonymizeToFiles || Type == ProcessType.Anonymize) && anonymizer != null)
                    //Anonimiseer de waarden in de te anonimiseren elementen
                    foreach (AnonymizeRule rule in anonymizer.AnonymizerRules)
                    {
								      XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
								      if (anonymizer.Namespaces.Count > 0 )
                        foreach (string ns in anonymizer.Namespaces)
                        {
									        string[] fields = ns.Split(';');
									        nsmgr.AddNamespace(fields[0], fields[1]);
									        // Console.WriteLine("Naam namespace:'{0}' URL:\t {1}", fields[0], fields[1]);
								        }
								
								        ExceptionMsg = "In file " + _filename + " Xpath that caused error:\t" + rule.Location;
								        XmlNodeList nodelist = null;
                        if (anonymizer.Namespaces.Count > 0)
                        {
									        nodelist = doc.SelectNodes(rule.Location, nsmgr);
								        } else {
									        nodelist = doc.SelectNodes(rule.Location);
								        }

                        foreach (XmlNode myNode in nodelist)
                        {
                            string nodeValue = myNode.InnerXml;
                            //Some anonymizations are filtered. They require handling by a different function
                            if(rule.IsFiltered) 
                            {
                                XmlNode filterNode = null;
                                if (anonymizer.Namespaces.Count > 0)
                                {
									                filterNode = doc.SelectSingleNode(rule.FilterLocation, nsmgr);
								                } else {
									                filterNode = doc.SelectSingleNode(rule.FilterLocation);
								                }
                                myNode.InnerXml = anonymizer.ProcessFilter(rule, nodeValue, filterNode.InnerXml);
                            }
                            else
                                myNode.InnerXml = anonymizer.Anonymize(nodeValue, rule.Method);

                            //if a filename contains the value of an element, it must be anonymized too
                            if (_anonymized_filename.Length == 0) _anonymized_filename = _filename;

                            //In onderstaande regels wordt de bestandsnaam ook geanonimiseerd.
                            //Alleen een geanonimiseerde waarde vervangen was niet scherp genoeg en 
                            //leidde tot ongewenste aanpassing aan de bestandsnaam
                            //Door een filenameRule te introduceren kon er een extra criterium namelijk de 
                            //elementnaam met de te anonimiseren waarde worden toegevoegd
                            bool found = false;
                            foreach (String filenameRule in anonymizer.AnonymizerRulesFilename)
                            {
                                if (filenameRule.Equals(myNode.Name))
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (_filename.IndexOf(nodeValue) > 0 && found) 
                                _anonymized_filename = _anonymized_filename.Replace(nodeValue, myNode.InnerXml);
                        }
          					 }

                    //format the XML subtree into something readable and store it in an XML Record
                    XMLRecord myRec = new XMLRecord(IDs, PrintXML(doc.OuterXml));
                    //Console.WriteLine(myRec.getID());

                    // Procestype afhankelijk verwerken van een wel of niet geanonimiseerd XMLRecord

        						if(Type == ProcessType.ToMemory) _XMLRecords.Add(myRec);
                    if(Type == ProcessType.ToFile || Type == ProcessType.AnonymizeToFiles)
                      {
                          this.Write(myRec,Type);
                          //count of written files
                          i++;
                      }

                    if (Type == ProcessType.Anonymize)
                      {
                          string xmlText = myRec.getXMLRecord();
                          xmlText = xmlText.Replace(_namespaceTag, "");
                          xmlText = xmlText.Replace(_endNamespaceTag, "");

                          AppendAnonymized(xmlText);
                      }

                        // loop house keeping
                        canRead = (outerxml != string.Empty);
                    }
                    else
                    {
                        canRead = myReader.Read();
                    }

				 }

                //Processtype specific output
                if (Type == ProcessType.ToFile) 
                    Console.WriteLine("{0} bestanden met 1 XMLRecord aangemaakt uit bestand {1} met containertag {2}.", i, _filename, _containerTags[0]);

				if (Type == ProcessType.AnonymizeToFiles) 
					Console.WriteLine("{0} bestanden geanonimiseerd ({3} anonimiseer regels) en aangemaakt uit bestand {1} met containertag {2}.", i, _filename, _containerTags[0], anonymizer.AnonymizerRules.Count);

                //if ((Type == ProcessType.ToFile || Type == ProcessType.AnonymizeToFiles)) 
                //{
                //    Console.WriteLine("Root element begin tag:\r\n{0}", _namespaceTag);
                //    Console.WriteLine("Root element eind tag:\r\n{0}", _endNamespaceTag);
                //}

                if (Type == ProcessType.Anonymize)
                {

                    AppendAnonymized(_endNamespaceTag);
                    if (_anonymized_filename.Length == 0)
                        output_anonymized_filename = _filename;
                    else
                        output_anonymized_filename = _anonymized_filename;
                    
                    output_anonymized_filename = String.Concat(output_anonymized_filename, "_anonymous.xml");
                    //Rename de tempfile naar de geanonimiseerde file
                    if (File.Exists(output_anonymized_filename))
                        File.Delete(output_anonymized_filename);
                    File.Move(tempfile, output_anonymized_filename);
                    Console.WriteLine("bestand {0} bevat de geanonimiseerde gegevens van {1}", output_anonymized_filename, _filename);
                }

                //if (Type == ProcessType.ToMemory)
                //    _XMLRecords.Sort();

			} catch (Exception e) {
                
				throw new XmlException(String.Concat(ExceptionMsg, "\r\n", e.ToString()));
			} finally {
				if (myReader != null) myReader.Close();
			}
		}
	}
}