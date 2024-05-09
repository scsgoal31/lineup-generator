
using LineupGenerator.App;

var INNINGS = 5;
var PLAYERS = 11;

var game = LineupService.GenerateFullGame(
    numInnings: INNINGS,
    numPlayers: PLAYERS,
    playersToAvoidBenching: [0, 5, 6],
    playersAllowedToPlay1B: [3, 5, 7, 9, 10]);

Console.WriteLine("\t" + string.Join("\t", Enumerable.Range(0, PLAYERS).ToArray()));
for (var inning = 0; inning < INNINGS; inning++)
{
    Console.Write($"{inning}\t");
    foreach (var player in game[inning].OrderBy(i => i.Key))
    {
        Console.Write($"{player.Value}\t");
    }
    Console.Write("\r\n");
}
