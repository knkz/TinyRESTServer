using System;
using System.Net;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TinyRESTServer
{
    public class HttpServer
    {
        private HttpListener _listener;

        private readonly HttpServerConfigEntry _config;

        private readonly Subject<string> _logStream = new Subject<string>();
        
        private readonly Subject<HttpListenerContext> _httpRequestStream = new Subject<HttpListenerContext>();

        /// <summary>
        /// 受信したリクエストが流れる IObservable を取得します。
        /// </summary>
        public IObservable<HttpListenerContext> HttpRequestStream => _httpRequestStream;

        /// <summary>
        /// ログが流れる IObservable を取得します。
        /// </summary>
        public IObservable<string> LogStream => _logStream;

        /// <summary>
        /// 新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="config"></param>
        public HttpServer(HttpServerConfigEntry config)
        {
            _config = config;

            if (string.IsNullOrEmpty(_config.Hostname) == true)
            {
                // ホスト名が指定されていなければすべてのホスト名を受け入れるようにする。
                _config.Hostname = "+";
            }
        }

        /// <summary>
        /// HTTP サービスを開始します。
        /// </summary>
        /// <returns></returns>
        public void Start()
        {
            WriteLog("Start HTTP Server...");

            _listener = new HttpListener();

            string tail = string.Empty;
            if (string.IsNullOrEmpty(_config.BasePath) != true)
            {
                tail = "/";
            }

            _listener.Prefixes.Add($"http://{_config.Hostname}:{_config.PortNumber}/{_config.BasePath}{tail}");
            try
            {
                _listener.Start();
            }
            catch (Exception e)
            {
                WriteLog($"{e.Message}", LogLevel.Error, e);
            }

            ServiceTask(_listener).ContinueWith(t =>
            {
                if (t.Exception == null)
                {
                    // 前の処理が正常終了なら何もしない。
                    return;
                }
                WriteLog($"{t.Exception?.Message}", LogLevel.Error, t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// HTTP サービスを停止します。
        /// </summary>
        public void Stop()
        {
            WriteLog("Stop HTTP Server...");

            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch (Exception e)
            {
                WriteLog($"{e.Message}", LogLevel.Error, e);
            }
        }

        private async Task ServiceTask(HttpListener listener)
        {
            try
            {
                while (listener.IsListening == true)
                {
                    HttpListenerContext context = await listener.GetContextAsync();

                    if (listener.IsListening == false)
                    {
                        // 接続待ち受け中に listener が閉じられた。
                        break;
                    }

                    try
                    {
                        if (_config.AllowCORS == true)
                        {
                            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                        }

                        // プリフライトリクエストには、OK を返す。
                        if (context.Request.HttpMethod == "OPTIONS")
                        {
                            if (_config.AllowCORS == true)
                            {
                                context.Response.AddHeader("Access-Control-Allow-Headers",
                                    "Content-Type, Accept, X-Requested-With");
                                context.Response.AddHeader("Access-Control-Allow-Methods",
                                    "GET, POST, PUT, DELETE, OPTIONS");
                                context.Response.AddHeader("Access-Control-Max-Age", "1728000");
                                context.Response.StatusCode = (int) HttpStatusCode.NoContent;
                            }
                            else
                            {
                                context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                            }

                            continue;
                        }

                        WriteLog($"Request from {context.Request.RemoteEndPoint}");
                        _httpRequestStream.OnNext(context);
                    }
                    catch (Exception e)
                    {
                        WriteLog($"{e.Message}", LogLevel.Error, e);
                    }
                    finally
                    {
                        context.Response.Close();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // HTTP Listener が閉じられた。
                // NOP
            }
            catch (Exception e)
            {
                WriteLog($"{e.Message}", LogLevel.Error, e);
            }
        }

        private void WriteLog(string message, LogLevel logLevel = LogLevel.Information, Exception ex = null)
        {
            string log = $"{DateTime.Now} [{logLevel}]\t {message}";
            if (ex != null)
            {
                log += $"\n{ex}";
            }
            _logStream.OnNext(log);
        }
    }
}
