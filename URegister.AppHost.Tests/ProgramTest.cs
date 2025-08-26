using Aspire.Hosting.Testing;
using URegister.Infrastructure.Constants;

[TestFixture]
public class ProgramIntegrationTests
{
    [Test]
    public async Task ContainerNamesAreCorrectTest()
    {
        // Arrange
        await using var app = await DistributedApplicationTestingBuilder.CreateAsync<Projects.URegister>();

        // Act
        var distributedApplication = await app.BuildAsync();
        await distributedApplication.StartAsync();

        // Assert
        Assert.IsNotNull(app.Resources.SingleOrDefault(r => r.Name == ContainerNameConstants.NomenclaturesCatalog));
        Assert.IsNotNull(app.Resources.SingleOrDefault(r => r.Name == ContainerNameConstants.AuditLog));
        Assert.IsNotNull(app.Resources.SingleOrDefault(r => r.Name == ContainerNameConstants.IntegrationsCatalog));
        Assert.IsNotNull(app.Resources.SingleOrDefault(r => r.Name == ContainerNameConstants.NumberGenerator));
        Assert.IsNotNull(app.Resources.SingleOrDefault(r => r.Name == ContainerNameConstants.ObjectsCatalog));
        Assert.IsNotNull(app.Resources.SingleOrDefault(r => r.Name == ContainerNameConstants.RegistersCatalog));
        Assert.IsNotNull(app.Resources.SingleOrDefault(r => r.Name == ContainerNameConstants.UsersCatalog));
        
        // Cleanup
        await distributedApplication.StopAsync();
    }
}