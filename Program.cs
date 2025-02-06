
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// הוספת Swagger לשירותים
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הוספת Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific",
                    //   policy  =>
                    //   {
                    //       policy.AllowAnyOrigin()
                    //              .AllowAnyHeader() 
                    //              .AllowAnyMethod();   // מאפשר כל שיטת HTTP
                    //   });
                       policy =>
                      {
                          policy.WithOrigins("https://todolistclient-19b1.onrender.com") // הוסף את הדומיין הספציפי של ה-React
                                .AllowAnyHeader()
                                .AllowAnyMethod(); // מאפשר כל שיטת HTTP
                      });
});



// הוספת שירותי חיבור למסד נתונים
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("my_database");
    return new MySqlConnection(connectionString);
});

var app = builder.Build();

// הפעלת המדיניות של CORS
app.UseCors("AllowSpecific");

// הפעלת Swagger בהתאם לסביבה
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // יוצר את המסמכים של Swagger
    app.UseSwaggerUI(options => // יוצר את הממשק של Swagger UI
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.RoutePrefix = string.Empty; // מציג את Swagger ב-root של האפליקציה
    });
}

// הגדרת מסלולים
app.MapGet("/items", async (IDbConnection db) =>
{
    var items = await db.QueryAsync<Item>("select * from items");
    return Results.Json(items);
});

app.MapPost("/items", async ([FromBody] ItemDTOPost itemDTOPost, IDbConnection db) =>
{
    var query = "insert into items (Name,IsComplete) values (@Name,false) ";
    var result = await db.ExecuteAsync(query, new { Name = itemDTOPost.Name });

    return Results.Ok(new { Message = "successFul Post" });
});

app.MapPut("/items/{id}", async (int id, [FromBody] ItemDTOPut itemDTOPut, IDbConnection db) =>
{
    if (id <= 0)
    {
        return Results.BadRequest(new { Message = "ID is required" });
    }

    var query = "UPDATE items SET IsComplete = @IsComplete WHERE Id = @Id";
    var result = await db.ExecuteAsync(query, new { Id = id, itemDTOPut.IsComplete });

    if (result > 0)
    {
        return Results.Ok(new { Message = "Item updated successfully" });
    }
    else
    {
        return Results.NotFound(new { Message = "Item not found" });
    }
});

app.MapDelete("/items/{id}", async (int id, IDbConnection db) =>
{
    var query = "Delete from items WHERE Id = @Id";
    var result = await db.ExecuteAsync(query, new { Id = id });

    if (result > 0)
    {
        return Results.Ok(new { Message = "Item deleted successfully" });
    }
    else
    {
        return Results.NotFound(new { Message = "Item not found" });
    }
});

app.MapGet("/", () => "Hello World!!!!!!!!!!!!!!!!!!!");

app.Run();

public record ItemDTOPost(string Name);//add
public record ItemDTOPut(string Name, bool IsComplete);//update

