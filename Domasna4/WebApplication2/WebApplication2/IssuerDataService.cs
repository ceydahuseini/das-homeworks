using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace WebApplication2.Services
{
    public sealed class IssuerDataService
    {
        // Lazy initialization of the Singleton instance
        private static readonly Lazy<IssuerDataService> _instance = new Lazy<IssuerDataService>(() => new IssuerDataService());

        // Public property to access the Singleton instance
        public static IssuerDataService Instance => _instance.Value;

        // Private constructor to prevent direct instantiation
        private IssuerDataService() { }

        // Method to fetch issuer codes
        public List<string> GetIssuerCodes(string url)
        {
            List<string> issuerCodes = new List<string>();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0";

                string html;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    html = reader.ReadToEnd();
                }

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                var dropdown = doc.DocumentNode.SelectSingleNode("//select");
                if (dropdown != null)
                {
                    foreach (var option in dropdown.SelectNodes(".//option"))
                    {
                        string code = option.InnerText.Trim();
                        if (!string.IsNullOrEmpty(code) && code.All(char.IsLetter))
                        {
                            issuerCodes.Add(code);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching issuer codes: " + ex.Message);
            }

            return issuerCodes;
        }

        // Method to fetch issuer data
        public DataTable FetchIssuerData(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0";

                string html;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    html = reader.ReadToEnd();
                }

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                var tableNode = doc.DocumentNode.SelectSingleNode("//table");
                if (tableNode == null) return null;

                DataTable table = new DataTable();
                var rows = tableNode.SelectNodes(".//tr");

                var headers = rows[0].SelectNodes(".//th|.//td");
                foreach (var header in headers)
                {
                    table.Columns.Add(header.InnerText.Trim());
                }

                for (int i = 1; i < rows.Count; i++)
                {
                    var cells = rows[i].SelectNodes(".//td");
                    DataRow dataRow = table.NewRow();

                    for (int j = 0; j < cells.Count; j++)
                    {
                        dataRow[j] = cells[j].InnerText.Trim();
                    }

                    table.Rows.Add(dataRow);
                }

                return table;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data: " + ex.Message);
            }
        }
    }
}
