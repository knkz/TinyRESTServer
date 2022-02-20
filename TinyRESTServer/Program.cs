using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace TinyRESTServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new HttpServer(new HttpServerConfigEntry()
            {
                PortNumber = 8080,
                BasePath = "api",
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
                ResponseEntity res = new ResponseEntity() {IntParam = 5, StringParam = "Hoge"};
                byte[] response = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(res));
                StreamWriter writer = new StreamWriter(context.Response.OutputStream);
                writer.BaseStream.Write(response, 0, response.Length);
                writer.Flush();

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
