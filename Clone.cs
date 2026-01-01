
partial class Program
{
    /// <summary>clone 후 즉시 vscode로 열어봅니다.</summary>
    /// <param name="repo">github 저장소 url. 브랜치, 하위경로 등을 포함할 수 있습니다.</param>
    public async Task Code([Argument] string repo, [Argument] string? dir = null)
    {
        repo = await Clone_(repo, dir);
        await code.WithArguments([repo]).ExecuteAsync();
    }

    /// <summary>github를 위한 좀더 간편한 clone.</summary>
    /// <param name="repo">github 저장소 url. 브랜치, 하위경로 등을 포함할 수 있습니다.</param>
    public Task Clone([Argument] string repo, [Argument] string? dir = null) => Clone_(repo, dir);
    async Task<string> Clone_(string sourceUrl, string? destDir)
    {
        var g = git | (Console.WriteLine, Console.Error.WriteLine);
        string url, dir;
        void Cat(object o) { Console.WriteLine(new { url, dir }); Console.WriteLine(o); }
        static Task Run(Command c) { Console.WriteLine(c); return c.ExecuteAsync(); }
        const string
            GH = "github.com",
            WGH = "www.github.com",
            GGH = "gist.github.com";
        switch (new Uri(sourceUrl))
        {
            case { Authority: GH or WGH, Segments: ["/", _, var repo, "tree/" or "blob/", var branch, _, ..] sgmts } uri:
                url = new Uri(uri, string.Concat(sgmts.Take(3))).OriginalString; // Fragment 도 제거됨.
                dir = destDir ?? repo;
                branch = branch.TrimEnd('/');
                var sparse = string.Concat(sgmts.Skip(5));
                Cat(new { repo, branch, sparse });
                await Run(g.WithArguments(["clone", "--branch", branch, "--sparse", url, dir]));
                await Run(g.WithArguments(["sparse-checkout", "set", sparse]).WithWorkingDirectory(dir));
                return dir;
            case { Authority: GH or WGH, Segments: ["/", _, var repo, "tree/" or "blob/", var branch] sgmts } uri:
                url = new Uri(uri, string.Concat(sgmts.Take(3))).OriginalString; // Fragment 도 제거됨.
                dir = destDir ?? repo;
                branch = branch.TrimEnd('/');
                Cat(new { repo, branch });
                await Run(g.WithArguments(["clone", "--branch", branch, url, dir]));
                return dir;
            case { Authority: GGH, Segments: ["/", _, var repo, var revision] } uri:
                throw new NotImplementedException("gist의 revision 을 어떻게 처리할지 아직 안만들엇어요");
            case { Authority: GH or WGH or GGH, Segments: ["/", _, var repo] } uri: return await Default(uri, repo);
            case { Segments: [.., var repo] } uri: return await Default(uri, repo);
            async Task<string> Default(Uri uri, string repo)
            {
                url = uri.OriginalString[..^uri.Fragment.Length]; // Fragment 제거.
                dir = destDir ?? repo;
                Cat(new { repo });
                await Run(g.WithArguments(["clone", url, dir]));
                return dir;
            }
            default:
                throw new NotImplementedException("clone ignored : 깃허브나 유효한 url이 아니네요.");
        }
    }
}