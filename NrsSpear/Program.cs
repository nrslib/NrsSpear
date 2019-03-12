using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using NrsSpear.AppSetting;
using NrsSpear.Client;
using NrsSpear.Client.Setting;
using NrsSpear.Presenter;

namespace NrsSpear
{


    class Program
    {
        private const string RecentSettingFileName = "NrsSpear.json";

        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = OnRemoteCertificateValidationCallback;

            var recentSetting = GetRecentSetting();

            Console.WriteLine("Welcome to NrsSpear");
            Console.WriteLine("");

            var spearDirectoryFullPath = PromptSpearDirectory(recentSetting);
            var pierceFileFullPath = PromptPierceFile(recentSetting);

            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("spear setting directory:");
            Console.WriteLine("- " + spearDirectoryFullPath);
            Console.WriteLine("spear piearce file:");
            Console.WriteLine("- " + pierceFileFullPath);
            Console.WriteLine("---------------------------------------------");

            recentSetting.SpearDirectory = spearDirectoryFullPath;
            recentSetting.PierceFile = pierceFileFullPath;
            File.WriteAllText(RecentSettingFileName, JsonConvert.SerializeObject(recentSetting));

            Console.WriteLine("running...");

            var spearFiles = Directory.GetFiles(spearDirectoryFullPath).Where(
                x => Path.GetExtension(x) == ".spear"
            );
            var spearSettings = spearFiles.AsParallel()
                .Select(x => new SpearSetting(x))
                .ToArray();

            var spearClient = new SpearClient(new ConsolePiercePresenter(), spearSettings);

            var pierceFile = File.ReadAllText(pierceFileFullPath);
            var setting = JsonConvert.DeserializeObject<PierceSetting>(pierceFile);
            spearClient.Pierce(setting);

            Console.WriteLine("done...");
            Console.WriteLine();
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }

        private static RecentSetting GetRecentSetting()
        {
            var filePath = RecentSettingFileName;
            if (File.Exists(filePath))
            {
                var texts = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<RecentSetting>(texts);
            }
            else
            {
                return new RecentSetting();
            }
        }

        private static string PromptSpearDirectory(RecentSetting recentSetting)
        {
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("set up spear");
            Console.WriteLine("---------------------------------------------");

            if (recentSetting?.SpearDirectory is var directoryPath && Directory.Exists(directoryPath))
            {
                var yes = PromptYesNo("current directory is " + directoryPath, "would you want to change directory?", "n");
                if (!yes)
                {
                    return directoryPath;
                }
            }

            inputSpearDirectory:
            Console.WriteLine("type your setting directory full path:");
            Console.Write("> ");
            var settingFileDirectoryFullPath = Console.ReadLine();
            if (!Directory.Exists(settingFileDirectoryFullPath))
            {
                Console.WriteLine("not found.");
                goto inputSpearDirectory;
            }

            return settingFileDirectoryFullPath;
        }


        private static string PromptPierceFile(RecentSetting recentSetting)
        {
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine("set up pierce");
            Console.WriteLine("---------------------------------------------");

            if (recentSetting?.PierceFile is var filePath && File.Exists(filePath))
            {
                var yes = PromptYesNo("current file is " + filePath, "would you want to change?", "n");
                if (!yes)
                {
                    return filePath;
                }
            }

            inputSpearDirectory:
            Console.WriteLine("type your pierce file full path:");
            Console.Write("> ");
            var settingFileDirectoryFullPath = Console.ReadLine();
            if (!File.Exists(settingFileDirectoryFullPath))
            {
                Console.WriteLine("not found.");
                goto inputSpearDirectory;
            }

            return settingFileDirectoryFullPath;
        }

        private static bool PromptYesNo(string currentMessage, string changeMessage, string defaultVal = null)
        {
            selectAction:
            if (currentMessage != null)
            {
                Console.WriteLine(currentMessage);
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(changeMessage);
            Console.ResetColor();
            Console.WriteLine("(y/n)");
            if (defaultVal != null)
            {
                Console.Write("[" + defaultVal + "] ");
            }
            Console.Write("> ");
            var input = Console.ReadLine();
            switch (input.ToLower())
            {
                case "y":
                    return true;
                case "n":
                    return false;
                case "":
                    switch (defaultVal)
                    {
                        case "y":
                            return true;
                        case "n":
                            return false;
                        default: goto selectAction;
                    }
                default: goto selectAction;
            }
        }

        private static bool OnRemoteCertificateValidationCallback(
            Object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
