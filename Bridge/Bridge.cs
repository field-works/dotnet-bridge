using System;
using System.IO;
using System.Net.Http;
using System.Web;

namespace FieldWorks.FieldReports
{
    /// <summary>
    /// .NET Bridge for Field Reports
    ///
    /// Field Reportsと連携するためのProxyオブジェクトを生成します。
    /// 
    /// </summary>
    public static class Bridge
    {
        /// <summary>
        /// 引数で与えられるURIに応じたField Reports Proxyオブジェクトを返却します。
        /// 
        ///     // コマンド連携時:
        ///     Proxy reports = Bridge.createProxy("exec:/usr/local/bin/reports?cwd=/usr/share&logleve=3");
        /// 
        ///     // HTTP連携時:
        ///     Proxy reports = Bridge.createProxy("http://localhost:50080/");
        /// </summary>
        /// <param name="uriString">
        /// Field Reportsとの接続方法を示すURI。<br>
        /// nullを指定または省略した場合，環境変数'REPORTS_PROXY'からURIを取得します。<br>
        /// 環境変数'REPORTS_PROXY'も未設定の場合のURIは，"exec:reports"とします。
        /// </param>
        /// <remarks>
        ///  URI書式（コマンド連携時）:
        /// 
        ///      exec:{exePath}?cwd={cwd}&loglevel={logLevel}
        ///
        ///   - cwd, loglevelは省略可能です。
        ///   - loglevelが0より大きい場合，STDERRにログを出力します。
        /// 
        ///  URI書式（HTTP連携時）:
        /// 
        ///      http://{hostName}:{portNumber}/
        /// </remarks>
        /// <returns>Field Reports Proxyオブジェクト</returns>
        public static IProxy CreateProxy(string uriString = null)
        {
            var uri = new Uri(uriString ?? Environment.GetEnvironmentVariable("REPORTS_PROXY") ?? "exec:reports");
            if (uri.Scheme == "exec")
            {
                var q = HttpUtility.ParseQueryString(uri.Query);
                var exePath = uri.AbsolutePath;
                var cwd = q["cwd"] ?? ".";
                int logLevel;
                Int32.TryParse(q["loglevel"], out logLevel);
                return CreateExecProxy(exePath, cwd, logLevel, null);
            }
            return CreateHttpProxy(uriString);
        }

        /// <summary>
        /// コマンド呼び出しによりField Reportsと連携するProxyオブジェクトを生成します。
        /// </summary>
        /// <param name="exePath">Field Reportsコマンドのパス</param>
        /// <param name="cwd">Field Reportsプロセス実行時のカレントディレクトリ</param>
        /// <param name="logLevel">ログ出力レベル（0: ログを出力しない，1: ERRORログ，2: WARNログ，3: INFOログ，4: DEBUGログ）</param>
        /// <param name="logWriter">ログ出力先Stream</param>
        /// <returns>Field Reports Proxyオブジェクト</returns>
        public static IProxy CreateExecProxy(
            string exePath = "reports", string cwd = ".",
            int logLevel = 0, TextWriter logWriter = null)
        {
            return new ExecProxy(exePath, cwd, logLevel, logWriter ?? Console.Error);
        }

        /// <summary>
        /// HTTP通信によりField Reportsと連携するProxyオブジェクトを生成します。
        /// </summary>
        /// <param name="baseUri">ベースUri</param>
        /// <returns>Field Reports Proxyオブジェクト</returns> 
        public static IProxy CreateHttpProxy(string baseUri = "http://localhost:50080/")
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(baseUri)
            };
            return new HttpProxy(httpClient);
        }

        /// <summary>
        /// HTTP通信によりField Reportsと連携するProxyオブジェクトを生成します。
        /// </summary>
        /// <remarks>
        /// 引数で指定したHttpClientを利用して，HTTP通信を行います。
        /// httpClientには，あらかじめBaseAddressを設定しておいてください。
        /// </remarks>
        /// <param name="httpClient">HttpClientインスタンス</param>
        /// <returns>Field Reports Proxyオブジェクト</returns> 
        public static IProxy CreateHttpProxy(HttpClient httpClient)
        {
            return new HttpProxy(httpClient);
        }
    }
}