using System;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.XmlDiffPatch;
using System.Collections.Generic;
using Core.XMLRecords;

namespace Core.XMLCompare
{
    /// <summary>
    /// This class compares two Lists of XMLRecord instances using the <strong>XML Diff and Patch 1.0</strong> from Microsoft.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The results of the compare are logged in a file with the 
    /// The compare uses the libray from Microsoft to detect if there are changes.
    /// However, the Diff diagram it produces isn't usable for logging so the logging is limited
    /// </para>
    /// </remarks>
    class XMLCompare
    {
	private string _file1;
	private string _file2;
	private string _containerTag;
	private string[] _identifyingTags;

    private bool _perf = true;
	/// <summary>
	/// Creates new instance of this class
	/// </summary>
	/// <param name="file1">first file to be compared</param>
	/// <param name="file2">second file to be compared</param>
	/// <param name="containerTag">string that indicates the element on which both files will be split</param>
	/// <param name="identifyingTags">string that indicates the elements by which a unique identifier is created</param>
	public XMLCompare(string file1, string file2, string containerTag, string[] identifyingTags)
	{
		
		_file1 = file1;
		_file2 = file2;
		_containerTag = containerTag;
		_identifyingTags = identifyingTags;
		
		//rest of the command line arguments are assumed to be the identifying tags for the XML record
		// int i =0;
		// for(i = 2; i < args.Length; i++)
		// {
			// _tags[i-2] = args[i];
		// }
	}

