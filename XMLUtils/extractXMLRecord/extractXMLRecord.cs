using System;
using Core.XMLRecords;

namespace Commandline.ExtractXMLRecord
{
    /// <summary>
    /// This class splits an XML file into the subtrees of an element called the containertag and writes each XML subtree to a directory 
    /// with the same name as the XML file to be split.<br/>
    /// The filenames in the directory are a concatenation of the names and values of elements, determined by socalled identifying tags which together have to uniquely identify the XMLTree otherwise files are overwritten.
    /// This is a static class with a static main method to provide a commandline interface for the <see cref="Core.XMLRecords.XMLRecordFileProcessor"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class splits an XML file into the subtrees of an element called the containertag and writes each XML subtree to a directory 
    /// with the same name as the XML file to be split.<br/>
    /// The filenames in the directory are a concatenation of the names and values of elements, determined by socalled identifying tags which together have to uniquely identify the XMLTree otherwise files are overwritten.
    /// This is a static class with a static main method to provide a commandline interface for the <see cref="Core.XMLRecords.XMLRecordFileProcessor"/> class.
    /// </para>
    /// <para></para>
    /// <para>
    /// It evaluates the commandline and translates the parameters to arguments for the constructor of the <see cref="Core.XMLRecords.XMLRecordFileProcessor"/> class</para>
    /// <para>Parameters are:</para>
    /// <list type="number">
    /// <item><term>&lt;any name&gt;.xml</term><description>Name of the XML file to be split</description></item>
    /// <item><term>any XML element without &lt; and &gt;</term><description>The element that defines the subtree to be anonymized</description></item>
    /// <item><term>any XML element without &lt; and &gt; in the subtree identified by the second parameter</term><description>The element(s) that define(s) a unique ID for the subtree</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>Example for a call to the executable:</strong></para>
    /// <code>extractXMLRecord.exe HYPKRO_1_7.20121231_TEST.xml hyp:Kredietovereenkomst hyp:Homes1Lennr hyp:House1HypothecaireLeningLeningNr hyp:DaybreakAccNbr hyp:VersieNr hyp:Quion1LeningContractNummer hyp:Quion1LeningNummer</code>
    /// </example>
    class extractXMLRecord
    {
        /// <summary>
        /// Main function that parses the arguments and creates an instance of <see cref="Core.XMLRecords.XMLRecordFileProcessor"/>
        /// </summary>
        /// <param name="args">command line arguments</param>
	   public static void Main(string[] args)
	   {
		   // The Length property is used to obtain the length of the array. 
		   // Notice that Length is a read-only property:
		   // Console.WriteLine("Number of command line parameters = {0}",
			  // args.Length);
           int rc = 0;
			try {
				//check if there are 3 parameters
				if(args.Length < 3) {
					throw new System.ArgumentException("3 or more parameters are expected. 1 XML filename, 1 container tag and identifying tags (1 or more)");
				}

                //Store starttime


                string[] IDTags = new string[args.Length-2];

                for (int i = 2; i < args.Length; i++)
                {
                    IDTags[i - 2] = args[i];
                }
				
				//Divide the XML files in XML records and write them to a directory with the same name as the XML file without the extension .xml
				XMLRecordFileProcessor myXMLFile = new XMLRecordFileProcessor(args[0], args[1], IDTags);
				myXMLFile.Process(XMLRecordFileProcessor.ProcessType.ToFile);
				rc = 0; //Success
			}
			catch (Exception e) {
			   Console.WriteLine(e);
			   rc = 1; //Error
			}
			finally {

				// Keep the console window open in debug mode.
                //Console.WriteLine("");
                //Console.WriteLine("Press any key to exit.");
                //Console.ReadKey();
                Environment.Exit(rc);
			}

        }
    }
}