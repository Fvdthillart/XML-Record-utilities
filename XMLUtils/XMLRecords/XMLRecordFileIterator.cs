using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Core.XMLRecords
{
  /// <summary>
  /// Iterator implementation of XMLrecordfile
  /// </summary>
  public class XMLRecordFileIterator : IEnumerable<XMLRecord>
  {
    #region types, members and properties

    /// <summary>
    /// Filename of the XMLFile to be iterated over
    /// </summary>
    protected string _XMLFilename; // persistent representation

    /// <summary>
    /// Member to store the encoding of the XML
    /// </summary>
    protected string _encoding = "UTF-8";
    /// <summary>
    /// Property to set the encoding of the XML
    /// </summary>
    public string Encoding
    {
      get { return _encoding; }
      set { _encoding = value; }
    }
    /// <summary>
    /// Read only property that returns an XML declaration. Default value = "<![CDATA[<?xml version=\"1.0\" encoding=\"UTF-8\"?>]]>"
    /// </summary>
    public string XmlDecl
    {
      get { return String.Format("<?XML version=\"1.0\" encoding=\"{0}\"?>", Encoding); }
    }
    /// <summary>
    /// This array contain the names of the XML elements whose subtree will be the subtree of the XMLRecord. <br />
    /// For most applications, only one containertag is necessary but for anonymizing large files sometimes more than one containertag is
    /// required.
    /// </summary>
    protected List<String> _containerTags;
    /// <summary>
    /// This array contain the names of the XML elements whose values are concatenated to create a unique keyvalue for the XMLTree.
    /// </summary>
    protected List<String> _IDTags;
    /// <summary>
    /// Initial element of an XML file. Used to add to the begin and end of an XML Subtree in an XMLRecord to allow 
    /// the XML to be validated against the original xsd's and ready to be processed
    /// </summary>
    protected string _namespaceTag = "";
    /// <summary>
    /// Closing tag of initial element of an XML file. Used to add to the begin and end of an XML Subtree in an XMLRecord to allow 
    /// the XML to be validated against the original XSD's and ready to be processed
    /// </summary>
    protected string _endNamespaceTag = "";

    #endregion

    #region Constructors, factory method

    /// <summary>
    /// Constructor: Instantiates the class. Protected because of Factory method
    /// </summary>
    /// <param name="XMLFilename">the XMLfile that is to be iterated over</param>
    /// <param name="Containertags">The ContainerTag list contains all the elements whose subtree is extracted and stored in an XML Record</param>
    /// <param name="IDtags"></param>
    protected XMLRecordFileIterator(string XMLFilename, List<String> Containertags, List<String> IDtags)
    {
      _XMLFilename = XMLFilename;
      _containerTags = Containertags;
      _IDTags = IDtags;
    }

    //Factory method
    /// <summary>
    /// <para>Factory method to get an instance of this class. It tests against the following conditions before instantiating the class:</para>
    /// <list type="bullet">
    /// <item>if the filename exists</item>
    /// <item>if the list Containertags is not null and contains at least 1 item</item>
    /// <item>if the list IDtags is not null and contains at least 1 item</item>
    /// </list>
    /// </summary>
    /// <param name="filename">the XMLfile that is to be iterated over</param>
    /// <param name="Containertags"></param>
    /// <param name="IDtags"></param>
    /// <returns>an instance of XMLRecordFileIterator or null if the preconditions are not met</returns>
    /// <exception cref="ArgumentException">Thrown when the conditions for the parameters are not met</exception>
    /// 
    public static XMLRecordFileIterator GetXMLRecordFileIterator(string filename, List<String> Containertags, List<String> IDtags)
    {
      string XMLFile = null;
      if (File.Exists(filename)) 
        if (Containertags != null && IDtags != null)
          if (Containertags.Count > 0 && IDtags.Count > 0)
            XMLFile = filename;
          else
            throw new ArgumentException("Invalid argument: Either the list Containertags or IDtags has no elements ");
        else
          throw new ArgumentException("Invalid argument: Either the list Containertags or IDtags is null ");
      else
        throw new ArgumentException(String.Format("Invalid argument: File {0} doesn't exist!", filename));
      if (XMLFile != null)
        return new XMLRecordFileIterator(XMLFile, Containertags, IDtags);
      return null;
    }

    #endregion

    #region IEnumerable implementation

    /// <summary>
    /// Returns the attributes of an XML element. Needed for XMLReader to skip over attributes and fill member namespacetag
    /// </summary>
    /// <param name="reader">Instance of XMLReader</param>
    /// <param name="elementAttributeCount">count of the attributes</param>
    /// <returns>return string with all atributes</returns>
    protected string getAttributes(XmlTextReader reader, int elementAttributeCount)
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
    /// Pretty prints XML
    /// </summary>
    /// <param name="XML">XML to be be indented and with line endings</param>
    /// <returns>Formatted XML so humans can also read it</returns>
    public string PrintXML(string XML)
    {
      if (XML == null) return "";
      if (XML.Length == 0) return "";

      string Result = "";

      MemoryStream mStream = new MemoryStream();
      XmlTextWriter writer = new XmlTextWriter(mStream, System.Text.Encoding.UTF8);
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
    /// Enumeration method
    /// </summary>
    /// <returns>Yiels an XMLRecord object to the foreach loop</returns>
    public IEnumerator<XMLRecord> GetEnumerator()
    {
      using (XmlTextReader myReader = new XmlTextReader(_XMLFilename))
      {
        bool canRead = myReader.Read();
        //read token by token
        while (canRead)
        {
          //Haal de namespacetag op (de rootelement over het algemeen)
          string elementName = myReader.Name;
          string elementValue = myReader.Value;
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


            sb.Clear();
            sb.AppendFormat("</{0}>", elementName);
            _endNamespaceTag = sb.ToString();
            // Console.WriteLine(_namespaceTag);
            // Console.WriteLine(_endNamespaceTag);
          }

          //Critical piece of this function: the finding of each container tag
          if (myReader.NodeType == XmlNodeType.Element && _containerTags.IndexOf(elementName) >= 0 && myReader.IsStartElement())
          {
            //Read the XML subtree including the element identified by container tag
            //the XMLTextReader reader skips to the next element after the closing tag of the element identified by container tag
            // Add namespace elements and xmldeclaration to the XML record for validation
            string outerxml = myReader.ReadOuterXml();
            List<string> IDs = new List<string>();

            // Create the XmlDocument.
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(outerxml);

            //Look for the  identifier tags en store their  value in the IDs List
            foreach (string IDTag in _IDTags)
            {
              StringBuilder myID = new StringBuilder("");
              XmlNodeList elemList = doc.GetElementsByTagName(IDTag);
              //if more than 1 value is found, append all the found values
              if (elemList.Count > 0)
              {
                foreach (XmlNode myNode in elemList)
                {
                  XmlElement myElement = (myNode as XmlElement);
                  myID.AppendFormat("{0}_", myElement.InnerXml);
                }
                // Remove last _
                myID.Remove(myID.Length - 1, 1);
                IDs.Add(myID.ToString());
              }
            }

            //format the XML subtree into something readable and store it in an XML Record
            yield return new XMLRecord(IDs.ToArray(), PrintXML(doc.OuterXml), XmlDecl, _namespaceTag, _endNamespaceTag);

          }
          canRead = myReader.Read();
        }
      }
    }

    // Must also implement IEnumerable.GetEnumerator, but implement as a private method. 
    private System.Collections.IEnumerator GetEnumeratorNonGeneric()
    {
      return this.GetEnumerator();
    }
    //fully qualifying an interface method apparently makes this a non class method
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumeratorNonGeneric();
    }


    #endregion
  }
}
