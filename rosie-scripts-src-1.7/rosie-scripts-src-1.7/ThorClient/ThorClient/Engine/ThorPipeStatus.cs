using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThorClient.Engine
{
    public enum ThorPipeStatus
    {
        ThorPipeStsNoError = 0,
        ThorPipeStsBusy = 1,
        ThorPipeStsBlankCommandError = 2,
        ThorPipeStreamNotSupportedError = 3,
        ThorPipeFormatError = 10,
        ThorPipeFormatRoutingError = 11,
        ThorpipeIOError = 20,
        ThorPipeError = 99,
    }
}
