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

using Microsoft.JSInterop;

namespace TrackHubMobile.Pages;

public partial class TransporterMap(IJSRuntime JS)
{
    private IEnumerable<Position> positions =
            [
                new Position { Lat = 12.34, Lng = 56.78 },
                new Position { Lat = 90.12, Lng = 45.67 }
            ];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("initMap", positions);
        }
    }
}

public class Position
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}