using System.Text.Json;
using APICatalogo.Context;
using APICatalogo.Controllers;
using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Models;
using APICatalogo.Repositories;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIcatalogo.Tests.UnitTestes;

public class PutProdutoUnitTests
{
    private readonly ProdutosController _controller;

    public PutProdutoUnitTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        // Se quiser carregar dados iniciais, pode usar o JSON aqui também
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "FakeData", "produto.json");
        var json = File.ReadAllText(jsonPath);
        var produtos = JsonSerializer.Deserialize<List<Produto>>(json);

        context.Produtos!.AddRange(produtos!);
        context.SaveChanges();

        var repository = new UnitOfWork(context);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new ProdutoDTOMappingProfile());
        });

        var mapper = config.CreateMapper();

        _controller = new ProdutosController(repository, mapper);
    }

    [Fact]
    public async Task PutProduto_Return_OkResult()
    {
        // Arrange
        var prodId = 14;

        var updateProdutoDto = new ProdutoDTO
        {
            ProdutoId = prodId,
            Nome = "Produto Atualizado - Testes",
            Descricao = "Minha Descricao",
            ImagemUrl = "imagem1.jpg",
            CategoriaId = 2
        };

        // Act
        var result = await _controller.Put(prodId, updateProdutoDto) as ActionResult<ProdutoDTO>;

        // Assert
        result.Should().NotBeNull(); //Verifica se o resultado nao e nulo
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PutProduto_Return_BadRequest()
    {
        // Arrange
        var prodId = 1000;

        var meuProduto = new ProdutoDTO
        {
            ProdutoId = 14,
            Nome = "Produto Atualizado - Testes",
            Descricao = "Minha Descricao",
            ImagemUrl = "imagem1.jpg",
            CategoriaId = 2
        };

        // Act
        var data = await _controller.Put(prodId, meuProduto);

        // Assert
        data.Result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }
}