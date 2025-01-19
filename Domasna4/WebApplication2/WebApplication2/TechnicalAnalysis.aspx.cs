using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using System.Web.UI;
using System.Web.UI.WebControls;

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
                List<string> stockIssuerCodes = DataFetcher.Instance.FetchIssuerCodes(url);

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

        protected void DisplayDataButton_Click(object sender, EventArgs e)
        {
            string selectedStockIssuer = IssuerDropdownList.SelectedValue;

            if (!string.IsNullOrEmpty(selectedStockIssuer))
            {
                string url = $"https://www.mse.mk/mk/stats/symbolhistory/{selectedStockIssuer}";
                DataTable stockData = DataFetcher.Instance.FetchStockData(url);

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
