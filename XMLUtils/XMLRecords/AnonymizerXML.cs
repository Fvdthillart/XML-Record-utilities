using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using Core.Anonymization;

namespace Core.XMLRecords.Anonymization
{

    /// <summary>
    /// <para>
    /// This class uses the functions from the AnonymizerWithRules class and expands them for XML files. It provides namespace handling so Xpath expressions can be resolved properly. 
    /// </para>
    /// <para>See remarks for guidance on using this class and the AnonymizerWithRules class.</para>
    /// </summary>
    /// <remarks>
    /// <para>To properly resolve XPath expressions in the location property of an AnonymizeRule instance, namespaces are required for resolution.</para>
    /// <para>Because the SelectNodes function of XMLdocument and XMLNode is very particular, some guidelines are provided:</para>
    /// <list type="bullet">
    /// <item><term>Default namespace</term><description>If only a default namespace is provided without a prefix, you MUST assign a prefix of your own choosing and prefix every element in your xpath with this prefix</description></item>
    /// <item><term>Prefixed namespaces</term><description>If the namespace has a prefix in the XML declaration, only the root element needs to be prefixed. All elements below do NOT require a prefix</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    ///<h1>Standard case: a prefixed namespace</h1>
    /// <para>Consider the XML below with a prefixed namespace giadd:</para>
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="UTF-8"?>
    /// <giadd:addressMessage xmlns:giadd="http://www.able.eu/epp/interfaces/generic/address" schemaBinding="1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.able.eu/epp/interfaces/generic/address">
    /// 	<party>
    /// 		<address xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:type="giadd:structuredAddress">
    /// 			<id>7545</id>
    /// 			<postalCode>1522AA</postalCode>
    /// 			<country>
    /// 				<id>1</id>
    /// 			</country>
    /// 			<usage>
    /// 				<id>1</id>
    /// 			</usage>
    /// 			<number>14</number>
    /// 			<street>OndernBGSgsweg</street>
    /// 		</address>
    /// 		<emailAddress>
    /// 			<address>testBGSble.eu</address>
    /// 		</emailAddress>
    /// 		<homePhone>
    /// 			<number>0297288547</number>
    /// 		</homePhone>
    /// 		<id>
    /// 			<number>4</number>
    /// 			<source>EOR</source>
    /// 		</id>
    /// 	</party>
    /// </giadd:addressMessage>
    /// ]]>
    /// </code>
    /// <para>To get to the street element you first need to declare the namespace above whit its prefix. In this example that's gi add</para>
    /// <code>
    /// XmlDocument doc = new XmlDocument();
    /// doc.LoadXml(outerxml); // contains the xml witht he declaration
    /// XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
    /// nsmgr.AddNamespace("giadd", "http://www.able.eu/epp/interfaces/generic/address");
    /// </code>
    /// <para> Then you have to specify the XPath and call selectnodes:</para>
    /// <code>
    /// string XPath = "//address/street"; //the full path would be /giadd:addressMessage/partyaddress/street
    /// XmlNodeList nodelist = doc.SelectNodes(XPath, nsmgr);
    /// </code>
    ///<h1>Counterintuitive case: a default namespace</h1>
    /// <para>Consider the following piece of XML with a default namespace:</para>
    /// <code>
    /// <![CDATA[
    /// <ExportDWHMessage xmlns="http://www.everest.nl/EMS/ExportDWH">
    /// <BatchSize>95</BatchSize> 
    /// <BatchSequenceNumber>32</BatchSequenceNumber> 
    /// <BatchID>todo</BatchID> 
    /// ]]>
    /// </code>
    /// <para>To get to the batchid element you first need to declare a prefix for the namespace above. In this example that's hs</para>
    /// <code>
    /// XmlDocument doc = new XmlDocument();
    /// doc.LoadXml(outerxml); // contains the xml witht he declaration
    /// XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
    /// nsmgr.AddNamespace("hs", "http://www.everest.nl/EMS/ExportDWH");
    /// </code>
    /// <para> Then you have to specify the XPath and call selectnodes:</para>
    /// <code>
    /// string XPath = "//hs:BatchID";
    /// XmlNodeList nodelist = doc.SelectNodes(XPath, nsmgr);
    /// </code>
    /// <para>Without the prefix this would return 0 elements even though the prefix appears nowhere in the xml</para>
    /// <para> </para>
    /// </example>
    public class AnonymizerXML : AnonymizerWithRules
    {
        /// <summary>
      /// string to recognize if an anonymizer rule applies to namespaces. Value="namespace:"
        /// </summary>
        private const string nsIndicator = "namespace:";

        /// <summary>
        /// See <see cref="Namespaces"/>
        /// </summary>
        private List<String> _namespaces;
        /// <summary>
        /// Returns a list of strings containing all the namespaces, named in the file with the anonymization rules
        /// </summary>
        public List<String> Namespaces
        {
            get { return _namespaces; }

        }

        /// <summary>
        /// Initializes the AnonymizerXML. Requires the file to be anonymized to determine which of the anonymization rules apply
        /// </summary>
        /// <param name="file_to_be_anonymized">the file which is going to be anonymized</param>
        public AnonymizerXML(string file_to_be_anonymized)
            : base(file_to_be_anonymized)
        {
            File_to_be_anonymized = file_to_be_anonymized;
            common_constructor(file_to_be_anonymized);
        }

        /// <summary>
        /// Initializes the AnonymizerXML. Requires the file to be anonymized to determine which of the anonymization rules apply
        /// </summary>
        /// <param name="file_to_be_anonymized">the file which is going to be anonymized</param>
        /// <param name="file_with_anonymizeRules">the file that contains the anonymization rules</param>
        public AnonymizerXML(string file_to_be_anonymized, string file_with_anonymizeRules)
            : base(file_to_be_anonymized, file_with_anonymizeRules)
        {
            _anonymizerFile = file_with_anonymizeRules;
            common_constructor(file_to_be_anonymized);
        }

        //protected functions
        /// <summary>
        /// function that's called in all constructors
        /// </summary>
        /// <param name="file_to_be_anonymized">the file which is going to be anonymized</param>
        protected override void common_constructor(string file_to_be_anonymized)
        {
            base.common_constructor(file_to_be_anonymized);
            _namespaces = new List<String>();
            ProcessAnonymizerFile();
           
        }

        /// <summary>
        /// This function is called from the processAnonymizerfile function and is a hook for derived classes to provide their own custom processing of the lines in the file with anonymization rules
        /// </summary>
        /// <param name="fields">the different values of a line in the file with anonymization rules. The values are seperated by comma's</param>
        protected override void ProcessRule(string[] fields)
        {
            if (fields[1].ToLower().IndexOf(nsIndicator.ToLower()) >= 0)
            {
                string ns = String.Concat(fields[1].Replace(nsIndicator, ""), ";", fields[2]);
                _namespaces.Add(ns);
                // Console.WriteLine("Namespace:\t{0}", ns);
            }
            else
            { //ancestor call
                base.ProcessRule(fields);
            }
        }


    }
}
