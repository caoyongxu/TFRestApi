using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;

using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

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
    internal class interactAzDOHelper
    {
        static WorkItemTrackingHttpClient WitClient;
        static BuildHttpClient BuildClient;
        static ProjectHttpClient ProjectClient;
        static TeamHttpClient TeamClient;
        static GitHttpClient GitClient;
        static TfvcHttpClient TfvsClient;
        static TestManagementHttpClient TestManagementClient;

        /// <summary>
        /// Get all teams
        /// </summary>
        /// <param name="TeamProjectName"></param>
        public static RepoTree PullDataFromOrg(string org, string pat)
        {
            string TFUrl = $"https://dev.azure.com/{org}/";
            ConnectWithPAT(TFUrl, pat);

            RepoTree data = new RepoTree() { Organiztion = org };

            IPagedList<TeamProjectReference> projects = ProjectClient.GetProjects().Result;
            foreach (TeamProjectReference project in projects)
            {
                //if (project.Name == "Courseware")
                //    continue;

                TeamProjectInfo tpi = new TeamProjectInfo() { TeamProjectName = project.Name };
                data.TeamProjects.Add(tpi);

                TeamProject pro = ProjectClient.GetProject(project.Id.ToString()).Result;
                List<GitRepository> repos = GitClient.GetRepositoriesAsync(pro.Name).Result;                

                foreach (var r in repos)
                {
                    RepoInfo ri = new RepoInfo();
                    ri.RepoName = r.Name;
                    tpi.Repos.Add(ri);

                    CheckDisableStatusAndEnableItIfNeed(org, pro.Name, r.Id.ToString(), pat);
                    List<GitRef> branches = GitClient.GetBranchRefsAsync(r.Id).Result;

                    foreach (var b in branches)
                    {
                        BranchInfo bi = new BranchInfo();
                        ri.Branches.Add(bi);
                        bi.BranchName = b.Name.Replace("refs/heads/","");
                        bi.IsNameContainsMaster = b.Name.Contains("master");
                        Console.WriteLine(b.Name);
                    }
                    ri.IsContainsMasterBranch = ri.Branches.Any(o => o.IsNameContainsMaster);
                }
                //break;
            }
            return data;
        }

        //https://docs.microsoft.com/en-us/rest/api/azure/devops/git/repositories/update?view=azure-devops-rest-7.1
        public static async void GetProjectsByWebAPI(string org, string pat)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", pat))));

                    using (HttpResponseMessage response = client.GetAsync(
                                $"https://dev.azure.com/{org}/_apis/projects").Result)
                    {
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseBody);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async void CheckDisableStatusAndEnableItIfNeed(string org, string projectName, string repoID, string pat)
        {
            try
            {
                string url = $"https://dev.azure.com/{org}/{projectName}/_apis/git/repositories/{repoID}?api-version=7.1-preview.1";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", pat))));

                    bool notfound = false;
                    using (HttpResponseMessage response = client.GetAsync(url).Result)
                    {
                        //response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            notfound = true;
                        }
                        else
                        {
                            Console.WriteLine(responseBody);
                        }
                    }
                    if (notfound)
                    {
                        var data = new { isDisabled = false };
                        HttpContent body = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                        using (HttpResponseMessage response = client.PatchAsync(url, body).Result)
                        {
                            response.EnsureSuccessStatusCode();
                            string responseBody = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(responseBody);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        #region create new connections
        static void InitClients(VssConnection Connection)
        {
            WitClient = Connection.GetClient<WorkItemTrackingHttpClient>();
            BuildClient = Connection.GetClient<BuildHttpClient>();
            ProjectClient = Connection.GetClient<ProjectHttpClient>();
            TeamClient = Connection.GetClient<TeamHttpClient>();
            GitClient = Connection.GetClient<GitHttpClient>();
            TfvsClient = Connection.GetClient<TfvcHttpClient>();
            TestManagementClient = Connection.GetClient<TestManagementHttpClient>();
        }

        static void ConnectWithPAT(string ServiceURL, string PAT)
        {
            VssConnection connection = new VssConnection(new Uri(ServiceURL), new VssBasicCredential(string.Empty, PAT));
            InitClients(connection);
        }
        #endregion
    }
}
