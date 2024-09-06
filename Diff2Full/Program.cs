using Runner.DTOs;
using Sproutopia.Models;
using Sproutopia.Utilities;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var fileArgument = new Argument<FileInfo>("file", "The file to transform.");
        var gridSize = new Option<(int,int)>(
            name: "gridsize",
            description: "Grid size associated with input file.",
            isDefault: true,
            parseArgument: result =>
            {
                if (!result.Tokens.Any())
                {
                    return (50,50);
                }

                var elems = result.Tokens.Single().Value.Split(',');
                if (elems.Length != 2)
                {
                    result.ErrorMessage = "gridsize must be in the format \"rows,colums\" where 'rows' and 'colums' are integers";
                    return (0,0);
                }

                if (!int.TryParse(elems[0], out var rows) || !int.TryParse(elems[1], out var cols))
                {
                    result.ErrorMessage = "gridsize must be in the format \"rows,colums\" where 'rows' and 'colums' are integers";
                    return (0, 0);
                }

                return (rows,cols);
            });

        var rootCommand = new RootCommand();
        rootCommand.AddArgument(fileArgument);
        rootCommand.AddOption(gridSize);

        rootCommand.SetHandler((fileArgument, gridSize) =>
        {
            TransformFile(fileArgument, gridSize);
        },
        fileArgument, gridSize);

        return await rootCommand.InvokeAsync(args);
    }

    private static void TransformFile(FileInfo file, (int rows, int cols) gridSize)
    {
        var diffLogs = Helpers.LoadJson<DiffLog>(file.FullName, ["\r\n", "\n", "\r"]);

        var newTerritory = Helpers.CreateJaggedArray<int[][]>(gridSize.rows, gridSize.cols);
        Helpers.SetAllValues(newTerritory, 255);

        var newTrails = Helpers.CreateJaggedArray<int[][]>(gridSize.rows, gridSize.cols);
        Helpers.SetAllValues(newTrails, 255);

        var gameState = new GameStateDto(
            timeStamp: DateTime.Now,
            currentTick: 0,
            botSnapshots: [],
            territory: newTerritory,
            trails: newTrails,
            leaderBoard: [],
            powerUps: Array.Empty<PowerUpLocation>(),
            weeds: Helpers.CreateJaggedArray<bool[][]>(gridSize.rows, gridSize.cols)
            );

        foreach (var diffLog in diffLogs)
        {
            gameState = gameState.ApplyDiff(diffLog);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(gameState));
        }
    }
}