using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Net;

namespace StockDataFetcher.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockDataController : ControllerBase
    {
        [HttpGet]
        public IActionResult FetchStockData([FromQuery] string url)
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
                if (tableNode == null) return NotFound("No table found.");

                DataTable dataTable = new DataTable();
                var rows = tableNode.SelectNodes(".//tr");

                if (rows == null || rows.Count == 0)
                    return NotFound("No rows found.");

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

                
                var result = ConvertDataTableToDictionary(dataTable);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching stock data: {ex.Message}");
            }
        }

        private List<Dictionary<string, object>> ConvertDataTableToDictionary(DataTable dataTable)
        {
            var result = new List<Dictionary<string, object>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    dict[column.ColumnName] = row[column];
                }
                result.Add(dict);
            }

            return result;
        }
    }
}