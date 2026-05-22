partial class Program
{
    /// <summary>마지막 커밋을 취소하고 변경사항을 스테이징 상태로 되돌립니다.</summary>
    public Task Undo()
    {
        var g = git | (Console.WriteLine, Console.Error.WriteLine);
        return g.WithArguments(["reset", "--soft", "HEAD~1"]).Enter();
    }
}