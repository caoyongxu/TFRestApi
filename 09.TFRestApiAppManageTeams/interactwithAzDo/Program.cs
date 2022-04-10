
using Microsoft.VisualStudio.Services.ClientNotification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace interactwithAzDo
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Pleae ensure assign two parameters: <organiztion> and <PAT>");
                return;
            }
            try
            {
                string UserPAT = args[1].Trim().Replace(":","").Replace("-","");
                string Organization = args[0].Trim();
                var data = interactAzDOHelper.PullDataFromOrg(Organization, UserPAT);

                var Header = $"org,project,repoName,hasMasterBranch?,Branches";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Header);

                foreach (var project in data.TeamProjects)
                {
                    foreach (var repo in project.Repos)
                    {
                        sb.Append($"{data.Organiztion},");
                        sb.Append($"{project.TeamProjectName},");
                        sb.Append($"{repo.RepoName},");
                        sb.Append($"{repo.IsContainsMasterBranch},");
                        sb.Append($"{string.Join<string>(";",from b in repo.Branches select b.BranchName)}");
                        sb.AppendLine();
                    }
                }
                if (!Directory.Exists("Data"))
                {
                    Directory.CreateDirectory("Data");
                }
                var fileName = $"Data/{Organization}-repos-branches-{DateTime.Now.Ticks}.csv";
                File.WriteAllText(fileName, sb.ToString(),Encoding.UTF8);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                if (ex.InnerException != null) Console.WriteLine("Detailed Info: " + ex.InnerException.Message);
                Console.WriteLine("Stack:\n" + ex.StackTrace);
            }
        }                
    }
}
