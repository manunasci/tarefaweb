using Tarefas.db;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Conexão
builder.Services.AddDbContext<tarefasContext>(opt =>
{
    string connectionString = builder.Configuration.GetConnectionString("tarefasConnection");
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    opt.UseMySql(connectionString, serverVersion);
});

// OpenAPI (Swagger)
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // OpenAPI (Swagger)
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Arquivos estáticos
app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoints da API
app.MapGet("/api/tarefas", ([FromServices] tarefasContext _db) =>
{
    return Results.Ok(_db.Tarefa.ToList<Tarefa>());
});

app.MapGet("/api/tarefas/{id}", (
    [FromServices] tarefasContext _db,
    [FromRoute] int id
) =>
{
  //busca pelo id que está na rota
  var tarefa =_db.Tarefa.Find(id);

  //encontrou?
  if (tarefa==null)
    {
      // não encontrou
      //retornar um 404
      return Results.NotFound();
    }

  // encontrou
  //retornar 200 e os dados da tarefa
  return Results.Ok(tarefa);
});

// Incluir nova tarefa
app.MapPost("/api/tarefas",(
 [FromServices] tarefasContext _db,
 [FromBody] Tarefa novaTarefa
) =>
{
  if (string.IsNullOrEmpty(novaTarefa.Descricao))
  {
      return Results.BadRequest();
  }

    if (novaTarefa.Concluida)
  {
      return Results.BadRequest();
  }

  var tarefa = new Tarefa{
      Descricao = novaTarefa.Descricao,
      Concluida = novaTarefa.Concluida,
  };

  _db.Tarefa.Add(tarefa);
  _db.SaveChanges();

  string urlTarefaCriada = $"/api/tarefas/{tarefa.Id}";

  return Results.Created(urlTarefaCriada, tarefa);
});

app.Run();
