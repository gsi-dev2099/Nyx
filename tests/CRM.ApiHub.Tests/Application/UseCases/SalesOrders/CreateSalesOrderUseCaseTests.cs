using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Application.DTOs;
using CRM.ApiHub.Application.UseCases.SalesOrders;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.ApiHub.Tests.Application.UseCases.SalesOrders;

public class CreateSalesOrderUseCaseTests
{
    private readonly Mock<ISalesOrderRepository> _salesOrderRepoMock;
    private readonly Mock<ISlaEngineClient> _slaEngineClientMock;
    private readonly Mock<IFlowEngineClient> _flowEngineClientMock;
    private readonly Mock<IApprovalEngineClient> _approvalEngineClientMock;
    private readonly Mock<ILogger<CreateSalesOrderUseCase>> _loggerMock;
    private readonly CreateSalesOrderUseCase _useCase;

    public CreateSalesOrderUseCaseTests()
    {
        _salesOrderRepoMock = new Mock<ISalesOrderRepository>();
        _slaEngineClientMock = new Mock<ISlaEngineClient>();
        _flowEngineClientMock = new Mock<IFlowEngineClient>();
        _approvalEngineClientMock = new Mock<IApprovalEngineClient>();
        _loggerMock = new Mock<ILogger<CreateSalesOrderUseCase>>();

        _useCase = new CreateSalesOrderUseCase(
            _salesOrderRepoMock.Object,
            _slaEngineClientMock.Object,
            _flowEngineClientMock.Object,
            _approvalEngineClientMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithDiscountGreaterThan10_ShouldSetPendingApprovalAndSubmitRequest()
    {
        // Arrange
        var dto = new SalesOrderCreateDto
        {
            IdUser = 1,
            IdCmpg = 2,
            DiscountPercentage = 15m
        };

        _salesOrderRepoMock.Setup(r => r.CreateAsync(It.IsAny<SalesOrder>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100L);

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        Assert.Equal("PENDING_APPROVAL", result.Status);
        Assert.Equal(100L, result.IdOrder);

        _salesOrderRepoMock.Verify(r => r.CreateAsync(It.Is<SalesOrder>(o => o.Status == "PENDING_APPROVAL"), It.IsAny<CancellationToken>()), Times.Once);
        _approvalEngineClientMock.Verify(c => c.SubmitRequestAsync("HIGH_DISCOUNT", "order", 100L, 1L, "{\"discount\":15}", null), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDiscount10OrLess_ShouldNotSubmitApprovalRequest()
    {
        // Arrange
        var dto = new SalesOrderCreateDto
        {
            IdUser = 1,
            IdCmpg = 2,
            Status = "BORRADOR",
            DiscountPercentage = 10m
        };

        _salesOrderRepoMock.Setup(r => r.CreateAsync(It.IsAny<SalesOrder>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(101L);

        // Act
        var result = await _useCase.ExecuteAsync(dto);

        // Assert
        Assert.Equal("BORRADOR", result.Status);
        Assert.Equal(101L, result.IdOrder);

        _salesOrderRepoMock.Verify(r => r.CreateAsync(It.Is<SalesOrder>(o => o.Status == "BORRADOR"), It.IsAny<CancellationToken>()), Times.Once);
        _approvalEngineClientMock.Verify(c => c.SubmitRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
