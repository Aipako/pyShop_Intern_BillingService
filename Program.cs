using Billing.Services;
using Billing.Models;
using Billing.DataBase;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();
//===============================================
//              DRIVER CODE SECTION
//===============================================

List<UserProfileModel> users = new List<UserProfileModel>();
users.Add(new UserProfileModel("boris", 5000));
users.Add(new UserProfileModel("maria", 1000));
users.Add(new UserProfileModel("oleg", 800));
Coins.Init();
Users.Init(users);

app.MapGrpcService<BillingService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
