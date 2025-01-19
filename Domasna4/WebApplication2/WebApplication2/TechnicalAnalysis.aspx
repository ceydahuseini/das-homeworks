<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TechnicalAnalysis.aspx.cs" Inherits="WebApplication2.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Stock Analysis</h2>

    
    <asp:DropDownList ID="IssuerDropdownList" runat="server"></asp:DropDownList>
    <asp:Button ID="DisplayDataButton" runat="server" Text="Show Data" OnClick="DisplayDataButton_Click" />
    <asp:Label ID="ErrorMessageLabel" runat="server" ForeColor="Red"></asp:Label>

    
    <asp:GridView ID="StockGridView" runat="server" AutoGenerateColumns="True" HeaderStyle-BackColor="#007bff"
        HeaderStyle-ForeColor="White" GridLines="None" BorderStyle="Solid" BorderWidth="1px">
    </asp:GridView>

    <h3>Stock Price Chart</h3>
    <canvas id="stockChart" style="width: 100%; height: 400px;"></canvas>

  
    <div id="tradingview-widget" style="margin-top: 30px;"></div>

    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script type="text/javascript" src="https://s3.tradingview.com/tv.js"></script>
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            
            var stockLabels = <%= ChartLabelsJson %>;
            var stockPrices = <%= ChartPricesJson %>;
            var movingAverages = <%= ChartMovingAveragesJson %>;

          
            var ctx = document.getElementById("stockChart").getContext("2d");
            var stockChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: stockLabels,
                    datasets: [{
                        label: 'Stock Prices',
                        data: stockPrices,
                        borderColor: 'rgba(0, 123, 255, 1)',
                        backgroundColor: 'rgba(0, 123, 255, 0.2)',
                        borderWidth: 2,
                        fill: true
                    }, {
                        label: '5-Day Moving Average',
                        data: movingAverages,
                        borderColor: 'rgba(255, 99, 132, 1)',
                        backgroundColor: 'rgba(255, 99, 132, 0.2)',
                        borderWidth: 2,
                        fill: false
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'top',
                        }
                    },
                    scales: {
                        x: {
                            title: {
                                display: true,
                                text: 'Date'
                            }
                        },
                        y: {
                            title: {
                                display: true,
                                text: 'Price'
                            }
                        }
                    }
                }
            });

          
            var selectedStockIssuer = "<%= IssuerDropdownList.SelectedValue %>";
            if (selectedStockIssuer) {
                var tradingViewSymbol = "NASDAQ:" + selectedStockIssuer;
                new TradingView.widget({
                    "width": 980,
                    "height": 610,
                    "symbol": tradingViewSymbol,
                    "interval": "D",
                    "timezone": "Etc/UTC",
                    "theme": "light",
                    "style": "1",
                    "locale": "en",
                    "toolbar_bg": "#f1f3f6",
                    "enable_publishing": false,
                    "allow_symbol_change": true,
                    "container_id": "tradingview-widget"
                });
            }
        });
    </script>
</asp:Content>
