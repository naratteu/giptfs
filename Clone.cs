
partial class Program
{
    /// <summary>clone 후 즉시 vscode로 열어봅니다.</summary>
    /// <param name="sourceUrl">github 저장소 url. 브랜치, 하위경로를 포함할 수 있습니다.</param>
    public async Task Code([Argument] string sourceUrl)
    {
        var repo = await Clone_(sourceUrl);
        await code.WithArguments([repo]).ExecuteAsync();
    }

    /// <summary>github를 위한 좀더 간편한 clone.</summary>
    /// <param name="sourceUrl">github 저장소 url. 브랜치, 하위경로를 포함할 수 있습니다.</param>
    public async Task Clone([Argument] string sourceUrl)
         => await Clone_(sourceUrl);
    async Task<string> Clone_(string sourceUrl)
    {
        var g = git | (Console.WriteLine, Console.Error.WriteLine);
        static void Cat(string url, object o) { Console.WriteLine(url); Console.WriteLine(o); }
        static Task Run(Command c) { Console.WriteLine(c); return c.ExecuteAsync(); }
        string url;
        switch (new Uri(sourceUrl))
        {
            case { Authority: "github.com", Segments: ["/", _, var repo, "tree/" or "blob/", var branch, _, ..] } uri:
                url = new Uri(uri, string.Concat(uri.Segments.Take(3))).OriginalString;
                branch = branch.TrimEnd('/');
                var sparse = string.Concat(uri.Segments.Skip(5));
                Cat(url, new { repo, branch, sparse });
                await Run(g.WithArguments(["clone", "--branch", branch, "--sparse", url, repo]));
                await Run(g.WithArguments(["sparse-checkout", "set", sparse]).WithWorkingDirectory(repo));
                return repo;
            case { Authority: "github.com", Segments: ["/", _, var repo, "tree/" or "blob/", var branch] } uri:
                url = new Uri(uri, string.Concat(uri.Segments.Take(3))).OriginalString;
                branch = branch.TrimEnd('/');
                Cat(url, new { repo, branch });
                await Run(g.WithArguments(["clone", "--branch", branch, url, repo]));
                return repo;
            case { Authority: "github.com", Segments: ["/", _, var repo] } uri:
                url = uri.OriginalString;
                Cat(url, new { repo });
                await Run(g.WithArguments(["clone", url, repo]));
                return repo;
            default:
                throw new NotImplementedException("clone ignored : 깃허브url만 됩니당.");
        }
    }
}