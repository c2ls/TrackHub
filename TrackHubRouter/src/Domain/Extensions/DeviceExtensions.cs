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

using System.Text;

namespace TrackHub.Router.Domain.Extensions;
public static class DeviceExtensions
{
    public static string GetIdsQueryString(this IEnumerable<DeviceTransporterVm> devices)
    {
        var stringBuilder = new StringBuilder();
        bool isFirst = true;
        foreach (var device in devices)
        {
            if (!isFirst)
            {
                stringBuilder.Append('&');
            }
            stringBuilder.Append("id=").Append(device.Identifier);
            isFirst = false;
        }
        return stringBuilder.ToString();
    }

    public static string GetIdsQueryString(this IEnumerable<int> ids)
    {
        var stringBuilder = new StringBuilder();
        bool isFirst = true;
        foreach (var id in ids.Where(x => x != 0))
        {
            if (!isFirst)
            {
                stringBuilder.Append('&');
            }
            stringBuilder.Append("id=").Append(id);
            isFirst = false;
        }
        return stringBuilder.ToString();
    }

}
