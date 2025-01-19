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
