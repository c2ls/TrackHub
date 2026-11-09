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


namespace TrackHub.Manager.Application.Staff.Commands;

[Authorize(Resource = Resources.Groups, Action = Actions.Write)]
// Enforcement: StaffGroupWriter checks that the staff member and the group share one account the caller may write.
[AccountScopeEnforcedInHandler]
public readonly record struct CreateStaffGroupCommand(StaffGroupDto StaffGroup) : IRequest<StaffGroupVm>;
public class CreateStaffGroupCommandHandler(IStaffGroupWriter writer) : IRequestHandler<CreateStaffGroupCommand, StaffGroupVm>
{
    public async Task<StaffGroupVm> Handle(CreateStaffGroupCommand request, CancellationToken cancellationToken)
        => await writer.CreateStaffGroupAsync(request.StaffGroup, cancellationToken);
}
public sealed class CreateStaffGroupCommandValidator : AbstractValidator<CreateStaffGroupCommand>
{
    public CreateStaffGroupCommandValidator()
    {
        RuleFor(v => v.StaffGroup.StaffId).NotEmpty();
        RuleFor(v => v.StaffGroup.GroupId).NotEmpty();
    }
}

[Authorize(Resource = Resources.Groups, Action = Actions.Delete)]
// Enforcement: StaffGroupWriter checks that the staff member and the group share one account the caller may write.
[AccountScopeEnforcedInHandler]
public readonly record struct DeleteStaffGroupCommand(Guid StaffId, long GroupId) : IRequest;
public class DeleteStaffGroupCommandHandler(IStaffGroupWriter writer) : IRequestHandler<DeleteStaffGroupCommand>
{
    public async Task Handle(DeleteStaffGroupCommand request, CancellationToken cancellationToken)
        => await writer.DeleteStaffGroupAsync(request.StaffId, request.GroupId, cancellationToken);
}
public sealed class DeleteStaffGroupCommandValidator : AbstractValidator<DeleteStaffGroupCommand>
{
    public DeleteStaffGroupCommandValidator()
    {
        RuleFor(v => v.StaffId).NotEmpty();
        RuleFor(v => v.GroupId).NotEmpty();
    }
}
