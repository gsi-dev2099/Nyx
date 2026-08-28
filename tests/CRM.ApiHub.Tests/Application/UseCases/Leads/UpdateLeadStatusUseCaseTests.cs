using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.Leads;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Domain.Exceptions;
using CRM.ApiHub.Infrastructure.Services;
using CRM.ApiHub.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.ApiHub.Tests.Application.UseCases.Leads;

public class UpdateLeadStatusUseCaseTests
{
    private readonly Mock<ILeadRepository> _mockRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IFlowEngineClient> _mockFlowEngineClient;
    private readonly Mock<ISlaEngineClient> _mockSlaEngineClient;
    private readonly Mock<ILogger<UpdateLeadStatusUseCase>> _mockLogger;
    private readonly UpdateLeadStatusUseCase _useCase;

    public UpdateLeadStatusUseCaseTests()
    {
        _mockRepository = new Mock<ILeadRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockFlowEngineClient = new Mock<IFlowEngineClient>();
        _mockSlaEngineClient = new Mock<ISlaEngineClient>();
        _mockLogger = new Mock<ILogger<UpdateLeadStatusUseCase>>();
        
        _useCase = new UpdateLeadStatusUseCase(
            _mockRepository.Object, 
            _mockNotificationService.Object,
            _mockFlowEngineClient.Object,
            _mockSlaEngineClient.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUpdate_WhenTransitionIsValid()
    {
        // Arrange
        var idLead = 1L;
        var actorId = 10L;
        var dto = new LeadUpdateStatusDto { IdStatus = 2, Comment = "Avanza a gestión" };
        var mockLead = new Lead { IdLead = idLead, CurrentStatusId = 1 };

        _mockRepository.Setup(r => r.GetByIdAsync(idLead, It.IsAny<CancellationToken>())).ReturnsAsync(mockLead);
        
        // Simular transición válida (retorna true)
        _mockFlowEngineClient.Setup(c => c.ValidateTransitionAsync("LEAD", 1, 2))
                             .ReturnsAsync(true);
                             
        _mockRepository.Setup(repo => repo.UpdateStatusAsync(idLead, dto.IdStatus, dto.Comment, actorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _useCase.ExecuteAsync(idLead, dto, actorId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(repo => repo.UpdateStatusAsync(idLead, 2, "Avanza a gestión", actorId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidTransitionException_WhenTransitionIsInvalid()
    {
        // Arrange
        var idLead = 1L;
        var actorId = 10L;
        var dto = new LeadUpdateStatusDto { IdStatus = 4, Comment = "Cierre prematuro" };
        var mockLead = new Lead { IdLead = idLead, CurrentStatusId = 1 };

        _mockRepository.Setup(r => r.GetByIdAsync(idLead, It.IsAny<CancellationToken>())).ReturnsAsync(mockLead);
        
        // Simular transición inválida (retorna false)
        _mockFlowEngineClient.Setup(c => c.ValidateTransitionAsync("LEAD", 1, 4))
                             .ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidTransitionException>(() => _useCase.ExecuteAsync(idLead, dto, actorId));
        Assert.Contains("no es válida según el motor", ex.Message);
        
        // Verifica que no se actualice la DB
        _mockRepository.Verify(repo => repo.UpdateStatusAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowInvalidTransitionException_WhenFlowEngineIsDown()
    {
        // Arrange
        var idLead = 1L;
        var actorId = 10L;
        var dto = new LeadUpdateStatusDto { IdStatus = 2, Comment = "Intentar avanzar con motor caído" };
        var mockLead = new Lead { IdLead = idLead, CurrentStatusId = 1 };

        _mockRepository.Setup(r => r.GetByIdAsync(idLead, It.IsAny<CancellationToken>())).ReturnsAsync(mockLead);
        
        // Simular caída del servicio (lanza excepción por Circuit Breaker abierto / error de red)
        _mockFlowEngineClient.Setup(c => c.ValidateTransitionAsync("LEAD", 1, 2))
                             .ThrowsAsync(new System.Net.Http.HttpRequestException("Connection refused"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidTransitionException>(() => _useCase.ExecuteAsync(idLead, dto, actorId));
        Assert.Contains("debido a un error de red", ex.Message);
        
        // Verifica que no se actualice la DB
        _mockRepository.Verify(repo => repo.UpdateStatusAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
