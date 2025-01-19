using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;

public class DataFetcher
{
    private static DataFetcher _instance;

    // Private constructor to prevent instantiation from other classes
    private DataFetcher() { }

    // Public method to get the single instance of the class
    public static DataFetcher Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DataFetcher();
            }
            return _instance;
        }
    }

    // Method to fetch issuer codes from the provided URL
    public List<string> FetchIssuerCodes(string url)
    {
        List<string> stockIssuerCodes = new List<string>();
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

            var dropdownNode = doc.DocumentNode.SelectSingleNode("//select");
            if (dropdownNode != null)
            {
                foreach (var optionNode in dropdownNode.SelectNodes(".//option"))
                {
                    string code = optionNode.InnerText.Trim();
                    if (!string.IsNullOrEmpty(code) && code.All(char.IsLetter))
                    {
                        stockIssuerCodes.Add(code);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching issuer codes: " + ex.Message);
        }
        return stockIssuerCodes;
    }

    // Method to fetch stock data from the provided URL
    public DataTable FetchStockData(string url)
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

            DataTable dataTable = new DataTable();
            var rows = tableNode.SelectNodes(".//tr");

            if (rows == null || rows.Count == 0)
                return null;

            var headers = rows[0].SelectNodes(".//th|.//td");
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    string columnName = header.InnerText.Trim();
                    if (string.IsNullOrEmpty(columnName))
                        columnName = $"Column{dataTable.Columns.Count + 1}";

                    dataTable.Columns.Add(columnName);
                }
            }

            for (int i = 1; i < rows.Count; i++)
            {
                var cells = rows[i].SelectNodes(".//td");
                if (cells == null || cells.Count != dataTable.Columns.Count)
                    continue;

                DataRow dataRow = dataTable.NewRow();
                for (int j = 0; j < cells.Count; j++)
                {
                    dataRow[j] = cells[j].InnerText.Trim();
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching stock data: " + ex.Message);
        }
    }
}
