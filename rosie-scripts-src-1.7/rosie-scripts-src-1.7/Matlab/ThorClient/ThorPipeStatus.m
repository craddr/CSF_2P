classdef ThorPipeStatus < uint32
    enumeration
        ThorPipeStsNoError (0),
        ThorPipeStsBusy (1),
        ThorPipeStsBlankCommandError (2),
        ThorPipeStreamNotSupportedError (3),
        ThorPipeFormatError (10),
        ThorPipeFormatRoutingError (11),
        ThorpipeIOError (20),
        ThorPipeError (99),
    end
end

