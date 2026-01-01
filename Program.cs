global using CliWrap;
global using ConsoleAppFramework;
using CliWrap.EventStream;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;
await ConsoleApp.Create().RunAsync(args);
[RegisterCommands]
partial class Program
{
    /// <summary>
    /// `git`으로 관리하기 어려운 대용량 파일에 대해서 LFS대신 `ipfs`로 공유하거나 내려받습니다.
    /// 따라서 이 도구를 사용하기 위해선, 위의 두 명령줄 도구가 필요합니다.
    /// </summary>
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Giptfs))]
    [Command("")] public Task Root() => ConsoleApp.Create().RunAsync(["--help"]);

    record Giptfs { public required string ipfs, path; }

    /// <summary>git에서 관리하지 않는 파일을 IPFS에 추가하여 파일명과 해시를 조회합니다.</summary>
    public async Task Ls()
    {
        var yaml = new SerializerBuilder().Build();
        await foreach (var file in UntrackFiles().ListenAsync())
        {
            if (file is StandardOutputCommandEvent { Text: { } path })
            {
                var hash = await GetIpfsHash(path).ListenAsync().CastNotNull<StandardOutputCommandEvent>().SingleAsync();
                var obj = new Giptfs { ipfs = hash.Text, path = path };
                Console.Write(yaml.Serialize(new[] { obj }));
            }
        }
    }

    /// <summary>*.giptfs.yml 에 매핑된 파일을 모두 가져옵니다.</summary>
    public async Task Get()
    {
        var yaml = new DeserializerBuilder().Build();
        foreach (var read in Directory.GetFiles(".", "*.giptfs.yml"))
        {
            Console.WriteLine(new { read });
            foreach (var file in yaml.Deserialize<Giptfs[]>(File.ReadAllText(read)))
            {
                try
                {
                    var add = await GetIpfsHash(file.path).ListenAsync().ToArrayAsync();
                    if (add is [
                        StartedCommandEvent,
                        StandardOutputCommandEvent { Text: { } hash },
                        ExitedCommandEvent { ExitCode: 0 }] &&
                        hash == file.ipfs)
                    {
                        Console.WriteLine(new { skip = file.path });
                        continue;
                    }
                    else Console.WriteLine(new { file.path, msg = "파일 해시가 다릅니다." });
                }
                catch { }

                Console.Write($"{file.path} 를 가져오시겟습니까? [y/-]");
                if (Console.ReadLine() is "y")
                {
                    try
                    {
                        var dir = Path.GetDirectoryName(file.path);
                        if (string.IsNullOrWhiteSpace(dir) is false)
                            Directory.CreateDirectory(dir);
                        await GetIpfsFile(file).WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError())).ExecuteAsync();
                        Console.WriteLine($"\e[38;2;0;255;0m{new { get = file.path }}\e[0m");
                    }
                    catch
                    {
                        Console.WriteLine($"\e[38;2;255;0;0m{new { err = file.path }}\e[0m");
                    }
                }
                else
                {
                    Console.WriteLine(new { skip = file.path });
                }
            }
        }
    }

    /// <summary>*.giptfs.yml 에 매핑된 해시를 모두 Pin 합니다.</summary>
    /// <param name="rm">unpin</param>
    public async Task Pin(bool rm = false)
    {
        var yaml = new DeserializerBuilder().Build();
        var hashs = from read in Directory.GetFiles(".", "*.giptfs.yml")
                    from file in yaml.Deserialize<Giptfs[]>(File.ReadAllText(read))
                    select file.ipfs;
        await (ipfs.WithArguments(["pin", rm ? "rm" : "add", .. hashs]) | Console.WriteLine).ExecuteAsync();
    }

    Command UntrackFiles() => git.WithArguments("ls-files --others --exclude-standard");
    readonly Command
        git = new(nameof(git)),
        ipfs = new(nameof(ipfs));
    Command GetIpfsHash(string file) => ipfs.WithArguments(["add", "-q", file]);
    Command GetIpfsFile(Giptfs file) => ipfs.WithArguments(["get", file.ipfs, "-o", file.path]);
}

static class Exts
{
    public delegate bool TryFunc<T, TT>(T t, out TT tt);
    public static async IAsyncEnumerable<TT> SelectWhen<T, TT>(this IAsyncEnumerable<T> e, TryFunc<T, TT> conv)
    {
        await foreach (var t in e)
            if (conv(t, out var tt))
                yield return tt;
    }
    public static IAsyncEnumerable<TT> SelectNotNull<T, TT>(this IAsyncEnumerable<T> e, Func<T, TT?> conv)
        => e.SelectWhen((T t, out TT tt) => (tt = conv(t)!) is not null);
    public static IAsyncEnumerable<T> CastNotNull<T>(this IAsyncEnumerable<object> e) where T : class
        => e.SelectNotNull(t => t as T);
}