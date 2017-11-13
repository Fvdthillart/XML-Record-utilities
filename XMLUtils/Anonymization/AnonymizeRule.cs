using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Anonymization
{
  /// <summary>
  /// <para>This class is a value class for storing anonymization rules</para>
  /// <para>
  /// An anonymization rule consists of a location and a method of anonymization as specified in <see cref="Core.Anonymization.AnonymizeRule.MethodType"/>.
  /// </para>
  /// </summary>
  public class AnonymizeRule : IEquatable<AnonymizeRule>, IComparable<AnonymizeRule>
  {  
    /// <summary>
    /// Contains all anonymization methods that are provided by class <see cref="Core.Anonymization.Anonymizer"/>
    /// </summary>
    public enum MethodType : byte {  /// <summary>
                     /// Default value for any variables of this type
                     /// </summary>
                     Unknown = 1, 
                     /// <summary>
                     /// Indicates that values will be anonymized by replacing letters with a random other letter and numbers with a different random number
                     /// </summary>
                     Text,
                     /// <summary>
                     /// Replaces the original date with a random date. Uses <see cref="Core.Anonymization.AnonymizeCache "/> to produce the same result date with the same input for the
                     /// same instance of anonymizer
                     /// </summary>
                     Date,
                     /// <summary>
                     /// Makes an MD5 hash of the input value. Unfortunately, it generates a LONG value even when the original value is short (a 4 char string produces a 32 char anonymized value)
                     /// </summary>
                     MD5Integer,
                     /// <summary>
                     /// Makes a SHA 512 hash of the input value. Unfortunately, it generates a LONG value even when the original value is short (a 4 char string produces a 32 char anonymized value)
                     /// </summary>
                     SHA512Number,
                     /// <summary>
                     /// Makes a SHA 1 hash of the input value. Unfortunately, it generates a LONG value even when the original value is short (a 4 char string produces a 32 char anonymized value)
                     /// </summary>
                     SHA1Number,
                     /// <summary>
                     /// A custom algorithm that anonymizes a number while retaining its length
                     /// </summary>
                     NumEqLen,
                     /// <summary>
                     /// Method specfic to the HOUSE XML anonymization where a key value is concatenatd with a version
                     /// </summary>
                     HouseAidVnr,
                     /// <summary>
                     /// Randomizes a true or false, regardless of the original value
                     /// </summary>
                     TrueFalse,
                     GUID,
                     /// <summary>
                     /// <para>Returns an empty value, regardless of original value if no filter is supplied</para>
                     /// <para>When a filter IS supplied in the <see cref="Core.Anonymization.AnonymizeRule"/>, this method makes more sense.
                     /// It's especially useful when needing to eliminate old values from a set. See function <see cref="Core.Anonymization.AnonymizerWithRules.ProcessFilter"/>
                     /// </para>
                     /// </summary>
                     Empty };
    /// <summary>
    /// see <see cref="Location"/>
    /// </summary>
    private string _location;
    /// <summary>
    /// <para>Location of the value to be anonymized.</para>
    /// <para>This can be an xpath expression for XML or the index number of a field in a CSV file</para>
    /// </summary>
    /// <value>
    /// Contains the location of the item to be anonymized
    /// </value>
    public string Location
    {
      get { return _location; }
      set { _location = value; }
    }

    /// <summary>
    /// See <see cref="Method"/>
    /// </summary>
    private MethodType _method;
    /// <summary>
    /// Contains the method. See <see cref="Core.Anonymization.AnonymizeRule.MethodType"/> for more information
    /// </summary>
    public MethodType Method
    {
      get { return _method; }
      set { _method = value; }
    }

    /// <summary>
    /// See <see cref="FilterLocation"/>
    /// </summary>
    private string _filterLocation;
    /// <summary>
    /// First part of the filter. Contains the data element that's being filtered
    /// </summary>
    public string FilterLocation
    {
      get { return _filterLocation; }
      set { _filterLocation = value; }
    }
    
    /// <summary>
    /// See <see cref="FilterOperator"/>
    /// </summary>
    private string _filterOperator;
    /// <summary>
    /// Second part of a filter. Contains the operator of the filter like <![CDATA[<]]>
    /// </summary>
    public string FilterOperator
    {
      get { return _filterOperator; }
      set { _filterOperator = value; }
    }
    
    /// <summary>
    /// See <see cref="FilterValue"/>
    /// </summary>
    private string _filterValue;
    /// <summary>
    /// Third part of a filter. Contains the value to which the filter compares the data element, specified in <see cref="FilterLocation"/>
    /// </summary>
    public string FilterValue
    {
      get { return _filterValue; }
      set { _filterValue = value; }
    }

    /// <summary>
    /// See <see cref="Allowed"/>
    /// </summary>
    private bool _allowed = true;
    /// <summary>
    /// Indicates whether a rule is Allowed to be used. This attribute allows for disabling a rule during processing if deemed necessary
    /// </summary>
    public bool Allowed
    {
      get { return _allowed; }
      set { _allowed = value; }
    }

    /// <summary>
    /// Indicates that the rule has a filter
    /// </summary>
    public bool IsFiltered
    {
      get
      {
        return (_filterLocation != null && _filterValue != null && _filterOperator != null); }
    }

    /// <summary>
    /// Read only property that returns the filter of the rule
    /// </summary>
    public string Filter
    {
      get { return String.Concat(_filterLocation, _filterOperator, _filterValue); }
    }
    /// <summary>
    /// Intializes the properties with default values
    /// </summary>
    public AnonymizeRule()
    {
      this._method = MethodType.Unknown;
      this._location = "";
    }
    /// <summary>
    /// Initializes method and location with the parameter values
    /// </summary>
    /// <param name="loc">location of the data element to anonymize</param>
    /// <param name="method">Method to use on the data element location points to. See <see cref="MethodType"/></param>
    public AnonymizeRule(String loc, MethodType method)
    {
      this._method = method;
      this._location = loc;
    }


    /// <summary>
    /// Translates the text version of MethodType to the correspondig value of enumeration <see cref="MethodType"/>
    /// </summary>
    /// <param name="method">string thst contains the value that is to be translated to a value of <see cref="MethodType"/></param>
    /// <returns>if found, a value of type <see cref="MethodType"/>, otherwise the <see cref="MethodType"/> value Unknown</returns>
    public static MethodType getMethod(string method)
    {
      MethodType result = MethodType.Unknown;
      switch (method.ToLower())
      {
        case "text":
          result = MethodType.Text;
          break;
        case "date":
          result = MethodType.Date;
          break;
        case "md5integer":
          result = MethodType.MD5Integer;
          break;
        case "sha1integer":
          result = MethodType.SHA1Number;
          break;
        case "sha512integer":
          result = MethodType.SHA512Number;
          break;
        case "numeqlen":
          result = MethodType.NumEqLen;
          break;
        case "houseaidvnr":
          result = MethodType.HouseAidVnr;
          break;
        case "truefalse":
          result = MethodType.TrueFalse;
          break;
        case "guid":
          result = MethodType.GUID;
          break;
        case "empty" :
          result = MethodType.Empty;
          break;
        default:
          result = MethodType.Unknown;
          break;
      }
      return result;

    }

    //Verplichte functies om in een generieke List te gebruiken
    /// <summary>
    /// Determines if an object is equal to this instance of AnonymizeRule
    /// </summary>
    /// <param name="obj">Any object</param>
    /// <returns>true if equal, false otherwise</returns>    
    public override bool Equals(object obj)
    {
      if (obj == null) return false;
      AnonymizeRule objAsAnonymizeRule = obj as AnonymizeRule;
      if (objAsAnonymizeRule == null) return false;
      else return Equals(objAsAnonymizeRule);
    }

    /// <summary>
    /// Determines if an object is equal to this instance of <see cref="Core.Anonymization.AnonymizeRule"/>
    /// </summary>
    /// <param name="other">the anonymizeCache instance this instance is compared to</param>
    /// <returns>true if equal, false otherwise</returns>
    public bool Equals(AnonymizeRule other)
    {
      if (other == null) return false;
      return (this._location.Equals(other.Location) && this._method == other.Method);
    }

    /// <summary>
    /// returns the hashcode of the original and the anonymized value, concatenated with a space in between
    /// </summary>
    /// <returns>a hashcode</returns>
    public override int GetHashCode()
    {
      return String.Concat(_location.ToString(), " ", _method.ToString()).GetHashCode();
    }

    /// <summary>
    /// <para>Compares one anonymization rule instance to another.</para>
    /// <para>Required for implementation in a generic list</para>
    /// </summary>
    /// <param name="other">the anonymizeRule instance this instance is compared to</param>
    /// <returns>return 1 if greater, 0 if equal and -1 if smaller</returns>
    public int CompareTo(AnonymizeRule other)
    {
      if (other == null) return 1;

      int rc = 0;
      if (other.Location.Equals(this._location) && other.Method == this._method) rc = 0;
      else rc = 1;

      return rc;
    }

    /// <summary>
    /// String representation of this rule
    /// </summary>
    /// <returns>returns the rule's location and method and if present its filter</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat("Rule with location \"{0}\" and method {1}",_location, _method.ToString());
      if (Filter.Length > 0)
        sb.AppendFormat(" and filter is \"{0}\"",Filter);
      return sb.ToString();
    }
  }
}
