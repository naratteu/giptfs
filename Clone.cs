
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
        const string
            GH = "github.com",
            WGH = "www.github.com",
            GGH = "gist.github.com"
        string url;
        switch (new Uri(sourceUrl))
        {
            case { Authority: GH or WGH, Segments: ["/", _, var repo, "tree/" or "blob/", var branch, _, ..] sgmts } uri:
                url = new Uri(uri, string.Concat(sgmts.Take(3))).OriginalString; // Fragment 도 제거됨.
                branch = branch.TrimEnd('/');
                var sparse = string.Concat(sgmts.Skip(5));
                Cat(url, new { repo, branch, sparse });
                await Run(g.WithArguments(["clone", "--branch", branch, "--sparse", url, repo]));
                await Run(g.WithArguments(["sparse-checkout", "set", sparse]).WithWorkingDirectory(repo));
                return repo;
            case { Authority: GH or WGH, Segments: ["/", _, var repo, "tree/" or "blob/", var branch] sgmts } uri:
                url = new Uri(uri, string.Concat(sgmts.Take(3))).OriginalString; // Fragment 도 제거됨.
                branch = branch.TrimEnd('/');
                Cat(url, new { repo, branch });
                await Run(g.WithArguments(["clone", "--branch", branch, url, repo]));
                return repo;
            case { Authority: GGH, Segments: ["/", _, var repo, var revision] } uri:
                throw new NotImplementedException("gist의 revision 을 어떻게 처리할지 아직 안만들엇어요");
            case { Authority: GH or WGH or GGH, Segments: ["/", _, var repo] } uri: return await Default(uri, repo);
            case { Segments: [.., var repo] } uri: return await Default(uri, repo);
            async Task<string> Default(Uri uri, string repo)
            {
                url = uri.OriginalString[..^uri.Fragment.Length]; // Fragment 제거.
                Cat(url, new { repo });
                await Run(g.WithArguments(["clone", url, repo]));
                return repo;
            }
            default:
                throw new NotImplementedException("clone ignored : 깃허브나 유효한 url이 아니네요.");
        }
    }
}