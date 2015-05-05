using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Core.Anonymization
{
    /// <summary>
    /// The anonymizer class is used to replace meaningful values with meaningless values.
    /// Anonymizing is done to cloak personal information like names, account number and anything that can be used
    /// to identify a person and use that information in a way that was not intended by the person, identified by the information
    /// </summary>
    public class Anonymizer
    {
        /// <summary>
        /// Random generator to be used by 1 way anonymizations that replaces characters with random characters like the text method
        /// </summary>
        protected Random RNDGen = new Random();
        /// <summary>
        /// Cache to save data anonymizations. This allows that the same date produces the same anonymized value
        /// </summary>
        protected List<AnonymizeCache> _dateCache;

        /// <summary>
        /// Initializes the anonymizer
        /// </summary>
        public Anonymizer()
        {
            common_constructor();
        }

        /// <summary>
        /// Function that's called in all constructors
        /// </summary>
        protected virtual void common_constructor()
        {
            _dateCache = new List<AnonymizeCache>();
        }

        //Static functions
        /// <summary>
        /// Function that encapsulates calls to the MD5, SHA1Managed en SHA512Managed classes in <see cref="System.Security.Cryptography"/> 
        /// </summary>
        /// <param name="HashType">String. MD5, SHA1 and SHA512 are legitimate values</param>
        /// <param name="input">string that will be hashed</param>
        /// <param name="format">output format of the string. See ToString(format) in the .NET documentation for details</param>
        /// <returns>returns a string with the hash value. If the hashtype is not recognized, it returns an empty string</returns>
        static string GetHash(string HashType, string input, string format)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = Encoding.Unicode.GetBytes(input);
            byte[] result;
            switch (HashType.ToUpper())
            {
                case "MD5":
                    MD5 md5Hash = MD5.Create();
                    result = md5Hash.ComputeHash(data);
                    break;
                case "SHA1":
                    SHA1 sha1M = new SHA1Managed();
                    result = sha1M.ComputeHash(data);
                    break;
                case "SHA512":
                    SHA512 sha512M = new SHA512Managed();
                    result = sha512M.ComputeHash(data);
                    break;
                default:
                    result = new byte[0];
                    break;
            }


            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one with the given format. 
            for (int i = 0; i < result.Length; i++)
            {
                sBuilder.Append(result[i].ToString(format));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }


        //protected virtual functions
        ///<summary>
        ///anonimizes a random text
        ///</summary>
        /// <param name="value">Value to be anonymized</param>
        /// <returns>Anonymized value</returns>
        ///<remarks>
        ///<para>AnonymizeText anonymizes the given data according to the rules below:</para>
        ///<list type="number">
        ///<item>Each number is replaced by a random number but not the same</item>
        ///<item>Each whitespace and aspecial character remains as is</item>
        ///<item>Each letter is replaced by a random different letter</item>
        ///<item>Special characters enclosed in <![CDATA[&]]> and <![CDATA[;]]> like <![CDATA[&quot;]]> will remain intact</item>
        ///</list>
        ///</remarks>
        protected virtual string AnonymizeText(string value)
        {
            string result = "";
            bool special_character = false;

            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                char replace = character;

                if (character == '&')
                {
                    //is er een matching ; zonder whitespace ertussen
                    int pos = value.IndexOf(';', i);
                    if (pos > i)
                    {
                        string rest = value.Substring(i, pos - i + 1);
                        //check op whitespace 
                        if (!(rest.IndexOf('\t') > 0 || rest.IndexOf(' ') > 0)) special_character = true;
                    }
                }

                if (character == ';' && special_character) special_character = false;

                if (!special_character)
                {
                    if (Char.IsLetter(character))
                        while (replace == character)
                            replace = (char)('a' + RNDGen.Next(0, 26));

                    if (Char.IsDigit(character))
                        while (replace == character)
                            replace = RNDGen.Next(10).ToString()[0];
                }

                result = String.Concat(result, replace);
            }
            return result;
        }

        /// <summary>
        /// Anonymizes a number and keeps its length so the data content is useless but its characteristics remain. 
        /// Useful for Id's as the same number yields the same anonymized value without any caching or lookup tables
        /// </summary>
        /// <param name="value">Value to be anonymized</param>
        /// <returns>Anonymized value</returns>
        protected virtual string AnonymizeNumEqLen(string value)
        {
            string result = "";
            int len = value.Length;
            // elk cijfer wordt vervangen voor een ander cijfer afhankelijk van de lengte. nul wordt niet vervangen
            for (int i = 0; i < value.Length; i++)
            {
                string character = value[i].ToString();
                string replace = character;
                int oldNum = 0;
                int newNum = 0;

                if (Int32.TryParse(character, out oldNum))
                {
                    if (character == "0")
                    {
                        replace = "0";
                    }
                    else
                    {
                        newNum = 11 - len - Convert.ToInt32(character);
                        if (newNum < 1) newNum = newNum + 9;
                        replace = newNum.ToString();
                    }
                }
                result = String.Concat(result, replace);
            }

            // elk cijfer wordt vervangen door de gespiegelde behalve als het eerste cijfer dan 0 wordt
            for (int i = 0; i < ((result.Length) / 2 - 1); i++)
            {
                string characterL = result[i].ToString();
                string characterR = result[len - i - 1].ToString();

                if (((characterR != "0")) || (i > 0))
                {
                    result = result.Substring(0, i) + characterR + result.Substring((i + 1), (len - (2 * (i + 1)))) + characterL + result.Substring(len - i, i);
                }
            }

            return result;
        }
        
        /// <summary>
        /// This function allows for numbers that are concatenated with a / and only the first number must be anonymized with the <see cref="AnonymizeRule.MethodType.NumEqLen"/> method
        /// </summary>
        /// <param name="value">Value to be anonymized</param>
        /// <returns>Anonymized value</returns>
        protected virtual string AnonymizeHouseAidVnr(string value)
        {
            string result = "";
            if (value.IndexOf("/") > 0)
            {
                string versienr = value.Substring(value.IndexOf("/"), (value.Length - value.IndexOf("/")));
                string aanvraagid = AnonymizeNumEqLen(value.Substring(0, value.IndexOf("/")));
                result = String.Concat(aanvraagid, versienr);
            }
            else
            {
                result = AnonymizeNumEqLen(value);
            }
            return result;
        }

        /// <summary>
        /// This function gives a random true or false
        /// </summary>
        /// <param name="value">Value to be anonymized</param>
        /// <returns>Anonymized value</returns>
        protected virtual string AnonymizeTrueFalse(string value)
        {
            string result = "";
            if (RNDGen.Next(0, 2) > 1)
            {
                result = "true";
            }
            else
            {
                result = "false";
            }

            return result;
        }

        /// <summary>
        /// Generates a MD5  hash , formatted as an integer
        /// </summary>
        /// <param name="value">Value to be anonymized</param>
        /// <returns>Anonymized value</returns>
        protected virtual string AnonymizeMD5Integer(string value)
        {
            string hash = "";
            hash = GetHash("MD5", value, "N0");
            return hash;
        }

        /// <summary>
        /// Generates a SHA1  hash , formatted as an integer
        /// </summary>
        /// <param name="value">Value to be anonymized</param>
        /// <returns>Anonymized value</returns>
        protected virtual string AnonymizeSHA1Integer(string value)
        {
            string hash = "";
            hash = GetHash("SHA1", value, "N0");
            return hash;
        }

        /// <summary>
        /// Generates a SHA512 hash, formatted as an integer
        /// </summary>
        /// <param name="value">Value to be anonymized</param>
        /// <returns>Anonymized value</returns>
        protected virtual string AnonymizeSHA512Integer(string value)
        {
            string hash = "";
            hash = GetHash("SHA512", value, "N0");
            return hash;
        }

        /// <summary>
        /// Generates a MD5 hash, formatted as a decimal
        /// </summary>
        /// <param name="value">Value to be anonymized</param>
        /// <returns>Anonymized value</returns>
        protected virtual string AnonymizeMD5Decimal(string value)
        {
            string hash = "";
            hash = GetHash("MD5", value, "N");
            return hash;
        }

        /// <summary>
        /// <para>if the date value has not already been anonymized, a random year is generated which lies 21 to 55 years before this date. Then a random month is chosen and a random day</para>
        /// <para>if the date value has already been anonymized by this instance of Anonymizer, the same anonymized value as before  is returned</para>
        /// </summary>
        /// <param name="value">Value to be anonymized</param>
        /// <returns>Anonymized value</returns>
        protected virtual string AnonymizeDate(string value)
        {
            //check of de datum al geanonimiseerd is en zo ja geef de bestaande waarde terug
            if (_dateCache.Count > 0)
            {
                AnonymizeCache foundcache = _dateCache.Find(
                delegate(AnonymizeCache cache)
                {
                    return cache.Original_value == value;
                }
                );
                if (foundcache != null)
                    return foundcache.Anonymous_value;
            }
            //new value
            string result = "";
            int year = DateTime.Today.AddYears(-1 * RNDGen.Next(21, 55)).Year;
            int month = RNDGen.Next(1, 12);
            int day = RNDGen.Next(1, DateTime.DaysInMonth(year, month));
            result = String.Concat(year.ToString("00"), "-", month.ToString("00"), "-", day.ToString("00"));

            AnonymizeCache newCache = new AnonymizeCache(value, result);
            _dateCache.Add(newCache);

            return result;
        }

        //Public functions

        /// <summary>
        /// Generic interface to the specialized protected functions of this class
        /// </summary>
        /// <param name="value">Value to be anonymized</param>
        /// <param name="method">Anonymization Method. See <see cref="AnonymizeRule.MethodType"/></param>
        /// <returns>Anonymized value</returns>
        public string Anonymize(string value, AnonymizeRule.MethodType method)
        {
            string result = "";
            switch (method)
            {
                case AnonymizeRule.MethodType.Date:
                    result = AnonymizeDate(value);
                    break;
                case AnonymizeRule.MethodType.MD5Integer:
                    result = AnonymizeMD5Integer(value);
                    break;
                case AnonymizeRule.MethodType.SHA1Number:
                    result = AnonymizeSHA1Integer(value);
                    break;
                case AnonymizeRule.MethodType.SHA512Number:
                    result = AnonymizeSHA512Integer(value);
                    break;
                case AnonymizeRule.MethodType.Text:
                    result = AnonymizeText(value);
                    break;
                case AnonymizeRule.MethodType.NumEqLen:
                    result = AnonymizeNumEqLen(value);
                    break;
                case AnonymizeRule.MethodType.HouseAidVnr:
                    result = AnonymizeHouseAidVnr(value);
                    break;
                case AnonymizeRule.MethodType.TrueFalse:
                    result = AnonymizeTrueFalse(value);
                    break;
                default:
                    result = value;
                    break;
            }
            return result;
        }

    }
}
