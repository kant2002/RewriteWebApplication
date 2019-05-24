using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RewriteWebApplication.Tests
{
    [TestClass]
    public class RewriteRulesTests : IDisposable
    {
        private Process iisProcess;
        public RewriteRulesTests()
        {
            var applicationPath = GetApplicationPath();
            var applicationPort = 8080;
            var iisProcess = StartIISExpressProcess(applicationPath, applicationPort);
            this.iisProcess = iisProcess;
        }

        [TestMethod]
        public async Task AssureThatPlusHandledTransparently()
        {
            using (var client = CreateHttpClient())
            {
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/home/about/5+"));
                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            }
        }

        [TestMethod]
        public async Task AssureThatUrlWithoutPlusHandledAsPreviously()
        {
            using (var client = CreateHttpClient())
            {
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/home/about/5"));
                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
            };
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:8080"),
            };
            return httpClient;
        }

        private static string GetApplicationPath()
        {
            var currentDirectory = GetCurrentDirectory();
            var path = Path.Combine(currentDirectory, "../../../../RewriteWebApplication");
            return Path.GetFullPath(path);
        }

        private static string GetCurrentDirectory()
        {
            var uriBuilder = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            var assemblyPath = Uri.UnescapeDataString(uriBuilder.Path);
            return Path.GetDirectoryName(assemblyPath);
        }


        private static Process StartIISExpressProcess(string applicationPath, int applicationPort)
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var iisProcess = new Process();
            iisProcess.StartInfo.FileName = programFiles + @"\IIS Express\iisexpress.exe";
            iisProcess.StartInfo.Arguments = string.Format("/path:\"{0}\" /port:{1}", applicationPath, applicationPort);
            iisProcess.Start();
            return iisProcess;
        }

        public void Dispose()
        {
            if (this.iisProcess != null)
            {
                this.iisProcess.Close();
            }

            this.iisProcess = null;
        }
    }
}
