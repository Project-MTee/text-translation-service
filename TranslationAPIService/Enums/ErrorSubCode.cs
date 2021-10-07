
namespace Tilde.MT.TranslationAPIService.Enums
{
    public enum ErrorSubCode
    {
        /*#region Domain detection errors

        GatewayDomainDetectionTimedOut = 1,
        GatewayDomainDetectionGeneric = 2,

        #endregion*/

        #region Translation errors

        GatewayTranslationTimedOut = 3,
        GatewayTranslationGeneric = 4,
        WorkerTranslationGeneric = 5,

        #endregion

        #region Language direction errors

        GatewayLanguageDirectionNotFound = 6,
        GatewayLanguageDirectionGeneric = 7

        #endregion
    }
}
