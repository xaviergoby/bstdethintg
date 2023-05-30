namespace Hodl.Api.Extensions;

public static class MigrationManager
{
    public static IApplicationBuilder ApplyDatabaseMigrations<TContext>(this IApplicationBuilder app) where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            dbContext.Database.Migrate();
            if (dbContext is IDbContextInitialDataProvider dbDataInit)
            {
                dbDataInit.AddInitialData();
            }
        }
        catch //(Exception ex)
        {
            // TODO: Log errors
            throw;
        }

        return app;
    }
}
