using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using NrsSpear.Client.Setting;

namespace NrsSpear.Client
{
    public class SpearClient
    {
        private readonly HttpClient client;

        public SpearClient(HttpClient client, SpearSetting[] spearSettings = null)
        {
            this.client = client;
            SpearSettings = spearSettings ?? new SpearSetting[] { };
        }

        public SpearSetting[] SpearSettings { get; set; }

        public void Pierce(PierceSetting setting)
        {
            foreach (var target in setting.Targets)
            {
                Run(target, setting);
            }
        }

        private void Run(string target, PierceSetting setting)
        {
            var tasks = new List<SpearTask>();

            foreach (var (spearSetting, param) in SpearSettings.SelectMany(x => x.Parameters.Select(param => (SpearSetting: x, SpearParam: param))))
            {
                Thread.Sleep(setting.Duration);
                var body = (JObject) setting.Content.DeepClone();
                var parameter = spearSetting.Mode == Mode.Append
                    ? body[target] + param
                    : param;
                body[target] = parameter;
                var request = CreateRequestMessage(setting);
                if (request.Method.Equals(HttpMethod.Get))
                {
                    var queries = body.Children<JProperty>().Select(prop => prop.Name + "=" + HttpUtility.UrlEncode(prop.Value.ToString()));
                    var queryString = string.Join("&", queries);
                    request.RequestUri = new Uri(request.RequestUri, "?" + queryString);
                }
                else
                {
                    request.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
                }
                tasks.Add(new SpearTask(spearSetting, param, request, client.SendAsync(request)));
            }

            Task.WaitAll(tasks.Select(x => x.Task).ToArray());
            Output(target, tasks, setting);
            foreach (var task in tasks)
            {
                task.Dispose();
            }
        }

        private string GenerateCookieString(JObject cookieObject)
        {
            var result = new List<string>();
            foreach (var keyValuePair in cookieObject)
            {
                result.Add(keyValuePair.Key + "=" + keyValuePair.Value);
            }

            return string.Join("; ", result);
        }

        private HttpRequestMessage CreateRequestMessage(PierceSetting setting)
        {
            var request = new HttpRequestMessage(setting.HttpMethod, setting.Url);
            foreach (var headerKv in setting.Headers)
            {
                request.Headers.Add(headerKv.Key, headerKv.Value.ToString());
            }
            var cookieString = GenerateCookieString(setting.Cookie);
            request.Headers.Add("Cookie", cookieString);

            return request;
        }

        private void Output(string target, IEnumerable<SpearTask> tasks, PierceSetting setting)
        {
            var directoryPath = Path.Combine(setting.OutputPath, "Result", target);
            foreach (var directory in tasks
                .Select(x => Path.Combine(directoryPath, x.Setting.SpearName))
                .Distinct()
                .Where(x => !Directory.Exists(x)))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (var (spearTask, index) in tasks.Select((task, index) => (task, index + 1)))
            {
                Output(Path.Combine(directoryPath, spearTask.Setting.SpearName, index + ".txt"), spearTask);
            }
        }

        private void Output(string fileName, SpearTask task)
        {
            var request = task.Request;
            var response = task.Task.Result;

            var taskRequestContent = request.Content?.ReadAsStringAsync();
            var taskResponseContent = response.Content.ReadAsStringAsync();

            if (taskRequestContent != null)
            {
                Task.WaitAll(taskRequestContent, taskResponseContent);
            }
            else
            {
                Task.WaitAll(taskResponseContent);
            }

            var requestContent = taskRequestContent?.Result ?? "";
            var responseContent = taskResponseContent.Result;
            var texts = new[]
            {
                "# Request",
                "",
                request.ToString(),
                "",
                "## Content",
                requestContent,
                "",
                "-----",
                "",
                "# Response",
                "",
                response.ToString(),
                "",
                "## Content",
                responseContent
            };

            var text = string.Join(Environment.NewLine, texts);
            File.WriteAllText(fileName, text);
        }
    }
}
