using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThorClient.Engine
{
    public enum ThorPipeCommand
    {
        Establish,
        TearDown,
        AcquireInformation,
        UpdataInformation,
        FilePath,
        StartAcquiring,
        StopAcquiring,
        StartBleach,
        StopBleach,
        Receive,
        Error,
        ChangeRemotePC,
        ChangeRemoteApp,
    }
}
