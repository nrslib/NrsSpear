using System;
using System.IO;
using System.Threading.Tasks;
using NrsSpear.Client;
using NrsSpear.Client.Setting;

namespace NrsSpear.Presenter
{
    public class ConsolePiercePresenter : IPiercePresenter
    {
        public void Handle(DateTime time, string target, SpearTask task, PierceSetting setting, int count)
        {
            var directoryPath = Path.Combine(setting.OutputPath, "Result_" + time.ToString("yyyyMMddHHmmss"), target);
            var directoryFullPath = Path.Combine(directoryPath, task.Setting.SpearName);
            if (!Directory.Exists(directoryFullPath))
            {
                Directory.CreateDirectory(directoryFullPath);
            }

            var fileFullPath = Path.Combine(directoryFullPath, count + ".txt");
            Output(fileFullPath, task);
        }

        private void Output(string fileName, SpearTask task)
        {
            var request = task.Request;
            var response = task.Task.Result;
            var taskResponseContent = response.Content.ReadAsStringAsync();

            Task.WaitAll(taskResponseContent);

            var requestContent = task.Content.ToString();
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
