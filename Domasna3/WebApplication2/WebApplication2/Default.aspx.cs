using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.UI.WebControls;
using HtmlAgilityPack;

namespace WebApplication2
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                string url = "https://www.mse.mk/mk/stats/symbolhistory/REPL";
                List<string> issuerCodes = GetIssuerCodes(url);

                if (issuerCodes.Count > 0)
                {

                    IssuerDropDown.DataSource = issuerCodes;
                    IssuerDropDown.DataBind();
                    IssuerDropDown.Items.Insert(0, new ListItem("Select an issuer", ""));
                }
                else
                {
                    Label1.Text = "Failed to load issuer codes.";
                }
            }
        }

        private List<string> GetIssuerCodes(string url)
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
                Label1.Text = "Error fetching issuer codes: " + ex.Message;
            }

            return issuerCodes;
        }

        protected void ShowButton_Click(object sender, EventArgs e)
        {
            string selectedIssuer = IssuerDropDown.SelectedValue;

            if (!string.IsNullOrEmpty(selectedIssuer))
            {
                string url = $"https://www.mse.mk/mk/stats/symbolhistory/{selectedIssuer}";
                DataTable data = FetchIssuerData(url);

                if (data != null)
                {
                    GridView1.DataSource = data;
                    GridView1.DataBind();
                }
                else
                {
                    Label1.Text = $"No data found for issuer: {selectedIssuer}";
                    GridView1.DataSource = null;
                    GridView1.DataBind();
                }
            }
            else
            {
                Label1.Text = "Please select an issuer from the dropdown.";
            }
        }

        private DataTable FetchIssuerData(string url)
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
                Label1.Text = "Error fetching data: " + ex.Message;
                return null;
            }
        }


        protected void ExportButton_Click(object sender, EventArgs e)
        {
            if (GridView1.Rows.Count > 0)
            {
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment;filename=StockData.csv");
                Response.ContentType = "application/text";


                for (int i = 0; i < GridView1.Columns.Count; i++)
                {
                    Response.Write(GridView1.Columns[i].HeaderText.Replace(",", "") + (i == GridView1.Columns.Count - 1 ? "" : ","));
                }
                Response.Write("\r\n");


                foreach (GridViewRow row in GridView1.Rows)
                {
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        Response.Write(row.Cells[i].Text.Replace(",", "") + (i == row.Cells.Count - 1 ? "" : ","));
                    }
                    Response.Write("\r\n");
                }

                Response.End();
            }
            else
            {
                Label1.Text = "No data available to export.";
            }
        }
    }
}