    /// <summary>
    /// Translates the identifyingTags array to a string
    /// </summary>
    /// <returns>comma separated list of the names of the identifying tags</returns>
    private string getIDTags()
    {
        StringBuilder sb = new StringBuilder("");
        for (int i = 0; i < _identifyingTags.Length; i++) sb.AppendFormat("{0},", _identifyingTags[i]);
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

    /// <summary>
    /// Compares the two files stored in members _file1 and _file2
    /// </summary>
    /// <param name="filePrefix">Used for making the file with the comparison Results</param>
    /// <returns>the filename of the file with the comparison results</returns>
	  public string compare(string filePrefix)
	   {
			// Console.WriteLine(_file1);
			// Console.WriteLine(_file2);
			// Console.WriteLine(_containerTag);
			// Console.WriteLine(_identifyingTag);
			
			//Process the XML files and make XML recordlists of them
            DateTime start = DateTime.Now;
            DateTime startCompare = start;
			XMLRecordFile firstXMLFile = new XMLRecordFile(_file1, _containerTag, _identifyingTags);
			firstXMLFile.Process(XMLRecordFile.ProcessType.ToMemory);
            DateTime stop = DateTime.Now;
            TimeSpan duration = stop.Subtract(start);
            if (_perf) Console.WriteLine("Duration of processing file {0}: {1:g}", _file1, duration);

            start = DateTime.Now;
			XMLRecordFile secondXMLFile = new XMLRecordFile(_file2, _containerTag, _identifyingTags);
			secondXMLFile.Process(XMLRecordFile.ProcessType.ToMemory);
            stop = DateTime.Now;
            duration = stop.Subtract(start);
            if (_perf) Console.WriteLine("Duration of processing file {0}: {1:g}", _file2, duration);
			
			//Make a unique list of IDs from both files
            start = DateTime.Now;
			List<string> allIDs = new List<string>();
			
			foreach(string ID in firstXMLFile.getIDList()) {
				allIDs.Add(ID);
			}
			
			foreach(string ID in secondXMLFile.getIDList()) {
				if(allIDs.IndexOf(ID) < 0) allIDs.Add(ID);
			}
            stop = DateTime.Now;
            duration = stop.Subtract(start);
            if (_perf) Console.WriteLine("Duration of making a unique list of IDs: {0:g}", duration);

            start = DateTime.Now;
            allIDs.Sort();
            stop = DateTime.Now;
            duration = stop.Subtract(start);
            if (_perf) Console.WriteLine("Duration of sorting the unique list of IDs: {0:g}", duration);
			
			StringBuilder filename = new StringBuilder("");
			bool found1 = false;
			bool found2 = false;
			if (filePrefix.Length > 0) filename.AppendFormat("{0}_",filePrefix);
			filename.Append("Compare summary.txt");

            start = DateTime.Now;
            TimeSpan[] durations = new TimeSpan[5];
            //Initialiseer timespans
            for (int i=0; i < durations.Length ; i++) {
                durations[i] = new TimeSpan(0);
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename.ToString()))
			{
				foreach(string ID in allIDs) {
                    start = DateTime.Now;
                    found1 = (firstXMLFile.getIDList().IndexOf(ID) >= 0);
                    found2 = (secondXMLFile.getIDList().IndexOf(ID) >= 0);
                    stop = DateTime.Now;
                    durations[0] += stop.Subtract(start);

                    start = DateTime.Now;
                    StringBuilder msg = new StringBuilder("");
                    msg.AppendFormat("{0} => ", _containerTag);
                    string[] idTagnames = getIDTags().Split(',');
                    string[] idvalues = ID.Split(',');
                    for (int i = 0; i < idTagnames.Length; i++)
                    {
                        string tag = "";
                        string value = "";
                        if (i < idTagnames.Length) tag = idTagnames[i];
                        if (i < idvalues.Length) value = idvalues[i];
                        //clean values
                        if (tag == null) tag = "";
                        if (value == null) value = "";

                        //Neem waarde op als gevuld
                        if (value.Length > 0)
                            msg.AppendFormat("{0}:{1}\t", tag, value);
                    }
                        
					if(found1 && !found2) msg.AppendFormat("Only present in file 1:'{0}'", firstXMLFile.getFilename());
					if(!found1 && found2) msg.AppendFormat("Only present in file 2:'{0}'", secondXMLFile.getFilename());
                    stop = DateTime.Now;
                    durations[1] += stop.Subtract(start);


                    start = DateTime.Now;
                    if (found1 && found2)
                    {
						msg.AppendFormat("Present in file '{0}' and file '{1}' => ", firstXMLFile.getFilename(), secondXMLFile.getFilename());
						//Haal XMLRecords op
						XMLRecord XMLRecord1 = firstXMLFile.getXMLRecord(ID);
                        XmlDocument doc1 = new XmlDocument();
                        doc1.LoadXml(String.Concat(firstXMLFile.getNamespaceTag(), XMLRecord1.getXMLRecord(), firstXMLFile.getEndNamespaceTag()));
                        
                        XMLRecord XMLRecord2 = secondXMLFile.getXMLRecord(ID);
                        XmlDocument doc2 = new XmlDocument();
                        doc2.LoadXml(String.Concat(secondXMLFile.getNamespaceTag(), XMLRecord2.getXMLRecord(), secondXMLFile.getEndNamespaceTag()));


						bool equal  = false;
						// if (XMLRecord1 != null && XMLRecord2 != null) equal = XMLRecord1.getXMLRecord().Equals(XMLRecord2.getXMLRecord());
						// if (equal) msg.AppendFormat("Contents are identical (binary compare)");
						// else msg.AppendFormat("Contents are different (binary compare)");
						XmlDiff myDiff = new XmlDiff(XmlDiffOptions.IgnoreComments|XmlDiffOptions.IgnorePI|XmlDiffOptions.IgnoreXmlDecl|XmlDiffOptions.IgnorePrefixes|XmlDiffOptions.IgnoreNamespaces|
                            XmlDiffOptions.IgnoreChildOrder|XmlDiffOptions.IgnoreWhitespace|XmlDiffOptions.IgnoreDtd);
						myDiff.Algorithm = XmlDiffAlgorithm.Precise;
                        //XMLRecord1.Write("XML1.xml",firstXMLFile.getNamespaceTag(), firstXMLFile.getEndNamespaceTag());
                        //// XMLRecord1.Write("XML2.xml",firstXMLFile.getNamespaceTag(), firstXMLFile.getEndNamespaceTag());
                        //XMLRecord2.Write("XML2.xml",secondXMLFile.getNamespaceTag(), secondXMLFile.getEndNamespaceTag());
						
						try {
							if (XMLRecord1 != null && XMLRecord2 != null) equal = myDiff.Compare(doc1,doc2);
							if (equal) msg.AppendFormat(" Contents are identical (XML compare)");
							else msg.AppendFormat(" Contents are different (XML compare)");
						}
						catch (Exception e)
						{
							msg.Append(" XML compare failed.");
                            StringBuilder sb = new StringBuilder(getIDTags());
							sb.AppendFormat("{0}_file1_exception.xml", XMLRecord1.getID());
							string filename_tmp = XMLRecordFile.cleanFilename(sb.ToString());
							XMLRecord1.Write(filename_tmp,firstXMLFile.getNamespaceTag(), firstXMLFile.getEndNamespaceTag());
							msg.AppendFormat("\r\n{0} {1} opgeslagen in bestand {2}",_identifyingTags,XMLRecord1.getID(),filename_tmp);
							// Console.WriteLine(XMLRecordFile.cleanFilename(sb.ToString()));

							sb.Clear();
                            sb.Append(getIDTags());
							sb.AppendFormat("{0}_file2_exception.xml", XMLRecord2.getID());
							// Console.WriteLine(XMLRecordFile.cleanFilename(sb.ToString()));
							filename_tmp = XMLRecordFile.cleanFilename(sb.ToString());
							XMLRecord2.Write(filename_tmp,secondXMLFile.getNamespaceTag(), secondXMLFile.getEndNamespaceTag());
							msg.AppendFormat("\r\n{0} {1} opgeslagen in bestand {2}",_identifyingTags,XMLRecord2.getID(),filename_tmp);

							msg.AppendFormat("\r\nError message:\r\n{0}", e.ToString());
                            file.WriteLine(msg.ToString());

						}
					}
					file.WriteLine(msg.ToString());
                    stop = DateTime.Now;
                    durations[2] += stop.Subtract(start);

				}
			}
            //performance output
            if (_perf) Console.WriteLine("Duration of searching both lists of XMLRecords: {0:g}", durations[0]);
            if (_perf) Console.WriteLine("Message preparation and checking if IDS are in both lists: {0:g}", durations[1]);
            if (_perf) Console.WriteLine("Comparing of two files XML1.xml and XML2.xml: {0:g}", durations[2]);

            stop = DateTime.Now;
            duration = stop.Subtract(startCompare);
            Console.WriteLine("Duration of compare: {0:g}", duration);
			
			return filename.ToString();
	   }
	

    }
}