using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.Leads;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using Moq;
using Xunit;

namespace CRM.ApiHub.Tests.Application.UseCases.Leads;

public class CreateLeadUseCaseTests
{
    private readonly Mock<ILeadRepository> _mockRepository;
    private readonly CreateLeadUseCase _useCase;

    public CreateLeadUseCaseTests()
    {
        _mockRepository = new Mock<ILeadRepository>();
        _useCase = new CreateLeadUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateLeadAndReturnIt_WithNullOwner()
    {
        // Arrange
        var dto = new LeadCreateDto
        {
            FirstName = "Juan",
            LastName = "Perez",
            Email = "juan@example.com",
            Phone = "123456789",
            IdCmpg = 10,
            IdSrc = 20,
            DocumentNumber = "87654321",
            RawData = "{\"source\":\"web\"}",
            AssignedUserId = null
        };

        var expectedId = 99L;
        _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.IdLead);
        Assert.Equal("Juan", result.FirstName);
        Assert.Equal("Perez", result.LastName);
        Assert.Null(result.OwnerUserId); // Valida explícitamente que sea nulo (bolsa de trabajo)
        Assert.Null(result.CustodyUserId);
        Assert.Equal(1, result.CurrentStatusId); // Estado por defecto NUEVO
        
        _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
