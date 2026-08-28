using System.Linq;
using System.Threading.Tasks;
using Bunit;
using CRM.WebFrontend.Client.Models.Leads;
using CRM.WebFrontend.Client.Pages.Asesor;
using CRM.WebFrontend.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CRM.WebFrontend.Client.Tests;

public class LeadTrayTests : BunitContext
{
    [Fact]
    public void LeadTray_RendersSkeletonTable_Initially()
    {
        // Arrange
        var mockLeadService = new Mock<ILeadService>();
        
        // Simular que la llamada asíncrona se queda esperando para poder ver el estado inicial "Loading"
        var tcs = new TaskCompletionSource<System.Collections.Generic.IEnumerable<LeadResponse>>();
        mockLeadService.Setup(s => s.GetLeadsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                       .Returns(tcs.Task);

        Services.AddSingleton(mockLeadService.Object);

        // Act
        var cut = Render<LeadTray>();

        // Assert
        // Validamos que el componente Virtualize exista en el DOM
        var virtualizeComp = cut.FindComponent<Microsoft.AspNetCore.Components.Web.Virtualization.Virtualize<LeadResponse>>();
        Assert.NotNull(virtualizeComp);
    }
}
