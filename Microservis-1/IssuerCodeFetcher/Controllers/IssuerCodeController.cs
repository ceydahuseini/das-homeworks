using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace IssuerCodeFetcher.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IssuerCodeController : ControllerBase
    {
        [HttpGet]
        public IActionResult FetchIssuerCodes([FromQuery] string url)
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
                return StatusCode(500, $"Error fetching issuer codes: {ex.Message}");
            }
            return Ok(stockIssuerCodes);
        }
    }
}