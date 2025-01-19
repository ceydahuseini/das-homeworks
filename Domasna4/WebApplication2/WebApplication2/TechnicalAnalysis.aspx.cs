using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication2
{
    public partial class Contact : System.Web.UI.Page
    {
        protected string ChartLabelsJson { get; set; }
        protected string ChartPricesJson { get; set; }
        protected string ChartMovingAveragesJson { get; set; }

        private static readonly HttpClient httpClient = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(LoadIssuerCodesAsync));
            }
        }

        private async Task LoadIssuerCodesAsync()
        {
            string issuerCodeUrl = "https://www.mse.mk/mk/stats/symbolhistory/REPL";
            var issuerCodes = await FetchIssuerCodesAsync(issuerCodeUrl);

            if (issuerCodes != null && issuerCodes.Count > 0)
            {
                IssuerDropdownList.DataSource = issuerCodes;
                IssuerDropdownList.DataBind();
                IssuerDropdownList.Items.Insert(0, new ListItem("Select an issuer", ""));
            }
            else
            {
                ErrorMessageLabel.Text = "Failed to load issuer codes.";
            }
        }

        protected void DisplayDataButton_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(DisplayDataAsync));
        }

        private async Task DisplayDataAsync()
        {
            string selectedStockIssuer = IssuerDropdownList.SelectedValue;

            if (!string.IsNullOrEmpty(selectedStockIssuer))
            {
                string stockDataUrl = $"https://www.mse.mk/mk/stats/symbolhistory/{selectedStockIssuer}";
                var stockData = await FetchStockDataAsync(stockDataUrl);

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

        private async Task<List<string>> FetchIssuerCodesAsync(string url)
        {
            string apiUrl = $"https://localhost:44349/api/IssuerCode?url={Uri.EscapeDataString(url)}";
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            HttpClient httpClient = new HttpClient(handler);

            var response = await httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<string>>(content);
            }
            return null;
        }


        private async Task<DataTable> FetchStockDataAsync(string url)
        {
            
            string apiUrl = $"https://localhost:44374/api/StockData?url={Uri.EscapeDataString(url)}";

            
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            HttpClient httpClient = new HttpClient(handler);

            
            var response = await httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(content);

                
                DataTable dataTable = new DataTable();
                if (data != null && data.Count > 0)
                {
                    foreach (var key in data[0].Keys)
                    {
                        dataTable.Columns.Add(key);
                    }

                    foreach (var row in data)
                    {
                        var dataRow = dataTable.NewRow();
                        foreach (var key in row.Keys)
                        {
                            dataRow[key] = row[key];
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }

                return dataTable;
            }

            
            ErrorMessageLabel.Text = $"API Error: {response.StatusCode} - {response.ReasonPhrase}";
            return null;
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