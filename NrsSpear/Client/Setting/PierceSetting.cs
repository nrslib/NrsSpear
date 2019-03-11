using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace NrsSpear.Client.Setting
{
    public class PierceSetting
    {
        public int Duration { get; set; }
        public string OutputPath { get; set; }

        public string Url { get; set; }
        public string Method { get; set; }
        public string[] Spears { get; set; }
        public string[] Targets { get; set; }

        public JObject Content { get; set; }
        public JObject Headers { get; set; }
        public JObject Cookie { get; set; }

        public HttpMethod HttpMethod
        {
            get
            {
                switch (Method.ToUpper())
                {
                    case "GET": return HttpMethod.Get;
                    case "POST": return HttpMethod.Post;
                    case "PUT": return HttpMethod.Put;
                    case "DELETE": return HttpMethod.Delete;
                    default: throw new ArgumentOutOfRangeException(nameof(Method), Method, null);
                }
            }
        }
    }
}
