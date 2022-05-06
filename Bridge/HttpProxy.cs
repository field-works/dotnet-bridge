using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace FieldWorks.FieldReports
{
    internal class HttpProxy : IProxy
    {
        private HttpClient HttpClient { get; }

        public HttpProxy(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        private async Task<string> VersionAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "version");
                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(await ResponseMessageAsync(response));
                }
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception exn)
            {
                throw new ReportsException("Fail to HTTP coomunication: ", exn); 
            }
        }

        public string Version()
        {
            try
            {
                return VersionAsync().Result;
            }
            catch (AggregateException exn)
            {
                throw exn.InnerException;
            }
        }

        public async Task<byte[]> RenderAsync(object param)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "render");
                var jstring = (param is string) ? (string)param : JsonSerializer.Serialize(param);
                request.Content = new StringContent(jstring, Encoding.UTF8, "application/json");;
                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(await ResponseMessageAsync(response));
                }
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception exn)
            {
                throw CreateReportsException(exn); 
            }
        }

        public byte[] Render(object param)
        {
            try
            {
                return RenderAsync(param).Result;
            }
            catch (AggregateException exn)
            {
                throw exn.InnerException;
            }
        }

        public async Task<string> ParseAsync(byte[] pdf)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "parse");
                request.Content = new ByteArrayContent(pdf);
                request.Content.Headers.Add("Content-Type", "application/pdf");
                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception(await ResponseMessageAsync(response));
                }
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception exn)
            {
                throw new ReportsException("Fail to HTTP coomunication: ", exn); 
            }
        }

        public string Parse(byte[] pdf)
        {
            try
            {
                return ParseAsync(pdf).Result;
            }
            catch (AggregateException exn)
            {
                throw exn.InnerException;
            }
        }

        private async Task<String> ResponseMessageAsync(HttpResponseMessage res)
        {
            var body = await res.Content.ReadAsStringAsync();
            return $"Status Code = {(int)res.StatusCode}, Reason = {res.ReasonPhrase}, Response = {body}"; 
        }

        private Exception CreateReportsException(Exception exn)
        {
            return new ReportsException($"Fail to HTTP coomunication: {exn.Message}.", exn); 
        }
    }
}
