using Microsoft.AspNetCore.Authorization;

namespace Arshatid.Authorization
{
    public class DevelopmentUserHandler : AuthorizationHandler<DevelopmentUserRequirement>
    {
        private readonly DevelopmentUserService _service;

        public DevelopmentUserHandler(DevelopmentUserService service)
        {
            _service = service;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DevelopmentUserRequirement requirement)
        {
            if (_service.IsDevelopmentUser(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
