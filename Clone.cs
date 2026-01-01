using System.Text.RegularExpressions;

partial class Program
{
    /// <summary>github를 위한 좀더 간편한 clone.</summary>
    /// <param name="sourceUrl">github 저장소 url. 브랜치, 하위경로를 포함할 수 있습니다.</param>
    public async Task Clone([Argument] string sourceUrl)
    {
        var g = git | (Console.WriteLine, Console.Error.WriteLine);
        static void Cat(string url, object o) { Console.WriteLine(url); Console.WriteLine(o); }
        static Task Run(Command c) { Console.WriteLine(c); return c.ExecuteAsync(); }
        switch (0)
        {
            case 0 when Regex.Match(sourceUrl, @"^(https?://github.com/[^/]+/([^/]+))/(tree|blob)/([^/]+)/(.+)$") is
            { Groups: [_, { Value: { } url }, { Value: { } repo }, _, { Value: { } branch }, { Value: { } sparse }] }:
                Cat(url, new { repo, branch, sparse });
                await Run(g.WithArguments(["clone", "--branch", branch, "--sparse", url, repo]));
                await Run(g.WithArguments(["sparse-checkout", "set", sparse]).WithWorkingDirectory(repo));
                break;
            case 0 when Regex.Match(sourceUrl, @"^(https?://github.com/[^/]+/([^/]+))/(tree|blob)/([^/]+)$") is
            { Groups: [_, { Value: { } url }, { Value: { } repo }, _, { Value: { } branch }] }:
                Cat(url, new { repo, branch });
                await Run(g.WithArguments(["clone", "--branch", branch, url, repo]));
                break;
            case 0 when Regex.Match(sourceUrl, @"^(https?://github.com/[^/]+/([^/]+))$") is
            { Groups: [_, { Value: { } url }, { Value: { } repo }] }:
                Cat(url, new { repo });
                await Run(g.WithArguments(["clone", url, repo]));
                break;
            default:
                Console.WriteLine("clone ignored : 깃허브url만 됩니당.");
                break;
        }
    }
}