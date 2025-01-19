## Домашна - 2
За да може веб-апликацијата да работи, следнава команда мора да биде инсталирана од Tools -> nuget package manager -> package manager console.
`Install-Package HtmlAgilityPack`

## Домашна - 3
[Линк до видеото](https://drive.google.com/file/d/1U2ftEqsMR9NtkOv7FcvkDmRoKgVTmdQp/view?usp=sharing)

## Домашна - 4
### 1. Код за рефакторирање со користење на моделот Singleton 
 Во овој проект, следните две класи се конфигурирани со моделот на дизајнирање `Singleton`:

 - **IssuerDataService**: Класа која се користи за преземање кодови и податоци на издавачот.
 - **DataFetcher**: Класа која се користи за преземање веб-податоци и добивање податоци за акции.

 Овој шаблон осигурува дека е создаден само еден пример од која било класа и тој пример може да се користи на различни места. На овој начин, операциите со податоци се вршат на централна точка, се обезбедува ефикасност на меморијата и управувањето може да се врши од еден извор.

IssuerDataService е класа што се користи за преземање и обработка на податоци за издавачот. Оваа класа гарантира дека ќе се создаде само еден пример.

    public sealed class IssuerDataService
    {
       private static readonly Lazy<IssuerDataService> _instance = new Lazy<IssuerDataService>(() => new IssuerDataService());

       public static IssuerDataService Instance => _instance.Value;

       private IssuerDataService() { }

       public List<string> GetIssuerCodes(string url) { ... }
       public DataTable FetchIssuerData(string url) { ... }
    }

DataFetcher е уште една класа на Singleton што се користи за преземање податоци за акции и кодови на издавачи.

    public class DataFetcher
    {
      private static DataFetcher _instance;

      private DataFetcher() { }

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

       public List<string> FetchIssuerCodes(string url) { ... }
       public DataTable FetchStockData(string url) { ... }
    }
### 2.StockDataFetcher и IssuerCodeFetcher Микросервиси
### Опис
`StockDataFetcher` зема табеларни податоци од дадена веб-страница и ги враќа обработени во JSON формат.

### Користење
- **Endpoint**: `GET /api/StockData`
- **Query Parameter**:
  - `url` (string): URL на веб-страницата од која ќе се преземат податоците.
- **Response**:
  - Во случај на успех, враќа табеларни податоци во JSON формат.
  - Во случај на грешка, враќа соодветна порака за грешка и HTTP статус код.
 
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
 ### Опис
`IssuerCodeFetcher` ги зема кодовите од `<select>` тагот на дадена веб-страница, извлекувајќи ги само оние кои содржат букви, и ги враќа како листа.

### Користење
- **Endpoint**: `GET /api/IssuerCode`
- **Query Parameter**:
  - `url` (string): URL на веб-страницата од која ќе се преземаат кодовите.
- **Response**:
  - Во случај на успех, враќа листа со кодови во JSON формат.
  - Во случај на грешка, враќа соодветна порака за грешка и HTTP статус код
 
