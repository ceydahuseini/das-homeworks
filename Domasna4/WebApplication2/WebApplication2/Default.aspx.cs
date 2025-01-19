using System;
using System.Data;
using System.Collections.Generic;
using WebApplication2.Services;
using System.Web.UI.WebControls;

namespace WebApplication2
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    // Use the Singleton instance to fetch issuer codes
                    var issuerCodes = IssuerDataService.Instance.GetIssuerCodes("https://www.mse.mk/mk/stats/symbolhistory/REPL");

                    if (issuerCodes.Count > 0)
                    {
                        IssuerDropDown.DataSource = issuerCodes;
                        IssuerDropDown.DataBind();
                        IssuerDropDown.Items.Insert(0, new ListItem("Select an issuer", ""));
                    }
                    else
                    {
                        Label1.Text = "No issuer codes available.";
                    }
                }
                catch (Exception ex)
                {
                    Label1.Text = ex.Message;
                }
            }
        }

        protected void ShowButton_Click(object sender, EventArgs e)
        {
            string selectedIssuer = IssuerDropDown.SelectedValue;

            if (!string.IsNullOrEmpty(selectedIssuer))
            {
                string url = $"https://www.mse.mk/mk/stats/symbolhistory/{selectedIssuer}";

                try
                {
                    // Use the Singleton instance to fetch issuer data
                    DataTable data = IssuerDataService.Instance.FetchIssuerData(url);

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
                catch (Exception ex)
                {
                    Label1.Text = ex.Message;
                }
            }
            else
            {
                Label1.Text = "Please select an issuer from the dropdown.";
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
