using System;
using System.IO;
using System.Net;
using System.Text;

namespace TinyRESTServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new HttpServer(new HttpServerConfigEntry()
            {
                Hostname = "localhost",
                PortNumber = 8080,
                BasePath = "api",
                UseSSL = false,
                AllowCORS = true
            });
            server.HttpRequestStream.Subscribe(OnRequested);
            server.LogStream.Subscribe(Console.WriteLine);
            server.Start();

            // 適当に何か入力されるまで待機
            Console.ReadLine();

            server.Stop();

            // 適当に何か入力されるまで待機
            Console.ReadLine();
        }

        static void OnRequested(HttpListenerContext context)
        {
            // 暫定
            try
            {
                byte[] response = Encoding.UTF8.GetBytes("にゃーん");
                StreamWriter writer = new StreamWriter(context.Response.OutputStream);
                writer.BaseStream.Write(response, 0, response.Length);
                writer.Flush();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
