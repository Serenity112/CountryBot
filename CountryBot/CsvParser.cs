using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;

namespace CountryBot
{
    class CsvParser
    {
        public static Dictionary<string, CountryData> GetCountriesFromFile(string path)
        {
            Dictionary<string, CountryData> result = new Dictionary<string, CountryData>();

            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(":");

                while (!parser.EndOfData)
                {
                    try
                    {
                        string[] fields = new string[2];

                        fields = parser.ReadFields();

                        string country = fields[0];

                        string captial = fields[1];

                        CountryData data = new CountryData(captial);


                        result.Add(country, data);
                    }
                    catch 
                    { 
                        Console.WriteLine("Invalid country format!");
                    }

                }
            }

            return result;
        }
    }
}
