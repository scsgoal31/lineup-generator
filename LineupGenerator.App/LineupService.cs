namespace LineupGenerator.App;

public static class LineupService
{
    private static readonly HashSet<Position> ALL_POSITIONS = [Position.B, Position.P, Position.C, Position.OneB, Position.TwoB, Position.ThreeB, Position.SS, Position.LF, Position.LCF, Position.RCF, Position.RF];
    private static readonly HashSet<Position> INFIELD = [Position.P, Position.C, Position.OneB, Position.TwoB, Position.ThreeB, Position.SS];

    private static readonly Random RANDOMIZER = new();

    public static IDictionary<int, Position>[] GenerateFullGame(int numInnings, int numPlayers, HashSet<int> playersToAvoidBenching, HashSet<int> playersAllowedToPlay1B)
    {
        var players = Enumerable.Range(0, numPlayers).ToArray();

        var innings = new List<Dictionary<int, Position>>();
        for (var inningNumber = 0; inningNumber < numInnings; inningNumber++)
        {
            // get random player order as queue
            var playersToPlace = players.OrderBy(_ => RANDOMIZER.Next()).ToArray();
            var positionsToAssign = ALL_POSITIONS.OrderBy(_ => RANDOMIZER.Next()).ToArray();

            var playerPositionScores = new List<PlayerPositionScore>();
            foreach (var playerToPlace in playersToPlace)
            {
                var positionScores = ALL_POSITIONS.ToDictionary(p => p, _ => 0);
                foreach (var positionToAssign in positionsToAssign)
                {
                    // Rule 2: A player may not play Catcher more than once this game.
                    if (positionToAssign == Position.C &&
                        TimesHasPlayedPositions(innings, playerToPlace, [positionToAssign]) < 1)
                        positionScores[positionToAssign] += 1;

                    // Rule 7: Each player may only sit out for up to one inning.
                    if (positionToAssign == Position.B &&
                        TimesHasPlayedPositions(innings, playerToPlace, [positionToAssign]) < 1 &&
                        !playersToAvoidBenching.Contains(playerToPlace))    // Rule 5: Players 0, 5, and 6 must play every inning and never sit out.
                        positionScores[positionToAssign] += 1;

                    // Rule 4: 3, 5, 7, 9, and 10 are the only players allowed to play 1B. When not playing 1B, they can play any other position.
                    if (positionToAssign == Position.OneB &&
                        TimesHasPlayedPositions(innings, playerToPlace, [positionToAssign]) < 1 &&
                        playersAllowedToPlay1B.Contains(playerToPlace))
                        positionScores[positionToAssign] += 1;

                    // Rule 3: A Player may play any other position beside Catcher for up to two innings in the game.
                    if (positionToAssign != Position.C &&
                        positionToAssign != Position.B &&   // not a real position and already has a rule about max of 1
                        TimesHasPlayedPositions(innings, playerToPlace, [positionToAssign]) < 2)
                        positionScores[positionToAssign] += 1;

                    // Rule 1: Each player must play in the infield for at least two innings this game.
                    if (INFIELD.Contains(positionToAssign) &&
                        TimesHasPlayedPositions(innings, playerToPlace, [positionToAssign]) < 2)
                        positionScores[positionToAssign] += 1;
                }

                playerPositionScores.AddRange(positionScores
                    .Select(ps => new PlayerPositionScore
                    {
                        Player = playerToPlace,
                        Position = ps.Key,
                        Score = ps.Value,
                    }));
            }

            var inningLineup = new Dictionary<int, Position>();
            /*
            foreach (var position in ALL_POSITIONS)
            {
                var player = playerPositionScores
                    .Where(s => s.Position == position)
                    .Where(s => !inningLineup.ContainsKey(s.Player))
                    .OrderByDescending(s => s.Score)
                    .First()
                    .Player;

                inningLineup.Add(player, position);
            }
            */
            foreach (var playerPositionScore in playerPositionScores.OrderByDescending(s => s.Score))
            {
                if (inningLineup.ContainsKey(playerPositionScore.Player)) continue;
                if (inningLineup.ContainsValue(playerPositionScore.Position)) continue;

                inningLineup.Add(playerPositionScore.Player, playerPositionScore.Position);
            }

            innings.Add(inningLineup);
        }

        return [.. innings];
    }

    private record PlayerPositionScore
    {
        public required int Player { get; init; }
        public required Position Position { get; init; }
        public required int Score { get; init; }
    }


    private static int TimesHasPlayedPositions(List<Dictionary<int, Position>> game, int player, HashSet<Position> positions)
        => game.Where(inning => inning.TryGetValue(player, out var position) && positions.Contains(position)).Count();
}

public enum Position
{
    P,
    C,
    OneB,
    TwoB,
    ThreeB,
    SS,
    LF,
    LCF,
    RCF,
    RF,
    B,
}
