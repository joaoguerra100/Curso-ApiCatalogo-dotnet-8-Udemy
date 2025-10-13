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

public class GetProdutoUnitTests
{

    private readonly ProdutosController _controller;

    public GetProdutoUnitTests()
    {
        // Banco isolado por teste
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        // Carregando dados do JSON
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
    public async Task GetProdutoById_Return_OK()
    {
        //Arrange
        var produtoId = 2;

        //Act
        var data = await _controller.Get(produtoId);

        //Assert (xunit)
        // var okResult = Assert.IsType<OkObjectResult>(data.Result);
        // Assert.Equal(200, okResult.StatusCode);

        // Assert(fluente assertions)
        data.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetProdutoById_Return_NotFound()
    {
        //Arrange
        var produtoId = 999;

        //Act
        var data = await _controller.Get(produtoId);

        // Assert(fluente assertions)
        data.Result.Should().BeOfType<NotFoundObjectResult>()
                    .Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetProdutoById_Return_BadRequest()
    {
        //Arrange
        int produtoId = -1;

        //Act
        var data = await _controller.Get(produtoId);

        // Assert(fluente assertions)
        data.Result.Should().BeOfType<BadRequestObjectResult>()
                    .Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetProdutos_Return_ListOfProdutoDTO()
    {
        //Act
        var data = await _controller.Get();

        // Assert(fluente assertions)
        data.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeAssignableTo<IEnumerable<ProdutoDTO>>()
            .And.NotBeNull();
    }

    /* [Fact]
    public async Task GetProdutos_Return_BadRequestResult()
    {
        //Act
        var data = await _controller.Get();

        // Assert
        data.Result.Should().BeOfType<BadRequestObjectResult>();
    } */
}