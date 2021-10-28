using System;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

using System.Text.Json;

namespace MTF.Utilities
{
    public static class Serializing
    {
        public static string XmlSerialize<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, value);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Serialization error occured.", ex);
            }
        }

        public static T XmlDeSerialize<T>(this T value,
                                          string xml)
        {
            var Sr = new XmlSerializer(typeof(T));
            using var srd = new StringReader(xml);
            return (T)Sr.Deserialize(srd);
        }
    }
}
