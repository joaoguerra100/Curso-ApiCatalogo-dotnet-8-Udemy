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

public class PostProdutoUnitTests
{
    private readonly ProdutosController _controller;

    public PostProdutoUnitTests()
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
    public async Task PostProduto_return_createdStatusCode()
    {
        // Arrange
        var novoProdutoDto = new ProdutoDTO
        {
            Nome = "Novo Produto",
            Descricao = "Descrição do Novo Produto",
            Preco = 10.99m,
            ImagemUrl = "imagemFake1.jpg",
            CategoriaId = 2
        };

        // Act
        var data = await _controller.Post(novoProdutoDto);

        // Assert
        var createdResult = data.Result.Should().BeOfType<CreatedAtRouteResult>();
        createdResult.Subject.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task PostProduto_return_BadRequest()
    {
        ProdutoDTO produto = null;

        // Act
        var data = await _controller.Post(produto);

        // Assert
        var BadRequest = data.Result.Should().BeOfType<BadRequestObjectResult>();
        BadRequest.Subject.StatusCode.Should().Be(400);
    }
}