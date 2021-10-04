using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Enums
{
    public enum ErrorSubCode
    {
        #region Domain detection errors

        GatewayDomainDetectionTimedOut = 1,
        GatewayDomainDetectionGeneric = 2,

        #endregion

        #region Translation errors

        GatewayTranslationTimedOut = 3,
        GatewayTranslationGeneric = 4,
        WorkerTranslationGeneric = 5

        #endregion
    }
}
