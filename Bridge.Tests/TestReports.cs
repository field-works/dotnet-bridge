using System;
using System.IO;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FieldWorks.FieldReports
{
    [TestClass]
    public class TestReports
    {
        private IProxy Reports;

        public TestReports()
        {
            Reports = Bridge.CreateProxy();
        }

        private static string PdfHeader(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes.Take(8).ToArray());
        }

        private static string PdfFooter(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes.Skip(bytes.Length - 6).ToArray()).TrimEnd();
        }

        [TestMethod]
        public void バージョンが取得できる()
        {
            var version = Reports.Version();
            Assert.AreEqual("2.", version.Substring(0, 2));
        }

        [TestMethod]
        public void JSON文字列を元にPDFを生成できる()
        {
            var param = @"{
                ""template"": {""paper"": ""A4""},
                ""context"": {
                ""hello"": {
                    ""new"": ""Tx"",
                    ""value"": ""Hello, World!"",
                    ""rect"": [100, 700, 400, 750]
                    }
                }
            }";
            var pdf = Reports.Render(param);
            Assert.AreEqual("%PDF", PdfHeader(pdf).Substring(0, 4));
            Assert.AreEqual("%%EOF", PdfFooter(pdf).Substring(0, 5));
        }

        [TestMethod]
        public void 動的に組み立てたパラメータでPDFを生成できる()
        {
            var param = new
            {
                template = new
                {
                    paper = "A4"
                },
                context = new
                {
                    hello = new
                    {
                        @new = "Tx",
                        value = "Hello, World!",
                        rect = new int[] { 100, 700, 400, 750 }
                    }
                }
            };
            var pdf = Reports.Render(param);
            Assert.AreEqual("%PDF", PdfHeader(pdf).Substring(0, 4));
            Assert.AreEqual("%%EOF", PdfFooter(pdf).Substring(0, 5));
        }

        [TestMethod]
        [ExpectedException(typeof(ReportsException))]
        public void パースエラーで例外が発生する()
        {
            var pdf = Reports.Render("{,}");
        }

        [TestMethod]
        public void PDFデータを解析できる()
        {
            using (var fs = new FileStream("./files/mitumori.pdf", FileMode.Open, FileAccess.Read))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                var s = Reports.Parse(ms.ToArray());
                Assert.IsTrue(s.TrimStart().StartsWith("{"));
                Assert.IsTrue(s.TrimEnd().EndsWith("}"));
                var json = JObject.Parse(s);
            }
        }
    }
}
