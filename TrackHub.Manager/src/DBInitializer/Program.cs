// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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

using DBInitializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("\nInitializing Manager Database ...\n\n");
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddScoped<ApplicationDbContextInitializer>();
var app = builder.Build();
await app.Services.InitializeDatabaseAsync();

Console.WriteLine("\nFinish Initializing Manager Database. \n\nPress any key to close this window ...");
Console.ReadLine();
