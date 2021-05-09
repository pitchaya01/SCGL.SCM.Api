using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SCGL.SCM.User.Api.Application.Query
{
    public class SearchUserCommandHandler : IRequestHandler<SearchUserCommand>
    {
        public Task<Unit> Handle(SearchUserCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
