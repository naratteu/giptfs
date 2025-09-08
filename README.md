# git + ipfs 파일관리전략

`git`으로 관리하기 어려운 대용량 파일에 대해서 LFS대신 `ipfs`로 공유하거나 내려받습니다.

따라서 이 도구를 사용하기 위해선, 위의 두 명령줄 도구가 필요합니다.

## Example

- https://github.com/naratteu/naratteu-win-utils

## Todo

- gui manager?
- ipfs로 내려받는데 실패할경우의 Plan B도 포함.
    - 직접 다운로드 받을 수 있는 경로 자동 혹은 수동 안내 메세지
- 공유중인 파일에 대한 정보를 담은 micro wiki server

# `gip clone`

당신이 `dotnet-10` 을 설치한 환경이라면, 아래와같이 github url만으로 `sparse-checkout`이 적용된 저장소를 즉시 복제할 수 있습니다. (이 기능이 어째서 giptfs에 통합되어야 하는지는 설득할 수 없지만, LGTM입니다🤪)

```bash
dnx gip clone https://github.com/Cysharp/ConsoleAppFramework/tree/master/sandbox/CliFrameworkBenchmark
cd ConsoleAppFramework
git sparse-checkout list # sandbox/CliFrameworkBenchmark
```

## Todo

- standalone
- `npx gip clone`
- `go run gip clone`
- `uvx gip clone`
