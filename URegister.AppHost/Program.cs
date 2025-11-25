var builder = DistributedApplication.CreateBuilder(args);
var auditlog = builder.AddProject<Projects.URegister_AuditLog>("uregister-auditlog");
var nomenclaturescatalog = builder.AddProject<Projects.URegister_NomenclaturesCatalog>("uregister-nomenclaturescatalog")
                                  .WithReference(auditlog);
var registerscatalog = builder.AddProject<Projects.URegister_RegistersCatalog>("uregister-registerscatalog")
                            .WithReference(nomenclaturescatalog)
                            .WithReference(auditlog);
var objectscatalog = builder.AddProject<Projects.URegister_ObjectsCatalog>("uregister-objectscatalog")
                            .WithReference(auditlog);
var numbergenerator = builder.AddProject<Projects.URegister_NumberGenerator>("uregister-numbergenerator")
                             .WithReference(auditlog);
var users = builder.AddProject<Projects.Uregister_Users>("uregister-users")
                   .WithReference(auditlog);
var integrationsCatalog = builder.AddProject<Projects.URegister_IntegrationsCatalog>("uregister-integrationscatalog")
                                 .WithReference(registerscatalog)
                                 .WithReference(nomenclaturescatalog)
                                 .WithReference(users)
                                 .WithReference(auditlog);

builder.AddProject<Projects.URegister>("uregister")
   .WithReference(nomenclaturescatalog)
   .WithReference(registerscatalog)
   .WithReference(objectscatalog)
   .WithReference(numbergenerator)
   .WithReference(users)
   .WithReference(integrationsCatalog);

builder.AddProject<Projects.URegister_ApiGateway>("uregister-api")
    .WithReference(registerscatalog)
    .WithReference(nomenclaturescatalog);

builder.AddProject<Projects.URegister_Admin>("uregister-admin")
    .WithReference(nomenclaturescatalog)
    .WithReference(registerscatalog)
    .WithReference(objectscatalog)
    .WithReference(users)
    .WithReference(integrationsCatalog)
    .WithReference(auditlog);

builder.Build().Run();