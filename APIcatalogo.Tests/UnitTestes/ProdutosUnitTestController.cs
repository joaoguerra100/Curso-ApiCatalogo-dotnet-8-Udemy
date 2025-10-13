using System.Text.Json;
using APICatalogo.Context;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Models;
using APICatalogo.Repositories;
using APICatalogo.Repositories.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APIcatalogo.Tests.UnitTestes;

public class ProdutosUnitTestController
{
   public IUnitOfWork repository;
   public IMapper mapper;
   public static DbContextOptions<AppDbContext> dbContextOptions { get; }

   public static string connectionString = "Host=localhost;Database=apicatalagodb;Username=postgres;Password=*****";

   static ProdutosUnitTestController()
   {
      // Usando banco em memória para testes
      dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;

      // Usando o banco de dados
      /* dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
          .UseNpgsql(connectionString)
          .Options; */
   }

   public ProdutosUnitTestController()
   {
      // Configuração do AutoMapper
      var config = new MapperConfiguration(cfg =>
      {
         cfg.AddProfile(new ProdutoDTOMappingProfile());
      });

      mapper = config.CreateMapper();
      
      // Criação do contexto com banco em memória
      var context = new AppDbContext(dbContextOptions);

      // Carregando dados mockados do JSON
      var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "FakeData", "produto.json");
      var json = File.ReadAllText(jsonPath);
      var produtos = JsonSerializer.Deserialize<List<Produto>>(json);

      // Adicionando os dados ao contexto
      context.Produtos!.AddRange(produtos!);
      context.SaveChanges();

      repository = new UnitOfWork(context);
   }
}