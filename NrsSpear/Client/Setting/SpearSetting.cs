using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NrsSpear.Client.Setting
{
    public class SpearSetting
    {
        public SpearSetting(string fileFullPath)
        {
            SpearName = Path.GetFileNameWithoutExtension(fileFullPath);
            var lines = File.ReadAllLines(fileFullPath);

            var header = lines.TakeWhile(x => x != "-----");

            var modeRow = lines.FirstOrDefault(x => x.ToLower().StartsWith("mode:"));
            var modeString = modeRow.Split(':')[1].Trim();
            if (Enum.TryParse(modeString, out Mode mode))
            {
                Mode = mode;
            }

            var body = lines.Skip(header.Count() + 1);
            Parameters = body.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        }

        public SpearSetting(string spearName, Mode mode, IEnumerable<string> parameters)
        {
            SpearName = spearName;
            Mode = mode;
            Parameters = parameters.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        }

        public string SpearName { get; }
        public Mode Mode { get; }
        public string[] Parameters { get; }
    }

    public enum Mode
    {
        Replace,
        Append
    }
}
