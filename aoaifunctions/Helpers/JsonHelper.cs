using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace aoaifunctions.Helpers
{
    public class JsonHelper
    {
        /// <summary>
        /// JSON Serialization
        /// </summary>
        public static string Serializer<T>(T t)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var ms = new MemoryStream();
            serializer.WriteObject(ms, t);
            var jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }

        /// <summary>
        /// JSON Deserialization
        /// </summary>
        public static T Deserialize<T>(string jsonString)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var obj = (T)serializer.ReadObject(ms);
            return obj;
        }
    }
}
