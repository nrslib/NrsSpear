using System;
using System.Net.Http;
using System.Threading.Tasks;
using NrsSpear.Client.Setting;

namespace NrsSpear.Client
{
    internal class SpearTask : IDisposable
    {
        public SpearTask(SpearSetting setting, string spearParameter, HttpRequestMessage request, Task<HttpResponseMessage> task)
        {
            Setting = setting;
            SpearParameter = spearParameter;
            Request = request;
            Task = task;
        }

        public SpearSetting Setting { get; }
        public string SpearParameter { get; }
        public HttpRequestMessage Request { get; }
        public Task<HttpResponseMessage> Task { get; }

        public void Dispose()
        {
            Request?.Dispose();
        }
    }
}
