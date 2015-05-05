using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.OleDb;


namespace Core.Anonymization
{
    /// <summary>
    /// This class anonymizes a CSV file, based on a file with Anonymization rules
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most of its functionality is already described in the documentation for its ancestor <see cref="Core.Anonymization.AnonymizerWithRules"/> class.
    /// </para>
    /// </remarks>
    
    public class AnonymizerCSV : AnonymizerWithRules
    {
        /// <summary>
        /// string with characters that are whitespace
        /// </summary>
        private const string whitespace = "\t ";

        /// <summary>
        /// Column seperator in the CSV file
        /// </summary>
        private string _seperator = ";";
        /// <summary>
        /// Column seperator in the CSV file
        /// </summary>
        public string Seperator
        {
            get { return _seperator; }
            set { _seperator = value; }
        }

        /// <summary>
        /// Text qualifier that indicates that a text value starts or ends
        /// </summary>
        private string _textIndicator = "\"";
        /// <summary>
        /// Text qualifier that indicates that a text value starts or ends
        /// </summary>
        public string TextIndicator
        {
            get { return _textIndicator; }
            set { _textIndicator = value; }
        }

        /// <summary>
        /// Constructor with 5 parameters
        /// </summary>
        /// <param name="file_to_be_anonymized">CSV file to be anonymized</param>
        /// <param name="rulesFile">File that contains the anonymization rules. See below for the syntax of these rules</param>
        /// <param name="sep">Column seperator in the CSV file</param>
        /// <param name="textInd">Text qualifier that indicates that a text value starts or ends and that column seperators
        /// witin these text qualifiers should be ignored. Can be empty</param>
        public AnonymizerCSV(string file_to_be_anonymized, string rulesFile, string sep, string textInd)
        {
            base.common_constructor(file_to_be_anonymized);
            _anonymizerFile = rulesFile;
            _seperator = sep;
            _textIndicator = textInd;
            base.ProcessAnonymizerFile();
        }

        /// <summary>
        /// This function splits a line of comma seperated values according to the stored seperator
        /// and text indicator
        /// </summary>
        /// <param name="line">a text line that contains the comma separated values</param>
        /// <returns>the comma separated values as an array</returns>
        /// <remarks>
        /// <par>The processing algoritm of split is different.</par>
        /// <para>Instead of splitting on every occurrence of seperator, the splitter first checks if the seperator is 
        /// preceded by a starting text qualifier.
        /// If this occurs, it checks if the first non-whitespace after the next text qualifier is a seperator. 
        /// If so, it sees the value between the textqualifiers as a column.
        /// if not, it searches for the next text qualifier and does the same check.
        /// </para><para>
        /// This way constructions like "" or \" are avoided. Of course, combinations of seperator and textqualifier 
        /// can still pose problems but these can be solved by choosing an exotic qualifier 
        /// that isn't used in natural language
        /// </para>
        /// </remarks>
        public List<string> split(string line)
        {
            List<string> result = new List<string>();
            string value  = "";
            bool text = false;
            for (int i = 0; i < line.Length; i++) 
            {
                string token = line[i].ToString();

                if (token.Equals(_textIndicator))
                {
                    if (!text)
                        text = true;
                
                    //check if the textindicator is the end of the text by checking if the next non whitespace character is the seperator
                    //Zo is er geen moeilijk gedoe met backslashes of dubbele quotes
                    if (text)
                    {
                        int j = i + 1;
                        while (j < line.Length)
                        {
                            if (whitespace.IndexOf(line[j]) > 0)
                                j++;
                            else
                                break;
                        }
                        if (j < line.Length)
                            if (line[j].ToString().Equals(_seperator))
                                text = false;
                    }

                }

                if(token.Equals(_seperator) && !text) {
                    result.Add(value);
                    value = "";
                }
                else
                   value = String.Concat(value, token);

                if (i == line.Length - 1 && value.Length > 0)
                    result.Add(value);
            }
            return result;
        }
    }
}
