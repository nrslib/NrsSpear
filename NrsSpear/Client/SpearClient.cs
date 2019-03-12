using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using Newtonsoft.Json.Linq;
using NrsSpear.Client.Setting;
using NrsSpear.Presenter;

namespace NrsSpear.Client
{
    public class SpearClient
    {
        private readonly IPiercePresenter presenter;

        public SpearClient(
            IPiercePresenter presenter,
            SpearSetting[] spearSettings = null
            )
        {
            this.presenter = presenter;
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
            var client = GenerateHttpClient(setting);
            var tasks = GenerateTasks(target, setting);
            var executeTime = DateTime.Now;

            foreach (var tpl in tasks.Select((task, index) => new {task, index = index + 1}))
            {
                RunTask(client, executeTime, target, setting, tpl.task, tpl.index);
            }

            foreach (var task in tasks)
            {
                task.Dispose();
            }
        }

        private HttpClient GenerateHttpClient(PierceSetting setting)
        {
            var cookieContainer = new CookieContainer();
            foreach (var cookie in setting.Cookie)
            {
                cookieContainer.SetCookies(new Uri(setting.Url), cookie.Key + "=" + cookie.Value);
            }

            HttpClientHandler handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer
            };

            if (!string.IsNullOrEmpty(setting.Proxy))
            {
                handler.Proxy = new WebProxy(setting.Proxy);
            }

            var client = new HttpClient(handler);

            return client;
        }

        private List<SpearTask> GenerateTasks(string target, PierceSetting setting)
        {
            var tasks = new List<SpearTask>();
            foreach (var tpl in SpearSettings
                .Where(x => setting.Spears.Contains(x.SpearName))
                .SelectMany(x => x.Parameters.Select(param => Tuple.Create(x, param)))
            )
            {
                var spearSetting = tpl.Item1;
                var param = tpl.Item2;

                var body = (JObject) setting.Content.DeepClone();
                var parameter = spearSetting.Mode == Mode.Append
                    ? body[target] + param
                    : param;
                body[target] = parameter;
                var request = CreateRequestMessage(setting);
                var requestContent = RegisterContent(setting, request, body);
                tasks.Add(new SpearTask(spearSetting, param, request, requestContent));
            }

            return tasks;
        }

        private HttpRequestMessage CreateRequestMessage(PierceSetting setting)
        {
            var request = new HttpRequestMessage(setting.HttpMethod, setting.Url);
            foreach (var headerKv in setting.Headers)
            {
                request.Headers.Add(headerKv.Key, headerKv.Value.ToString());
            }

            return request;
        }

        private string RegisterContent(PierceSetting setting, HttpRequestMessage request, JObject body)
        {
            string requestContent;
            if (request.Method.Equals(HttpMethod.Get))
            {
                var queryString = CreateQueryParameter(body);
                request.RequestUri = new Uri(request.RequestUri, "?" + queryString);
                requestContent = queryString;
            }
            else if(setting.ContentType != "json")
            {
                var queryString = CreateQueryParameter(body);
                request.Content = new StringContent(queryString);
                requestContent = queryString;
            }
            else
            {
                request.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
                requestContent = body.ToString();
            }

            return requestContent;
        }

        private string CreateQueryParameter(JObject body)
        {
            var queries = body.Children<JProperty>().Select(prop => prop.Name + "=" + HttpUtility.UrlEncode(prop.Value.ToString()));
            var queryString = string.Join("&", queries);
            return queryString;
        }

        private void RunTask(HttpClient client, DateTime executeTime, string target, PierceSetting setting, SpearTask task, int index)
        {
            Thread.Sleep(setting.Duration);
            task.Run(client);
            task.Task.Wait();
            presenter.Handle(executeTime, target, task, setting, index);
        }
    }
}
