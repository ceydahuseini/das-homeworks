using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace WebApplication2
{
    public partial class Contact : Page
    {
        protected string ChartLabelsJson { get; set; }
        protected string ChartPricesJson { get; set; }
        protected string ChartMovingAveragesJson { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string url = "https://www.mse.mk/mk/stats/symbolhistory/REPL";
                List<string> stockIssuerCodes = FetchIssuerCodes(url);

                if (stockIssuerCodes.Count > 0)
                {
                    IssuerDropdownList.DataSource = stockIssuerCodes;
                    IssuerDropdownList.DataBind();
                    IssuerDropdownList.Items.Insert(0, new ListItem("Select an issuer", ""));
                }
                else
                {
                    ErrorMessageLabel.Text = "Failed to load issuer codes.";
                }
            }
        }

        private List<string> FetchIssuerCodes(string url)
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
                ErrorMessageLabel.Text = "Error fetching issuer codes: " + ex.Message;
            }

            return stockIssuerCodes;
        }

        protected void DisplayDataButton_Click(object sender, EventArgs e)
        {
            string selectedStockIssuer = IssuerDropdownList.SelectedValue;

            if (!string.IsNullOrEmpty(selectedStockIssuer))
            {
                string url = $"https://www.mse.mk/mk/stats/symbolhistory/{selectedStockIssuer}";
                DataTable stockData = FetchStockData(url);

                if (stockData != null)
                {
                    StockGridView.DataSource = stockData;
                    StockGridView.DataBind();

            
                    List<string> chartLabels = new List<string>();
                    List<decimal> chartPrices = new List<decimal>();
                    List<decimal> movingAverages = new List<decimal>();

                    if (stockData.Columns.Contains("Date") && stockData.Columns.Contains("Price"))
                    {
                        List<decimal> prices = new List<decimal>();

                        foreach (DataRow row in stockData.Rows)
                        {
                            chartLabels.Add(row["Date"].ToString());
                            decimal price = decimal.Parse(row["Price"].ToString());
                            chartPrices.Add(price);
                            prices.Add(price);
                        }

                        
                        movingAverages = CalculateMovingAverage(prices, 5);
                    }
                    else
                    {
                        //ErrorMessageLabel.Text = "Required columns ('Date' and 'Price') are missing in the data.";
                    }

   
                    ChartLabelsJson = JsonConvert.SerializeObject(chartLabels);
                    ChartPricesJson = JsonConvert.SerializeObject(chartPrices);
                    ChartMovingAveragesJson = JsonConvert.SerializeObject(movingAverages);
                }
                else
                {
                    ErrorMessageLabel.Text = $"No data found for issuer: {selectedStockIssuer}";
                    StockGridView.DataSource = null;
                    StockGridView.DataBind();
                }
            }
            else
            {
                ErrorMessageLabel.Text = "Please select an issuer from the dropdown.";
            }
        }

        private DataTable FetchStockData(string url)
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
                ErrorMessageLabel.Text = "Error fetching data: " + ex.Message;
                return null;
            }
        }

        private List<decimal> CalculateMovingAverage(List<decimal> prices, int period)
        {
            List<decimal> movingAverages = new List<decimal>();

            for (int i = 0; i < prices.Count; i++)
            {
                if (i + 1 >= period)
                {
                    decimal sum = 0;
                    for (int j = i + 1 - period; j <= i; j++)
                    {
                        sum += prices[j];
                    }
                    movingAverages.Add(sum / period);
                }
                else
                {
                    movingAverages.Add(0); 
                }
            }

            return movingAverages;
        }
    }
}
