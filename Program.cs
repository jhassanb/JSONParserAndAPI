using Newtonsoft.Json;
using System.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Project1
{
    //class for all of the statistics that are under visiting statistics and home statistics 
    public class GameStats
    {
        public string StatIDCode { get; set; }
        public string GameCode { get; set; }
        public int TeamCode { get; set; }
        public string GameDate { get; set; }
        public int RushYds { get; set; }
        public int RushAtt { get; set; }
        public int PassYds { get; set; }
        public int PassAtt { get; set; }
        public int PassComp { get; set; }
        public int Penalties { get; set; }
        public int PenaltYds { get; set; }
        public int FumblesLost { get; set; }
        public int InterceptionsThrown { get; set; }
        public int FirstDowns { get; set; }
        public int ThridDownAtt { get; set; }
        public int ThirdDownConver { get; set; }
        public int FourthDownAtt { get; set; }
        public int FourthDownConver { get; set; }
        public int TimePoss { get; set; }
        public int Score { get; set; }
    }

    //class for all game data put together 
    public class AllGameData
    {

        public bool Neutral { get; set; }
        public string VisTeamName { get; set; }
        public GameStats VisStats { get; set; }
        public string HomeTeamName { get; set; }
        public GameStats HomeStats { get; set; }
        public bool IsFinal { get; set; }
        public string Date { get; set; }

    }

    public class AllGameDataWrapper
    {
        public List<AllGameData> MatchUpStats { get; set; }
    }

    //reads and deserializes data from the JSON file into a C# object
    public class UrlDataReader
    {
        private readonly HttpClient httpClient; // Declare an instance of HttpClient
        public UrlDataReader()
        {
            httpClient = new HttpClient(); // Initialize the HttpClient instance
        }

        // Method to fetch JSON data asynchronously from the given URL
        public async Task<string> FetchJsonDataAsync(string url)
        {
            try
            {
                // Send an HTTP GET request to the specified URL
                HttpResponseMessage response = await httpClient.GetAsync(url);

                // Ensure the response is successful (status code 200)
                response.EnsureSuccessStatusCode(); // This line throws an exception if the status code is not in the success range

                // Read the response content as a string (the JSON data)
                string jsonData = await response.Content.ReadAsStringAsync();

                return jsonData; // Return the fetched JSON data
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching data: " + ex.Message); // Print an error message if something goes wrong
                return null; // Return null to indicate failure
            }
        }
    }

    public class JsonDataParser
    {
        // Method to parse JSON data into a list of AllGameData objects
        public List<AllGameData> ParseJsonData(string jsonData) // Define a public method named ParseJsonData that takes a string jsonData as input
        {
            try
            {
                // Deserialize the JSON data into a List<AllGameData> using Newtonsoft.Json
                List<AllGameData> allGameDataList = JsonConvert.DeserializeObject<List<AllGameData>>(jsonData);
                return allGameDataList; // Return the parsed list of game data
            }
            catch (Exception ex) // Catch any exceptions that occur during parsing
            {
                Console.WriteLine("Error parsing JSON data: " + ex.Message); // Print an error message if parsing fails
                return null; // Return null to indicate failure
            }
        }
    }

    public class TeamStatsProcessor
    {
        private const string BaseUrl = "https://sports.snoozle.net/search/nfl/searchHandler?";
        private const string FileType = "inline";
        private const string StatType = "teamStats";
        private const string Season = "2020";

        public async Task ProcessTeamStatsAsync()
        {
            Dictionary<int, int> teamPoints = new Dictionary<int, int>();
            Dictionary<int, string> teamCodeToName = new Dictionary<int, string>();

            for (int teamNumber = 1; teamNumber <= 32; teamNumber++)
            {
                string teamName = teamNumber.ToString();
                string requestUrl = $"{BaseUrl}fileType={FileType}&statType={StatType}&season={Season}&teamName={teamName}";

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(requestUrl);
                        string json = await response.Content.ReadAsStringAsync();

                        AllGameDataWrapper dataWrapper = JsonConvert.DeserializeObject<AllGameDataWrapper>(json);

                        if (dataWrapper.MatchUpStats.Count > 0)
                        {
                            foreach (var gameData in dataWrapper.MatchUpStats)
                            {
                                int teamCode = gameData.VisStats.TeamCode;

                                int teamScoreInGame = 0;
                                if (gameData.VisStats.TeamCode == teamCode)
                                {
                                    teamScoreInGame = gameData.VisStats.Score;
                                }
                                else if (gameData.HomeStats.TeamCode == teamCode)
                                {
                                    teamScoreInGame = gameData.HomeStats.Score;
                                }

                                if (teamPoints.ContainsKey(teamCode))
                                {
                                    teamPoints[teamCode] += teamScoreInGame;
                                }
                                else
                                {
                                    teamPoints[teamCode] = teamScoreInGame;
                                }

                                // Associate team name with team code
                                if (!teamCodeToName.ContainsKey(teamCode))
                                {
                                    teamCodeToName[teamCode] = gameData.VisTeamName;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }

            // Sort the team codes numerically
            List<int> sortedTeamCodes = teamPoints.Keys.ToList();
            sortedTeamCodes.Sort();

            foreach (var teamCode in sortedTeamCodes)
            {
                string teamName = teamCodeToName.ContainsKey(teamCode) ? teamCodeToName[teamCode] : "Unknown Team";

                Console.WriteLine("Team Code: " + teamCode);
                Console.WriteLine("Team Name: " + teamName);
                Console.WriteLine("Total Points Scored: " + teamPoints[teamCode]);
                Console.WriteLine();
            }
        }
    }

    internal class JSONProgram
    {
        static async Task Main()
        {
            Console.WriteLine("Welcome to the 2020 Football Season Hub!");

            TeamStatsProcessor teamStatsProcessor = new TeamStatsProcessor();
            await teamStatsProcessor.ProcessTeamStatsAsync();
        }
    }
}

