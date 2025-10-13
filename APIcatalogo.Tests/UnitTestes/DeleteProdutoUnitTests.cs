using System.Text.Json;
using APICatalogo.Context;
using APICatalogo.Controllers;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Models;
using APICatalogo.Repositories;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIcatalogo.Tests.UnitTestes;

public class DeleteProdutoUnitTests
{
    private readonly ProdutosController _controller;

    public DeleteProdutoUnitTests()
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
    public async Task DeleteProdutoById_Return_OKResult()
    {
        // Arrange
        var prodId = 2;

        // Act
        var result = await _controller.Delete(prodId);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteProdutoById_Return_NotFound()
    {
        // Arrange
        var prodId = 999;

        // Act
        var result = await _controller.Delete(prodId);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}