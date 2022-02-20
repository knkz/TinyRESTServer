namespace TinyRESTServer
{
    public class HttpServerConfigEntry
    {
        /// <summary>
        /// ホスト名を取得または設定します。
        /// </summary>
        public string Hostname { get; set; }
        /// <summary>
        /// 使用するポート番号を取得または設定します。
        /// </summary>
        public int PortNumber { get; set; }
        /// <summary>
        /// URL の BasePath を取得または設定します。
        /// </summary>
        public string BasePath { get; set; }
        /// <summary>
        /// CORS を有効にするかどうかを取得または設定します。
        /// </summary>
        public bool AllowCORS { get; set; }
    }
}
