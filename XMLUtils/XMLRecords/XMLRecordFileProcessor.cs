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
    public class XMLRecordFileProcessor
    {
        #region members
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
        /// postfix for the new filename when an XML file is anonymized 
        /// </summary>
        private const string anonymous = "_anonymous";
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
        protected List<string> _containerTags = new List<string>();
        /// <summary>
        /// This array contain the names of the XML elements whose values are concatenated to create a unique keyvalue for the XMLTree.
        /// </summary>
        protected List<string> _IDTags = new List<string>();
  
        /// <summary>
        /// List of the XMLrecords in an XMLfile if in memory processing is required
        /// </summary>
        protected List<XMLRecord> _XMLRecords;

        /// <summary>
        /// If a data element that needs to be anonymized is in the filename, this member stores the filename with the anonymized value
        /// </summary>
        private string _anonymized_filename="";

        #endregion

        #region properties and enums

        /// <summary>
        /// Exposes member <see cref="_XMLRecords"/>
        /// </summary>
        public List<XMLRecord> XMLRecordList
        {
          get { return _XMLRecords; }
          set { _XMLRecords = value; }
        }

        /// <summary>
        /// Returns an anonymized version of the XML filename. 
        /// </summary>
        /// <remarks>
        /// <para>Sometimes values to be anonymized are included in the filename. The resulting filename must 
        /// then also be anonymized as specified by the Anonymize rules </para>
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
            /// ToMemory indicates that the XMLRecords are stored in memory 
            /// </summary>
            ToMemory=1,
            /// <summary>
            /// ToFile indicates that the XML File will be split into smaller files where each files contains 1 XML record.
            /// </summary>
            ToFile,
            /// <summary>
            /// This later addition is an expansion on the ToFile processing method and uses the anonymizer class to 
            /// anonymize values indicated by rules in a separate file.
            /// </summary>
            AnonymizeToFiles,
            /// <summary>
            /// Anonymize indicates that the XML file will be anonymized and a copy of the original XML file with the anonymized data 
            /// will be stored in a new XML file with _anonymous.xml instead of .xml appended to the filename. If a filename contains a value 
            /// that should be anonymized, that will be anonymized if indicated by a filename anonymization rule.
            /// </summary>
            Anonymize
        };

        #endregion

        #region constructors
        /// <include file='generic_docs.xml' path='//XMLRecords/ConstructorText[@name="summary"]/summary'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="containerTag"]/param[@name="containerTag"]'/>
        public XMLRecordFileProcessor(string file, string containerTag)
        {
          string[] containers = new string[1];
          containers[0] = containerTag;
          common_constructor(file, containers);
        }

        /// <summary>
        /// Initializes the instance. All checking of parameters is done by the calling class so no checking is done inside the constructor.
        /// This constructor allows for more than one containertag to be passed
        /// </summary>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        /// <param name="containerTags">The tags that define the subtree of the XML record.
        /// The tags are meant to be an element directly under the root element of the file 
        /// but it can be any element since the processing will not break when this is the case.</param>
        public XMLRecordFileProcessor(string file, string[] containerTags)
        {
          common_constructor(file, containerTags);
        }
        /// <include file='generic_docs.xml' path='//XMLRecords/ConstructorText[@name="summary"]/summary'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="containerTag"]/param[@name="containerTag"]'/>
        /// <param name="identifyingTag">a comma seperated value list with all the identifying tags that identify the elements 
        /// whose value make up the ID part of the XML Record</param>
        public XMLRecordFileProcessor(string file, string containerTag, string identifyingTag)
        {
          string[] containers = new string[1];
          containers[0] = containerTag;
          common_constructor(file, containers);
          _IDTags.Add(identifyingTag);
        }

        /// <include file='generic_docs.xml' path='//XMLRecords/ConstructorText[@name="summary"]/summary'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="containerTag"]/param[@name="containerTag"]'/>
        /// <param name="IDTags">a string array with all the identifying tags that identify the elements whose value 
        /// make up the ID part of the XML Record</param>
        public XMLRecordFileProcessor(string file, string containerTag, string[] IDTags)
		    {	
          string[] containers = new string[1];
          containers[0] = containerTag;
			    common_constructor(file,containers);
          for (int i = 0; i < IDTags.Length; i++ )
            _IDTags.Add(IDTags[i]);
		    }

        /// <include file='generic_docs.xml' path='//XMLRecords/ConstructorText[@name="summary"]/summary'/>
        /// <include file='generic_docs.xml' path='//XMLRecords/Constructorparm[@name="file"]/param[@name="file"]'/>
        /// <param name="containerTags">The tags that define the root element of the subtree for the XML record. 
        /// The tags are meant to be an element directly under the root element of the original XML file but 
        /// it can be any element since the processing will not break when this is the case</param>
        protected void common_constructor(string file, string[] containerTags)
		    {	
			    processFilename(file);
          for (int i = 0; i < containerTags.Length; i++)
            _containerTags.Add(containerTags[i]);
			    curDir = Directory.GetCurrentDirectory();
		    }

        #endregion

        #region general functions
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
            List<string> IDValues = XMLRec.getIDList();


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
                XMLRec.Write(XMLFilename);
            else {
              XMLRec.MustBeValidatable = true;
              XMLRec.Write(XMLFilename);
            }

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
            foreach(string ID in _IDTags)
            {
                found |= element.Equals(ID, StringComparison.OrdinalIgnoreCase);
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
        /// Combines the 3 separate members for the filename, extension and path into a fully qualified filename
        /// </summary>
        /// <returns> the fully qualified filename</returns>
		    public string getFullFilename(){
			
			    StringBuilder sb = new StringBuilder(_filepath);
			    sb.AppendFormat("\\{0}.{1}", _filename, _fileext);
			    // Console.WriteLine(sb.ToString());
			    return sb.ToString();
		    }

        /// <summary>
        /// This function returns all the ID's in the XMLRecords list if filled by the function Process with processtype ToMemory
		    /// </summary>
		    /// <returns>A list of CSV strings containing the ID's of every XMLrecord in the list</returns>
		    public List<string> getIDList(){

          if (XMLRecordList == null)
            return null;
			    List<string> IDList = new List<string>();
			    foreach(XMLRecord XMLRec in XMLRecordList) {
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
          if (XMLRecordList == null)
            return -1;
			    XMLRecord searchID = new XMLRecord(ID);
			    return XMLRecordList.IndexOf(searchID);
		    }

        /// <summary>
        /// Searches the list of XMLRecords based on the passed ID
        /// </summary>
        /// <param name="ID">a CSV string with all the IDs of the XML record to be found</param>
        /// <returns>the xml subtree if found, null otherwise</returns>
		    public XMLRecord getXMLRecord(string ID) {
			    if (this.IndexOf(ID) >= 0)
				    return XMLRecordList[this.IndexOf(ID)];
			    return null;
		    }
		
        /// <summary>
        /// This function removes illegal characters for a filename from a string
        /// </summary>
        /// <param name="filename">the filename to be cleaned</param>
        /// <returns>the cleaned filename</returns>
        /// <remarks>
        /// <para>The function removes the characters <![CDATA[:,*,/,\,,,=,?,<,>,|, ]]> and replaces __ with _.</para>
        /// <para>If the filename is null X is returned</para>
        /// <para>If the filename is empty, 0 is returned</para>
        /// </remarks>
		    public static string cleanFilename(string filename) {
			    if (filename == null) return "X";
			    if (filename.Length <= 0 ) return "0";
			    string cleanName = filename.Replace(":", "_");
			    //remove low values from string
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
        /// Append anonymized text to workfile
        /// </summary>
        /// <param name="xmlText">The xml to be appended</param>
        private void AppendAnonymized(string xmlText)
        {
            using (StreamWriter anonymized = File.AppendText(tempfile))
                anonymized.WriteLine(xmlText);
        }

        ///<summary>
		    /// This function processes the given XML file according to the processing type provided. 
		    ///</summary>
        ///<param name="Type">Indicates how the XML file should be processed.see <see cref="XMLRecordFileProcessor.ProcessType"/></param>
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

          if (Type == ProcessType.ToMemory)
            _XMLRecords = new List<XMLRecord>();

	        //Iterate through the XMLfile
			    try {
            //create the namespace manager
            XmlNamespaceManager nsmgr = null;

            //Process XMLRecords
            XMLRecordFileIterator myXMLRecFile = XMLRecordFileIterator.GetXMLRecordFileIterator(this.getFullFilename()
                                                                                              , _containerTags
                                                                                              , _IDTags );
            //Add namespace tag (root element) to anonymized file
            if (Type == ProcessType.Anonymize)
              AppendAnonymized(myXMLRecFile.NamespaceTag);


            foreach (XMLRecord myRec in myXMLRecFile) {
              XmlDocument doc = new XmlDocument();
              myRec.MustBeValidatable = true;
              doc.LoadXml(myRec.getXMLRecord());

              if (anonymizer != null && nsmgr == null)
              {
                nsmgr = new XmlNamespaceManager(doc.NameTable);
                if (anonymizer.Namespaces.Count > 0)
                  foreach (string ns in anonymizer.Namespaces)
                  {
                    string[] fields = ns.Split(';');
                    nsmgr.AddNamespace(fields[0], fields[1]);
                    // Console.WriteLine("Naam namespace:'{0}' URL:\t {1}", fields[0], fields[1]);
                  }

              }

              //-------------------------------
              //Processtype specific coding
              //--------------------------------
              if((Type == ProcessType.AnonymizeToFiles || Type == ProcessType.Anonymize) && anonymizer != null)
                  //Anonymize the values in the elements to be anonymized
                  foreach (AnonymizeRule rule in anonymizer.AnonymizerRules)
                  {
  							    ExceptionMsg = "In file " + _filename + " Xpath that caused exception:\t" + rule.Location;
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

                        //In the code the filename is also anonymized.
                        //First this was implemented by recognizing if there was a value to be anonymized
                        // but this wasn't precise enough and to unwanted modifications in the filename
                        //By introducing a filename rule, an extra criteria, namely the  
                        //elementname with the value to be anonymized, could be added
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

        				  if(Type == ProcessType.ToMemory) XMLRecordList.Add(myRec);
                  if(Type == ProcessType.ToFile || Type == ProcessType.AnonymizeToFiles)
                    {
                        this.Write(myRec,Type);
                        //count of written files
                        i++;
                    }

                  if (Type == ProcessType.Anonymize)
                    {
                      myRec.MustBeValidatable = false;
                      string xmlText = myRec.getXMLRecord();
                      AppendAnonymized(xmlText);
                    }
              } // foreach

              //Processtype specific output
              if (Type == ProcessType.ToFile) 
                  Console.WriteLine("{0} bestanden met 1 XMLRecord aangemaakt uit bestand {1} met containertag {2}."
                                   , i, _filename, _containerTags[0]);

				      if (Type == ProcessType.AnonymizeToFiles) 
					      Console.WriteLine("{0} bestanden geanonimiseerd ({3} anonimiseer regels) en aangemaakt uit bestand {1} met containertag {2}."
                  , i, _filename, _containerTags[0], anonymizer.AnonymizerRules.Count);

              if (Type == ProcessType.Anonymize)
              {
                  AppendAnonymized(myXMLRecFile.EndNamespaceTag);
                  if (_anonymized_filename.Length == 0)
                      output_anonymized_filename = _filename;
                  else
                      output_anonymized_filename = _anonymized_filename;
                    
                  output_anonymized_filename = String.Concat(output_anonymized_filename, String.Concat(anonymous,postfix));
                  //Rename the tempfile to the anonymized filename
                  if (File.Exists(output_anonymized_filename))
                      File.Delete(output_anonymized_filename);
                  File.Move(tempfile, output_anonymized_filename);
                  Console.WriteLine("bestand {0} contains the anonymized data of {1}", output_anonymized_filename, _filename);
              }

              if (Type == ProcessType.ToMemory)
                  XMLRecordList.Sort();

			      } catch (Exception e) {
                
				      throw new XmlException(String.Concat(ExceptionMsg, "\r\n", e.ToString()));
			      }
        }
        #endregion
    }
}