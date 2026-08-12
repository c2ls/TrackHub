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

namespace Common.Domain.Constants;

public static class ColumnMetadata
{
    public const int DefaultFieldLength = 100;
    public const int DefaultNameLength = 200;
    public const int DefaultDescriptionLength = 500;
    public const int DefaultUserNameLength = 200;
    public const int DefaultEmailLength = 200;
    public const int DefaultTokenLength = 200;
    public const int DefaultPasswordLength = 200;
    public const int DefaultAddressLength = 250;
    public const int DefaultPhoneNumberLength = 200;
    public const int MinimumPasswordLength = 8;

    public const string TextField = "text";
}
