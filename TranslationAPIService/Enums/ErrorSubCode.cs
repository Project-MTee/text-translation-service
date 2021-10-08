
using System.ComponentModel;

namespace Tilde.MT.TranslationAPIService.Enums
{
    public enum ErrorSubCode
    {
        /*#region Domain detection errors

        GatewayDomainDetectionTimedOut = 1,
        GatewayDomainDetectionGeneric = 2,

        #endregion*/

        #region Translation errors

        [Description("Translation timed out")]
        GatewayTranslationTimedOut = 3,

        [Description("Translation failed due to unkown reason")]
        GatewayTranslationGeneric = 4,

        [Description("")]
        WorkerTranslationGeneric = 5,

        #endregion

        #region Language direction errors

        [Description("Language direction is not found")]
        GatewayLanguageDirectionNotFound = 6,

        [Description("Failed to verify language direction")]
        GatewayLanguageDirectionGeneric = 7,

        #endregion

        [Description("An unexpected error occured")]
        GatewayGeneric = 8,

        [Description("Request too large")]
        GatewayRequestTooLarge = 9,

        [Description("Request validation failed")]
        GatewayRequestValidation = 10
    }
}
