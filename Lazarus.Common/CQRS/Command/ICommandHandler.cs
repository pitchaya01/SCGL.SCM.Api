using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lazarus.Common.CQRS.Command
{
    public interface ICommandHandler<in TCommand> where TCommand : class, ICommand
    {
        void Handle(TCommand command);
        void Validate(TCommand command);
    }
}
