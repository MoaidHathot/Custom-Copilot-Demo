using Dumpify;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Demo;

public class GithubPlugin
{
    private static readonly HttpClient _client = new HttpClient();

    [KernelFunction, Description("Provide the top rated Github repositories for a Github user handle")]
    public static async Task<string> TopRepositories(
        [Description("A Github user handle")] string handle)
        => (await GetRepositories(handle)).DumpText();

    [KernelFunction, Description("Provide the Readme.md file of the most rated Github repository for a Github user handle")]
    public static async Task<string> TopRepositoryReadMe([Description("A Github user handle")] string handle)
    {
        var repo = (await GetRepositories(handle)).FirstOrDefault()!;
        var readme = await _client.GetStringAsync($"https://github.com/{handle}/{repo}/blob/main/README.md");

        return readme;
    }

    private static async Task<IEnumerable<string>> GetRepositories(string handle)
    {
        var url = $"https://github.com/{handle.Trim()}?tab=repositories&q=&type=&language=&sort=stargazers".Dump();
        var html = await _client.GetStringAsync(url);

        return Regex.Matches(html, @$".+/{handle}/(?<repo_name>[0-9a-zA-Z_\-]+).+name codeRepository.*", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            .Where(match => match.Success)
            .Select(match => match.Groups["repo_name"].Value)
            .ToArray();
    }
}