using System;


namespace Core.Anonymization
{

    /// <summary>
    /// This class contains an original value and its corresponding anonymized value so that one way anonymizations can still 
    /// have the same result in the same file.
    /// It's compatible with the List<![CDATA[<T>]]> datatype
    /// </summary>
    [Serializable()]
    public class AnonymizeCache : IEquatable<AnonymizeCache>, IComparable<AnonymizeCache>
    {
        /// <summary>
        /// The original unanonymized value
        /// </summary>
        private string _original_value;
        /// <summary>
        /// The original unanonymized value
        /// </summary>
        public string Original_value
        {
            get { return _original_value; }
            set { _original_value = value; }
        }

        /// <summary>
        /// The anonymized value
        /// </summary>
        private string _anonymous_value;
        /// <summary>
        /// The anonymized value
        /// </summary>
        public string Anonymous_value
        {
            get { return _anonymous_value; }
            set { _anonymous_value = value; }
        }

        /// <summary>
        /// Intializes original value and Anonymous value to an empty string
        /// </summary>
        public AnonymizeCache()
        {
            _original_value = "";
            _anonymous_value = "";
        }

        /// <summary>
        /// Intializes original value and Anonymous value to the value of the parameter
        /// </summary>
        /// <param name="org">New value for orginal value</param>
        /// <param name="ano">New value for anonymous value</param>
        public AnonymizeCache(string org, string ano)
        {
            _original_value = org;
            _anonymous_value = ano;
        }


        //Verplichte functies om in een generieke List te gebruiken
        /// <summary>
        /// Determines if an object is equal to this instance of AnonymizeCache
        /// </summary>
        /// <param name="obj">Any object</param>
        /// <returns>true if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            AnonymizeCache objAsAnonymizeCache = obj as AnonymizeCache;
            if (objAsAnonymizeCache == null) return false;
            else return Equals(objAsAnonymizeCache);
        }

        /// <summary>
        /// Determines if an object is equal to this instance of AnonymizeCache
        /// </summary>
        /// <param name="other">the anonymizeCache instance this instance is compared to</param>
        /// <returns>true if equal, false otherwise</returns>
        public bool Equals(AnonymizeCache other)
        {
            if (other == null) return false;
            return (_original_value.Equals(other.Original_value) && this._anonymous_value == other.Anonymous_value);
        }

        /// <summary>
        /// returns the hashcode of the original and the anonymized value, concatenated with a space in between
        /// </summary>
        /// <returns>a hashcode</returns>
        public override int GetHashCode()
        {
            return String.Concat(_original_value.ToString(), " ", _anonymous_value.ToString()).GetHashCode();
        }
        /// <summary>
        /// <para>Compares one Anonymizecache instance to another.</para>
        /// <para>Required for implementation in a generic list</para>
        /// </summary>
        /// <param name="other">the anonymizeCache instance this instance is compared to</param>
        /// <returns>return 1 if greater, 0 if equal and -1 if smaller</returns>
        public int CompareTo(AnonymizeCache other)
        {
            if (other == null) return 1;

            int rc = 0;
            if (other.Original_value.Equals(this._original_value) && other.Anonymous_value == this._anonymous_value) rc = 0;
            else rc = _original_value.CompareTo(other.Original_value);

            return rc;
        }
    }
}
