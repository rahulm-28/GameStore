using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoint
{
    const string GetGameEndpointName = "GetGame";

    private static readonly List<GameDto> games = [
        new GameDto(1, "Street Fighter II", "Fighting", 19.99M, new DateOnly(1992, 7, 15)),
    new GameDto(2, "Final Fantasy XIV", "Roleplaying", 59.99M, new DateOnly(2010, 9, 30)), new GameDto(3, "FIFA 23", "Sports", 69.99M, new DateOnly(2022, 9, 27))
    ];

    public static RouteGroupBuilder MapGamesEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("games").WithParameterValidation();
        // Get all games
        group.MapGet("/", () => games);

        // Get one game
        group.MapGet("/{id}", (int id) =>
        {
            GameDto? game = games.Find(game => game.Id == id);

            return game is null ? Results.NotFound() : Results.Ok(game);
        })
        .WithName(GetGameEndpointName);

        // POST /games
        group.MapPost("/", (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = new()
            {
                Name = newGame.Name,
                Genre = dbContext.Genres.Find(newGame.GenreId),
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };
            dbContext.Games.Add(game);
            dbContext.SaveChanges();

            return Results.CreatedAtRoute(GetGameEndpointName, new { id = game.Id }, game);
        }).WithParameterValidation();

        // PUT
        group.MapPut("/{id}", (int id, UpdateGameDto updatedGame) =>
        {
            var index = games.FindIndex(game => game.Id == id);
            if (index == -1)
            {
                return Results.NotFound();
            }
            games[index] = new GameDto(id, updatedGame.Name, updatedGame.Genre, updatedGame.Price, updatedGame.ReleaseDate);

            return Results.NoContent();
        });

        // DELETE 
        group.MapDelete("/{id}", (int id) =>
        {
            games.RemoveAll(game => game.Id == id);
            return Results.NoContent();
        });
        return group;
    }
}