using System.Threading.Tasks;

namespace FieldWorks.FieldReports
{
    /// <summary>
    /// Field Reportsの機能を呼び出すためのProxyインターフェースです。
    /// 
    /// </summary>
    public interface IProxy
    {
        /// <summary>
        /// バージョン番号を取得します。
        /// </summary>
        /// <returns>バージョン番号</returns>
        /// <exception cref="ReportsException">Field Reportsとの連携に失敗した場合に発生</exception>
        string Version();

        /// <summary>
        /// レンダリング・パラメータを元にレンダリングを実行します。
        /// </summary>
        /// <param name="param">
        /// JSON文字列またはシリアライズ可能なオブジェクト形式レンダリング・パラメータ<br>
        /// ユーザーズ・マニュアル「第5章 レンダリングパラメータ」を参照してください。
        /// </param>
        /// <returns>PDFデータ</returns>
        /// <exception cref="ReportsException">Field Reportsとの連携に失敗した場合に発生</exception>
        byte[] Render(object param);

        /// <summary>
        /// レンダリング・パラメータを元にレンダリングを非同期で実行します。
        /// </summary>
        /// <param name="param">
        /// JSON文字列またはシリアライズ可能なオブジェクト形式レンダリング・パラメータ<br>
        /// ユーザーズ・マニュアル「第5章 レンダリングパラメータ」を参照してください。
        /// </param>
        /// <returns>PDFデータ</returns>
        /// <exception cref="ReportsException">Field Reportsとの連携に失敗した場合に発生</exception>
        Task<byte[]> RenderAsync(object param);

        /// <summary>
        /// PDFデータを解析し，フィールドや注釈の情報を取得します。
        /// </summary>
        /// <param name="pdf">PDFデータ</param>
        /// <returns>解析結果（JSON文字列形式）</returns>
        /// <exception cref="ReportsException">Field Reportsとの連携に失敗した場合に発生</exception>
        string Parse(byte[] pdf);

        /// <summary>
        /// PDFデータを非同期で解析し，フィールドや注釈の情報を取得します。
        /// </summary>
        /// <param name="pdf">PDFデータ</param>
        /// <returns>解析結果（JSON文字列形式）</returns>
        /// <exception cref="ReportsException">Field Reportsとの連携に失敗した場合に発生</exception>
        Task<string> ParseAsync(byte[] pdf);
    }
}