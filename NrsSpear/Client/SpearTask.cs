using System;
using System.Net.Http;
using System.Threading.Tasks;
using NrsSpear.Client.Setting;

namespace NrsSpear.Client
{
    public class SpearTask : IDisposable
    {
        public SpearTask(SpearSetting setting, string spearParameter, HttpRequestMessage request, object content)
        {
            Setting = setting;
            SpearParameter = spearParameter;
            Request = request;
            Content = content;
        }

        public SpearSetting Setting { get; }
        public string SpearParameter { get; }
        public HttpRequestMessage Request { get; }
        public Task<HttpResponseMessage> Task { get; private set; }
        public object Content { get; }

        public void Run(HttpClient client) {
            Task = client.SendAsync(Request);
        }

        public void Dispose()
        {
            Request?.Dispose();
        }
    }
}
