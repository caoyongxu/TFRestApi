using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace interactwithAzDo
{

    public class RepoTree
    {
        public string Organiztion { get; set; }
        public List<TeamProjectInfo> TeamProjects { get; set; }
            = new List<TeamProjectInfo>();

    }

    public class TeamProjectInfo
    {

        public string TeamProjectName { get; set; }
        public List<RepoInfo> Repos { get; set; }
            = new List<RepoInfo>();
    }

    public class RepoInfo
    {
        public string RepoName { get; set; }
        public List<BranchInfo> Branches { get; set; }
            = new List<BranchInfo>();
        public bool IsContainsMasterBranch { get; set; }
    }

    public class BranchInfo
    {
        public string BranchName { get; set; }
        public bool IsNameContainsMaster { get; set; }
    }
}
