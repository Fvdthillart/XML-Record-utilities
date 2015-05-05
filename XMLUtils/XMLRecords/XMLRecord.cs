using System;
using System.Text;
using System.IO;

namespace Core.XMLRecords
{
    ///<summary>
    ///This class is a data structure for use in a generic List.
    ///To this end, interfaces IEquatable and IComparable are implemented so all the function of the List&lt;T&gt; class can be used
    ///</summary>
    ///<remarks>
    ///<para>
    /// Een XMLRecord is a new term and describes a part of an XML file that repeats like paragraphs in a document.
    /// An XMLrecord consists of a string array of ID's, the identifying part, and the XML subtree identified by the containertag.
    ///</para><para>
    /// Basically what this class does is take the XML subtree of the XML node identified by container tag and store it in a string. Then it takes the values of all the XML nodes identified by the IDtags in the XML subtree and store these in a string array as the identifier. 
    /// and take the  for that ID and store it in an instance of XMLRecord that holds an ID and the XML that the ID refers to
    /// </para>
    ///</remarks>
    public class XMLRecord : IEquatable<XMLRecord>, IComparable<XMLRecord>
    {

 
        private string[] _IDs;
        private string _XMLRecord;
        /// <summary>
        /// To communicate the seperator that is used in the constructor that takes a string instead of a string array as argument for the identifying tags
        /// </summary>
		public static char seperator = ',';
		

        /// <summary>
        /// Constructor: Creates an instance of the XML record class
        /// </summary>
        /// <param name="ID">CSV string with the identifying values</param>
        /// <param name="XMLRecord">string with the XML subtree to be stored</param>
        public XMLRecord(string ID, string XMLRecord)
        {
			_IDs = ID.Split(',');
            _XMLRecord = XMLRecord;
        }

        /// <summary>
        /// Constructor: Creates an instance of the XML record class
        /// </summary>
        /// <param name="IDs">string array with the identifying values</param>
        /// <param name="XMLRecord">string with the XML subtree to be stored</param>
        public XMLRecord(string[] IDs, string XMLRecord)
        {
            _IDs = IDs;
            _XMLRecord = XMLRecord;
        }

        /// <summary>
        /// Constructor for an xmlrecord with ids but an empty xml subtree
        /// </summary>
        /// <param name="ID">CSV string with the IDs of the new XML Tree</param>
        public XMLRecord(string ID)
        { //id kan een comma seperated string zijn
	
            _IDs = ID.Split(XMLRecord.seperator);
			
        }

        /// <summary>
        /// Constructor for an xmlrecord with ids but an empty xml subtree
        /// </summary>
        /// <param name="IDs">string array with the IDs of the new XML Tree</param>
        public XMLRecord(string[] IDs)
        {
            _IDs = IDs;
        }
		
        /// <summary>
        /// Makes a csv string of all ID values
        /// </summary>
        /// <returns>csv string with all IDs</returns>
        public string getID()
        {
			StringBuilder sb = new StringBuilder("");
			for (int i=0;i<_IDs.Length;i++) sb.AppendFormat("{0},", _IDs[i]);
			sb.Remove(sb.Length-1,1);
            return sb.ToString();
        }
		/// <summary>
		/// returns a string array with all the ID values
		/// </summary>
		/// <returns>string array with all ID values</returns>
		public string[] getIDs() {
			return _IDs;
		}

        /// <summary>
        /// Returns the XML subtree of the XML records
        /// </summary>
        /// <returns>returns the XML subtree </returns>
        public string getXMLRecord()
        {
            return _XMLRecord;
        }

        /// <summary>
        /// Makes a string representation of XMLRecords. Concatenates all ids and the XML subtree
        /// </summary>
        /// <returns>string representation with IDS and XML subtree seperated by :</returns>
        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("ID {0}", _IDs[0]);
            for (int i = 1; i < _IDs.Length; i++) sb.AppendFormat(".{0}", _IDs[i]);
            sb.AppendFormat(":\r\n{1}", _XMLRecord);

            return sb.ToString();
        }

        /// <summary>
        /// Indicates if an object is equal to this XMLRecord
        /// </summary>
        /// <param name="obj">pointer to an object that has to be compared to this XMLRecord</param>
        /// <returns>true if equal, false if otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            XMLRecord objAsXMLRecord = obj as XMLRecord;
            if (objAsXMLRecord == null) return false;
            else return Equals(objAsXMLRecord);
        }

        /// <summary>
        /// <para>Indicates if the records are equal. ID values are compared.</para>
        /// <para>Required for implementation in a generic list</para>
        /// </summary>
        /// <param name="other">XMLRecord to compare to</param>
        /// <returns>true if equal, false if otherwise</returns>
        public bool Equals(XMLRecord other)
        {
            if (other == null) return false;
            return (this.getID().Equals(other.getID()));
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
            if (other == null) return 1;

            // The XMLRecord comparison or search depends on the ID of the XMLRecord
            
			string[] otherIDs = other.getID().Split(XMLRecord.seperator);
			int rc = 0; //compareTo geeft dit terug als de waarden gelijk zijn

            if (_IDs.Length != otherIDs.Length) rc = _IDs.Length - otherIDs.Length;
			else for(int i = 0; i<_IDs.Length; i++) {
				
				if( i < otherIDs.Length) {
                    if (_IDs[i] == null) rc = -1;
                    else if (otherIDs[i] == null) rc = 1;
                    else
                        rc = _IDs[i].CompareTo(otherIDs[i]);
				}
				
				if (rc != 0) break; // beslissing is gemaakt
				
			}
            return rc;
            
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
        /// <param name="tag">Starting tag of the XML file. Usually the starting tag of the XML file the XML record was extracted from</param>
        /// <param name="endTag">End  tag of the XML file. Usually the starting tag of the XML file the XML record was extracted from</param>
        public void Write(string filename, string tag, string endTag)
        {
            StringBuilder sb = new StringBuilder(tag);
            sb.Append("\r\n");
            sb.AppendFormat("{0}", _XMLRecord);
            sb.AppendFormat("{0}\r\n", endTag);
            XMLRecord.Write(filename, sb.ToString());
        }
    
        /// <summary>
        /// Builds a attribute=value pair string where each id tag is coupled to a value for use in creating a filename
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
    }
}