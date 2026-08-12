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

namespace TrackHub.Security.Application.Users.Events;

public sealed class UserUpdated
{
    // Define a record struct for the UserUpdated notification
    public readonly record struct Notification(Guid Id, UpdateUserShrankDto User) : INotification
    {
        // Define an event handler for the UserUpdated notification
        public class EventHandler(IManagerWriter managerWriter) : INotificationHandler<Notification>
        {
            // Handle the UserUpdated notification by updating the user asynchronously
            public async Task Handle(Notification notification, CancellationToken cancellationToken)
                => await managerWriter.UpdateUserAsync(notification.Id, notification.User, cancellationToken);
        }
    }
}
