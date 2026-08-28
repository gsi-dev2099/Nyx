using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace CRM.WebFrontend.E2ETests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class LeadTrayE2ETests : PageTest
{
    private string BaseUrl => Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:5261";
    private string UserEmail => Environment.GetEnvironmentVariable("TEST_USER_EMAIL") ?? throw new InvalidOperationException("TEST_USER_EMAIL no está configurada");
    private string UserPassword => Environment.GetEnvironmentVariable("TEST_USER_PASSWORD") ?? throw new InvalidOperationException("TEST_USER_PASSWORD no está configurada");

    [Test]
    public async Task Asesor_PuedeAsignarseUnLead_DesdeLaBolsaDeTrabajo()
    {
        // 1. Navegar e Iniciar Sesión
        await Page.GotoAsync($"{BaseUrl}/login");
        
        await Page.FillAsync("input[type='email']", UserEmail);
        await Page.FillAsync("input[type='password']", UserPassword);
        await Page.ClickAsync("button[type='submit']");
        
        // Esperar a que el login finalice exitosamente (ej. redirige al home/dashboard)
        await Page.WaitForURLAsync(url => !url.Contains("/login"), new PageWaitForURLOptions { Timeout = 10000 });

        // 2. Navegar a la Bolsa de Trabajo de Leads
        await Page.GotoAsync($"{BaseUrl}/leads/tray");

        // 3. Esperar a que el LoadingSkeleton desaparezca (el DOM de virtualize carga los datos)
        // Buscamos cualquier elemento del skeleton y esperamos que su estado sea Hidden o Detached.
        var skeletonLocator = Page.Locator(".skeleton-shimmer");
        if (await skeletonLocator.CountAsync() > 0)
        {
            await skeletonLocator.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        }

        // Esperar a que aparezcan los botones de "Asignarme" o el "EmptyState"
        var asignarmeButton = Page.Locator("button:has-text('Asignarme')");
        var emptyState = Page.Locator("text='No hay leads en la bolsa de trabajo en este momento.'");

        // Utilizamos WaitForSelector o evaluamos cuál de los dos estados se cumple
        var elementAppeared = await Task.WhenAny(
            asignarmeButton.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 }),
            emptyState.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 })
        );

        if (await emptyState.IsVisibleAsync())
        {
            Assert.Pass("El test funcionó pero la base de datos no tiene Leads disponibles para ser asignados (EmptyState).");
            return;
        }

        // Contamos cuántos botones "Asignarme" hay antes de hacer clic
        int initialCount = await asignarmeButton.CountAsync();
        Assert.That(initialCount, Is.GreaterThan(0), "Debería haber al menos un lead para asignar.");

        // 4. Localizar el primer Lead y hacer clic en "Asignarme"
        await asignarmeButton.First.ClickAsync();

        // 5. Afirmar (Assert) que la interfaz se actualiza correctamente
        // Esperamos a que aparezca la notificación de éxito en la interfaz o que el número de botones disminuya.
        var successNotification = Page.Locator("text='Lead asignado correctamente'");
        
        try
        {
            await successNotification.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
            Assert.That(await successNotification.IsVisibleAsync(), Is.True, "La notificación de éxito debe ser visible.");
        }
        catch (TimeoutException)
        {
            // Alternativamente, comprobamos si la tabla refrescó y hay un botón menos o si se aplicó otro efecto visual.
            // Para la virtualización, un simple recuento de elementos visibles puede bastar.
            Assert.Fail("No se encontró el mensaje de éxito de asignación de Lead.");
        }
    }
}
