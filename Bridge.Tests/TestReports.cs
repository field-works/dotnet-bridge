using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;
using Xunit;

namespace FieldWorks.FieldReports
{
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

        [Fact]
        public void バージョンが取得できる() 
        {
            var version = Reports.Version();
            Assert.StartsWith("2.", version);
        }

        [Fact]
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
            Assert.StartsWith("%PDF", PdfHeader(pdf));
            Assert.EndsWith("%%EOF", PdfFooter(pdf));
        }

        [Fact]
        public void 動的に組み立てたパラメータでPDFを生成できる()
        {
            var param = new {
                template = new {
                    paper = "A4"
                },
                context = new {
                    hello = new {
                        @new = "Tx",
                        value = "Hello, World!",
                        rect = new int[] {100, 700, 400, 750}
                    }
                }
            };
            var pdf = Reports.Render(param);
            Assert.StartsWith("%PDF", PdfHeader(pdf));
            Assert.EndsWith("%%EOF", PdfFooter(pdf));
        }

        [Fact]
        public void パースエラーで例外が発生する()
        {
            var exn = Assert.Throws<ReportsException>(() => {
                var pdf = Reports.Render("{,}");
            });
            Console.Error.WriteLine(exn.Message);
            Assert.NotEmpty(exn.Message);
        }

        [Fact]
        public void PDFデータを解析できる()
        {
            using (var fs = new FileStream("./files/mitumori.pdf", FileMode.Open, FileAccess.Read))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                var s = Reports.Parse(ms.ToArray());
                Assert.StartsWith("{", s.TrimStart());
                Assert.EndsWith("}", s.TrimEnd());
                var json = JsonDocument.Parse(s);
            }
        }
    }
}
