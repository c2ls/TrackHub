// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using TrackHub.AuthorityServer.ClientSeeder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TrackHub.AuthorityServer.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

Console.WriteLine("\nSeeding clients...\n");
builder.Services.AddOpenIdDictDbContext(builder.Configuration);
builder.Services.AddOpenIddict()
        .AddCore(options => options.UseEntityFrameworkCore()
                .UseDbContext<AuthorityDbContext>()
                .ReplaceDefaultEntities<long>());

builder.Services.AddScoped<Seeder>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
    await seeder.StartAsync(CancellationToken.None);
}

Console.WriteLine("\nFinish Seeding Client. Don't forget to restart the Authority Service. \n\nPress any key to close this window ...");
Console.ReadLine();
