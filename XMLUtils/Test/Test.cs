using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Anonymization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Test
{
  class Test
  {
    static void Main(string[] args)
    {
      List<AnonymizeCache> myCache = new List<AnonymizeCache>();
      myCache.Add(new AnonymizeCache("9", "1"));
      myCache.Add(new AnonymizeCache("8", "2"));
      myCache.Add(new AnonymizeCache("7", "3"));
      myCache.Add(new AnonymizeCache("6", "4"));
      myCache.Add(new AnonymizeCache("5", "5"));
      myCache.Add(new AnonymizeCache("4", "6"));
      myCache.Add(new AnonymizeCache("3", "7"));
      myCache.Add(new AnonymizeCache("2", "8"));
      myCache.Add(new AnonymizeCache("1", "9"));

      myCache.Add(new AnonymizeCache("Z", "A"));
      myCache.Add(new AnonymizeCache("Y", "B"));
      myCache.Add(new AnonymizeCache("X", "C"));
      myCache.Add(new AnonymizeCache("W", "D"));
      myCache.Add(new AnonymizeCache("V", "E"));
      myCache.Add(new AnonymizeCache("U", "F"));


      while (true)
      {
        Console.WriteLine("s=serialize, r=read, t=sort:");
        switch (Console.ReadLine())
        {
          case "s":

            try
            {
              using (Stream stream = File.Open("data.bin", FileMode.Create))
              {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, myCache);
              }
            }
            catch (IOException)
            {
            }
            break;

          case "r":
            try
            {
              using (Stream stream = File.Open("data.bin", FileMode.Open))
              {
                BinaryFormatter bin = new BinaryFormatter();

                myCache = (List<AnonymizeCache>)bin.Deserialize(stream);
                foreach (AnonymizeCache cacheItem in myCache)
                {
                  Console.WriteLine("{0} = {1}",
                      cacheItem.Original_value,
                      cacheItem.Anonymous_value);
                }
              }
            }
            catch (IOException)
            {
            }
            break;
          case "t":
            try
            {
              //sort Anonymizecache
              myCache.Sort();
            }
            catch (IOException)
            {
            }
            break;

        }
      }
    }
  }
}
