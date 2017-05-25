using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace EventStreamDb.Examples.CsvImport
{
    public class CsvRow
    {
        public DateTime Timestamp {get;set;}
        public string IPAddress {get;set;}
        public string UserName {get;set;}
        public string Domain {get;set;}
    }

    public static class CsvLoader
    {
        public static IEnumerable<CsvRow> Load(string fileName)
        {
            using (var csv = new CsvReader(new StreamReader(File.OpenRead(fileName))))
            {
                return csv.GetRecords<CsvRow>().ToArray();
            }
        }
    }
}