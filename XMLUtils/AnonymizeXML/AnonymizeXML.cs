using System;
using System.Text;
using System.IO;
using Core.XMLRecords;
using System.Collections.Generic;

namespace Commandline.AnonymizeXML
{
    /// <summary>
    /// This class anonymizes an XML file according to the rules in Anonymize.txt and writes the result
    /// to a file with the orginal filename and "_Anonymous" appended to it
    /// This is a static class with a static main method to provide a commandline interface for the <see cref="Core.XMLRecords.Anonymization.AnonymizerXML"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class anonymizes an XML file according to the rules in Anonymize.txt and writes the result
    /// to a file with the orginal filename and "_Anonymous" appended to it
    /// This is a static class with a static main method to provide a commandline interface for the <see cref="Core.XMLRecords.Anonymization.AnonymizerXML"/> class.
    /// </para>
    /// <para></para>
    /// <para>
    /// It evaluates the commandline and translates the parameters to arguments for the constructor of the <see cref="Core.XMLRecords.Anonymization.AnonymizerXML"/> class</para>
    /// <para>Parameters are:</para>
    /// <list type="number">
    /// <item><term>&lt;any name&gt;.xml</term><description>Name of the file to be anonymized</description></item>
    /// <item><term>any XML element(s) without &lt; and &gt;</term><description>The element(s) that define(s) the subtree(s) to be anonymized</description></item>
    /// <para>The class can process 1 or more containertags.</para>
    /// </list>
    /// </remarks>
    /// <example>
  /// <para><strong>Example for a call to the executable:</strong></para>
  /// <code>AnonymizeXML.exe EP_NN-Transactiegegevens_B_M_2014070101011406.xml transaction</code>
  /// <para><strong>Example for a call to the executable with multiple tags:</strong></para>
  /// <code>AnonymizeXML.exe </code>
  /// </example>
    class AnonymizeXML
    {
        /// <summary>
        /// name of the file with the anonymization rules
        /// </summary>
        private const string anonymizeFile = "Anonymize.txt";

        /// <summary>
        /// Returns a help text to be used in Exception messages
        /// </summary>
        /// <returns></returns>
        private static string getHelpMessage()
        {
            StringBuilder sb = new StringBuilder("\r\nThis utility anonymizes an XML file.\r\nIt will create an anonymized copy of the orginal XML file \r\n");
            sb.Append("with _anonymous.xml appended to the filename of the original file\r\n\r\n");
            sb.Append("2 or parameters are expected:\r\n1 filename for the XML file to be anonymized and 1 or more container tags \r\n");
            sb.Append("A container tag is an XML element by which the XML element can be divided.\r\n\r\n");
            sb.AppendFormat("Also a file {0} is required wich contains the XML elements which need to be anonymized.\r\n", anonymizeFile);
            sb.AppendFormat("Each line in {0} consists of 2 parts, seperated by a ;.\r\n", anonymizeFile);
            sb.Append("The first part is (part of) the XML filename. This means\r\nthe file with the anonymization rules can be used for multiple files\r\n");
            sb.Append("so the anonymization rules can be stored in one file and flexibly applied to multiple XML files. ");
                sb.Append("The second part is the fully qualified xpath to the element that is to be anonymized.");
            return sb.ToString();
        }

        /// <summary>
        /// Main function that parses the arguments and creates an instance of <see cref="Core.XMLRecords.Anonymization.AnonymizerXML"/>
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static void Main(string[] args)
        {

            int rc = 0;
            try
            {
                //check if the anonymizer file exists
                if (!File.Exists(anonymizeFile))
                    throw new System.ArgumentException(getHelpMessage());

                //check if there are 2 parameters
                if (args.Length < 2)
                  throw new System.ArgumentException(getHelpMessage());
                //Add containertags
                List<String> containerTags = new List<string>();
                for (int i = 1; i < args.Length; i++)
                  containerTags.Add(args[i]);
                //Divide the XML files in XML records and write them to a directory with the same name as the XML file without the extension .xml
                XMLRecordFile myXMLFile = new XMLRecordFile(args[0], containerTags.ToArray());
                myXMLFile.Process(XMLRecordFile.ProcessType.Anonymize);
                rc = 0; //Success
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(getHelpMessage());
                rc = e.GetHashCode(); // alleen om warning weg te krijgen
                rc = 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                rc = 1; //Error
            }
            finally
            {

                // Keep the console window open in debug mode.
                Console.WriteLine("");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(rc);
            }

        }
    }
}