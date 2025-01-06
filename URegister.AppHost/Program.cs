var builder = DistributedApplication.CreateBuilder(args);
var nomenclaturescatalog = builder.AddProject<Projects.URegister_NomenclaturesCatalog>("uregister-nomenclaturescatalog");
var registerscatalog = builder.AddProject<Projects.URegister_RegistersCatalog>("uregister-registerscatalog")
                            .WithReference(nomenclaturescatalog);
var objectscatalog = builder.AddProject<Projects.URegister_ObjectsCatalog>("uregister-objectscatalog");
var numbergenerator = builder.AddProject<Projects.URegister_NumberGenerator>("uregister-numbergenerator");
var users = builder.AddProject<Projects.Uregister_Users>("uregister-users");
builder.AddProject<Projects.URegister>("uregister")
   .WithReference(nomenclaturescatalog)
   .WithReference(registerscatalog)
   .WithReference(objectscatalog)
   .WithReference(numbergenerator)
   .WithReference(users); 

builder.AddProject<Projects.URegister_ApiGateway>("uregister-api");

builder.AddProject<Projects.URegister_Admin>("uregister-admin")
    .WithReference(nomenclaturescatalog)
    .WithReference(registerscatalog)
    .WithReference(objectscatalog)
    .WithReference(users);



builder.AddProject<Projects.URegister_IntegrationsCatalog>("uregister-integrationscatalog");











builder.Build().Run();
