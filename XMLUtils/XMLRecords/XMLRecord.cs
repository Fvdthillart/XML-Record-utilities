using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Core.XMLRecords
{
    ///<summary>
    ///This class is a data structure for use in a generic List.
    ///To this end, interfaces IEquatable and IComparable are implemented so all the function of the List&lt;T&gt; class can be used
    ///</summary>
    ///<remarks>
    ///<para>
    /// An XMLRecord describes a part of an XML file that repeats like paragraphs in a document.
  /// An XMLrecord consists of the identifying part, a string array of ID's, and the XML subtree identified by a string called the containertag.
    ///</para><para>
    /// Basically what this class does is take the XML subtree of the XML node identified by container tag and store it in a string. Then it takes the values of all the XML nodes identified by the IDtags in the XML subtree and store these in a string array as the identifier. 
    /// and take the  for that ID and store it in an instance of XMLRecord that holds an ID and the XML that the ID refers to
    /// </para>
    ///</remarks>
    public class XMLRecord : IEquatable<XMLRecord>, IComparable<XMLRecord>
    {
        /// <summary>
        /// member to store the ID values of the XML Record
        /// </summary>
        protected List<string> _IDs = new List<string>();
        
        /// <summary>
        /// member to store the XML part of the XML record
        /// </summary>
        protected string _XMLRecord = "";
        
        /// <summary>
        /// member to store if just the XML part of the XMLPart is returned by <see cref="XMLRecord.getXMLRecord"/> 
        /// or if validatable XML is returned if members <see cref="_xmlDecl"/>, <see cref="NamespaceTag"/>
        /// , <see cref="EndNamespaceTag"/> aren't empty or null
        /// </summary>
        protected bool mustBeValidatable = false;
        
        /// <summary>
        /// property to expose member <see cref="mustBeValidatable"/>
        /// </summary>
        public bool MustBeValidatable
        {
          get { return mustBeValidatable; }
          set { mustBeValidatable = value; }
        }
        
        /// <summary>
        /// To communicate the separator that is used in the constructor that takes a string instead of a string array as argument for the identifying tags
        /// </summary>
		    public const char separator = ',';
        /// <summary>
        /// XML declaration. Default value = "<![CDATA[<?xml version=\"1.0\" encoding=\"UTF-8\"?>]]>"
        /// </summary>
        protected string _xmlDecl = "<?XML version=\"1.0\" encoding=\"UTF-8\"?>";
        /// <summary>
        /// Initial element of an XML file. Used to add to the begin and end of an XML Subtree in an XMLRecord to allow 
        /// the XML to be validated against the original xsd's and ready to be processed
        /// </summary>
        protected string _namespaceTag = "";

        /// <summary>
        /// Read only property to expose member <see cref="_namespaceTag"/>
        /// </summary>
        public string NamespaceTag
        {
          get { return _namespaceTag; }
        }
        /// <summary>
        /// Closing tag of initial element of an XML file. Used to add to the begin and end of an XML Subtree in an XMLRecord to allow 
        /// the XML to be validated against the original XSD's and ready to be processed
        /// </summary>
        protected string _endNamespaceTag = "";

        /// <summary>
        /// Read only property to expose member <see cref="_endNamespaceTag"/>
        /// </summary>
        public string EndNamespaceTag
        {
          get { return _endNamespaceTag; }
        }

        /// <summary>
        /// Default is false but if an XMLRecord is instantiated with only IDs, this is set to true as this only happens
        /// to support an indexof operation in a list. For examples, see class <see cref="XMLRecordFileProcessor"/>
        /// </summary>
        protected bool _searchOnly = false;

        /// <summary>
        /// read-only property to expose member <see cref="_searchOnly"/>
        /// </summary>
        public bool SearchOnly
        {
          get { return _searchOnly; }
        }

        /// <summary>
        /// Constructor: Creates an instance of the XML record class. Fills the members, required for <see cref="mustBeValidatable"/> 
        /// to have effect
        /// </summary>
        /// <param name="IDs">string array with the identifying values</param>
        /// <param name="XMLRecord">string with the XML subtree to be stored</param>
        /// <param name="XMLDecl">string with the XML Declaration and encoding</param>
        /// <param name="nsTag">Tag that contains the namespace attributes, usually the root element</param>
        /// <param name="nsEndTag">Endtag for nsTag</param>
        private XMLRecord(List<string> IDs, string XMLRecord, string XMLDecl, string nsTag, string nsEndTag)
        {
          _IDs=IDs;
          _XMLRecord = XMLRecord;
          _xmlDecl = XMLDecl;
          _namespaceTag = nsTag;
          _endNamespaceTag = nsEndTag;
        }

        /// <summary>
        /// Factory method for generating XMLRecord so the parameters can be tested  whether they are null or empty
        /// </summary>
        /// <param name="IDs">string array with the identifying values</param>
        /// <param name="XMLRecord">string with the XML subtree to be stored</param>
        /// <param name="XMLDecl">string with the XML Declaration and encoding</param>
        /// <param name="nsTag">Tag that contains the namespace attributes, usually the root element</param>
        /// <param name="nsEndTag">Endtag for nsTag</param>
        /// <returns>valid XMLRecord if the parameters are valid, else null</returns>
        public static XMLRecord XMLRecordFactory(List<string> IDs, string XMLRecord, string XMLDecl, string nsTag, string nsEndTag)
        {
          XMLRecord newRec = null;
          if (IDs == null)
            throw new ArgumentException();
          if(IDs.Count == 0)
            throw new ArgumentException("Parameter IDs is empty");
          if (String.IsNullOrEmpty(XMLRecord))
            throw new ArgumentException("Parameter XMLRecord is null or empty");
          if (String.IsNullOrEmpty(XMLDecl))
            throw new ArgumentException("Parameter XMLDecl is null or empty");
          if (String.IsNullOrEmpty(nsTag))
            throw new ArgumentException("Parameter nsTag is null or empty");
          if (String.IsNullOrEmpty(nsEndTag))
            throw new ArgumentException("Parameter nsEndTag is null or empty");

          newRec = new XMLRecord(IDs, XMLRecord, XMLDecl, nsTag, nsEndTag);
          return newRec;
        }

        /// <summary>
        /// Constructor for an xmlrecord with ids but an empty xml subtree. Used for use in an indexOf search
        /// </summary>
        /// <param name="ID">CSV string with the IDs of the new XML Tree</param>
        public XMLRecord(string ID)
        {
          _searchOnly = true;
          //id kan een comma seperated string zijn
          string[] IDs = ID.Split(separator);
          for (int i = 0; i < IDs.Length; i++)
            _IDs.Add(IDs[i]);
        }

        /// <summary>
        /// Makes a csv string of all ID values
        /// </summary>
        /// <returns>csv string with all IDs</returns>
        public string getID()
        {
			    StringBuilder sb = new StringBuilder("");
			    foreach(string ID in _IDs) 
              sb.AppendFormat("{0},", ID);
			    sb.Remove(sb.Length-1,1);
          return sb.ToString();
        }

		    /// <summary>
		    /// returns a string array with all the ID values
		    /// </summary>
		    /// <returns>string array with all ID values</returns>
		    public string[] getIDs() {
			    return _IDs.ToArray();
		    }

        /// <summary>
        /// returns a string list with all the ID values
        /// </summary>
        /// <returns>string array with all ID values</returns>
        public List<string> getIDList()
        {
          return _IDs;
        }

        /// <summary>
        /// Returns the XML subtree of the XML records. If mustbeValidatable = true, it prefixes the value of member _XMLRecord
        /// with an XMLdeclaration and the opening tag of the root element of the XMLfile containing the record. An endtag is appended.
        /// </summary>
        /// <returns>returns the XML subtree </returns>
        public string getXMLRecord()
        {
          if (_searchOnly)
            return "";
          if (mustBeValidatable && !String.IsNullOrEmpty(NamespaceTag))
            return String.Concat(_xmlDecl, "\r\n", NamespaceTag, "\r\n", _XMLRecord, "\r\n", EndNamespaceTag);
          else
            return _XMLRecord;
        }

        /// <summary>
        /// Makes a string representation of XMLRecords. Concatenates all ids and the XML subtree
        /// </summary>
        /// <returns>string representation with IDS and XML subtree seperated by :</returns>
        public override string ToString()
        {
          if (_searchOnly)
            return "";
          StringBuilder sb = new StringBuilder();
          sb.AppendFormat("ID {0}", _IDs[0]);
          foreach(string ID in _IDs) 
              sb.AppendFormat(".{0}", ID);
          sb.AppendFormat(":\r\n{1}", _XMLRecord);

          return sb.ToString();
        }

        /// <summary>
        /// Indicates if an object is equal to this XMLRecord
        /// </summary>
        /// <param name="obj">pointer to an object that has to be compared to this XMLRecord</param>
        /// <returns>true if equal, false if otherwise</returns>
        public override bool Equals(Object obj)
        {
 
          if (obj == null) 
            return false;
          XMLRecord objAsXMLRecord = obj as XMLRecord;
          if (objAsXMLRecord == null) 
            return false;
          else 
            return Equals(objAsXMLRecord);
        }

        /// <summary>
        /// <para>Indicates if the records are equal. ID values are compared.</para>
        /// <para>Required for implementation in a generic list</para>
        /// </summary>
        /// <param name="other">XMLRecord to compare to</param>
        /// <returns>true if equal, false if otherwise</returns>
        bool Equals(XMLRecord other)
        {
          if (other == null)
            return false;
          return (this.getID().Equals(other.getID()));
        }

        /// <summary>
        /// Explicit interface implementation for method <see cref="Equals(XMLRecord)"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool IEquatable<XMLRecord>.Equals(XMLRecord other)
        {
          return this.Equals(other);
        }

        /// <summary>
        /// <para>Generates a hashvalues, based on a csv string of all the IDs of the XMLRecord</para>
        /// <para>Required for implementation in a generic list</para>
        /// </summary>
        /// <returns>a hashcode</returns>
        public override int GetHashCode()
        {
            return getID().GetHashCode();
        }

        /// <summary>
        /// <para>Compares one XML record to another. The id values are compared</para>
        /// <para>Required for implementation in a generic list</para>
        /// </summary>
        /// <param name="other">CSV string that contains all the ID values of the other XML record</param>
        /// <returns>return 1 if greater, 0 if equal and -1 if smaller</returns>
        public int CompareTo(XMLRecord other)
        {
            // If other is not a valid object reference, this instance is greater. 
            if (other == null) 
              return 1;

            // The XMLRecord comparison or search depends on the ID of the XMLRecord
            
			      string[] otherIDs = other.getID().Split(XMLRecord.separator);
			      int rc = 0; //compareTo geeft dit terug als de waarden gelijk zijn
            for(int i = 0; i<_IDs.Count; i++) {
				
				      if( i < otherIDs.Length) {
                if (String.IsNullOrEmpty(_IDs.ToArray()[i])) 
                  rc = -1;
                else if (String.IsNullOrEmpty(otherIDs[i])) 
                  rc = 1;
                else
                  rc = _IDs[i].CompareTo(otherIDs[i]);
				      }
				
				      if (rc != 0) break; // beslissing is gemaakt
				
			      }
            return rc;
        }

        int IComparable<XMLRecord>.CompareTo(XMLRecord other)
        {
          return this.CompareTo(other);
        }

        /// <summary>
        /// Writes content to a file. Static because it's stateless
        /// </summary>
        /// <param name="filename">file to write to</param>
        /// <param name="content">content to be written. Usually the XML subtree of the XML record</param>
        public static void Write(string filename, string content)
        {
            try
            {
                using (StreamWriter writer = File.CreateText(filename))
                {
                    writer.Write(content);
                }
            }
            catch (Exception e)
            {
				        Console.WriteLine(filename);
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Writes the XML subtree of the XMLRecord to an XML file, preceded by a begin and end tag
        /// </summary>
        /// <param name="filename">file to write to</param>

        public void Write(string filename)
        {
          XMLRecord.Write(filename, this.getXMLRecord());
        }
    
        /// <summary>
        /// Builds a attribute_value string where each id tag is coupled to (a) value(s) for use in creating a filename
        /// </summary>
        /// <param name="IDTags">Tags that identify the XML elements that contain the ID values</param>
        /// <param name="IDs">Values of the XML elements identified by the IDTags</param>
        /// <returns>Returns a string where the identifier tag is coupled to the value of the element in the XMLRecord</returns>
        /// <remarks>
        /// This function is static so it can be used in situations when the IDs are already known and don't need to be retrieved from the XML record
        /// </remarks>
        public static string getIDTagString(String[] IDTags, String[] IDs)
        {
            StringBuilder sb = new StringBuilder("");
            for (int i = 0; i < IDTags.Length; i++)
            {
                if (IDs[i] != null)
                    if (IDs[i].Length > 0)
                        sb.AppendFormat("{0}={1}_", IDTags[i], IDs[i]);
            }
            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
            // Console.WriteLine(sb.ToString());
            return sb.ToString();
        }
        /// <summary>
        /// Overload of method <see cref="getIDTagString(string[],string[])"/> to provide support for <![CDATA[list<string>]]> type parameters
        /// </summary>
        /// <param name="IDTags"></param>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public static string getIDTagString(List<string> IDTags, List<string> IDs)
        {
          return XMLRecord.getIDTagString(IDTags.ToArray(), IDs.ToArray());
        }
    }
}