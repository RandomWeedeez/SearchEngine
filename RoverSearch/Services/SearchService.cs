using RoverSearch.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RoverSearch.Services;

public class SearchService
{
    private string path = @".\Data\";

    public SearchService()
    {

    }

    /// <summary>
    /// Naive search implementation
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public SearchResults Search(string query)
    {
        var sw = new Stopwatch();
        sw.Start();

        var results = new List<Result>();

        string[] words = query.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (string file in Directory.GetFiles(path))
        {
            string content = File.ReadAllText(file);
            string filename = Path.GetFileName(file);

            // probably overcoplex but my mind is on using split
            string season = content
                .Split('\n')
                .FirstOrDefault(line => line.StartsWith("season:"))
                ?.Split(':', 2)[1].Trim() ?? "";

            string episode = content
                .Split('\n')
                .FirstOrDefault(line => line.StartsWith("episode:"))
                ?.Split(':', 2)[1].Trim() ?? "";

            string title = content
                .Split('\n')
                .FirstOrDefault(line => line.StartsWith("title:"))
                ?.Split(':', 2)[1].Trim() ?? "";

            string script = content
                .Split(new[] { "\r\n\r\n", "\n\n" }, 2, StringSplitOptions.None)
                .LastOrDefault() ?? "";

            // the first 200 characters was suggested by a friend... but i just did first 5 sentances to show the start of an episode.
            var sentences = Regex.Matches(script, @"[^.!?]+[.!?]+");

            string description = string.Join(" ",
                sentences.Cast<Match>()
                .Take(5)
                .Select(m => m.Value.Trim()));

            // score segment. this took ages but im happy with it
            int score = 0;

            // you getscore based on how many times each word appears
            foreach (string word in words)
            {
                if (content.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    int occurrences = Regex.Matches(
                    content,
                    Regex.Escape(word),
                    RegexOptions.IgnoreCase
                    ).Count;

                    score += occurrences;
                }

                // filename matches have additional score to account for searching for episodes
                if (filename.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    score += 2;
                }
            }

            // big score if the whole thing is in the file
            if (content.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                score += 5;
            }

            // only includes files with one match or more
            if (score > 0)
            {
                results.Add(new Result
                {
                    Filename = filename,
                    Title = title,
                    Season = season,
                    Episode = episode,
                    Description = description,
                    Score = score
                });
            }
        }

        // sorts them by which has the highest scpore first
        results = results
            .OrderByDescending(r => r.Score)
            .ToList();

        sw.Stop();

        return new SearchResults
        {
            Query = query,
            Results = results,
            Duration = sw.Elapsed
        };
    }
}
